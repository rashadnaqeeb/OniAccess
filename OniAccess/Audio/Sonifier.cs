using System;
using FMOD;
using FMODUnity;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Audio {
	public class Sonifier: MonoBehaviour {
		internal static Sonifier Instance { get; private set; }

		// Base pitch: C4 (middle C). Change these to shift the octave.
		const float BaseFrequencyHz = 262f;
		const float OctaveRatio = 2.0f;

		// Volume mapping
		const float SilenceDb = -60f;
		const float MaxVolume = 0.05f;
		const float VolumeFadeSeconds = 0.1f;

		// Fade ramp to prevent click artifacts on start/stop
		const float FadeSeconds = 0.01f;

		// ~100ms window at 60fps
		const int PresenceBufferSize = 6;

		// Ring buffer for running average (presence samples)
		private float[] _presenceBuffer;
		private int _presenceIndex;
		private int _presenceCount;

		private Sound _toneSound;
		private Channel _channel;
		private bool _playing;

		// Fade state
		private float _fadeGain;
		private float _fadeTarget;
		private float _smoothedVolume;

		private void Awake() {
			Instance = this;
			_toneSound = ToneGenerator.CreateLoopingSineWave(BaseFrequencyHz, 1.0f);

			_presenceBuffer = new float[PresenceBufferSize];
		}

		private void OnDestroy() {
			Stop();
			if (_toneSound.hasHandle())
				_toneSound.release();
			if (Instance == this)
				Instance = null;
		}

		private void Update() {
			SonifierController.Instance.Tick();

			if (!_playing) return;

			// Advance fade
			if (_fadeGain != _fadeTarget) {
				float fadeStep = Time.unscaledDeltaTime / FadeSeconds;
				_fadeGain = Mathf.MoveTowards(_fadeGain, _fadeTarget, fadeStep);
				if (_fadeGain <= 0f && _fadeTarget <= 0f) {
					StopChannel();
					return;
				}
			}

			// Smooth volume toward target to prevent artifacts
			if (_channel.hasHandle()) {
				float targetVolume = ComputeVolume();
				float fadeStep = Time.unscaledDeltaTime / VolumeFadeSeconds;
				_smoothedVolume = Mathf.MoveTowards(_smoothedVolume, targetVolume, fadeStep);
				_channel.setVolume(_smoothedVolume * _fadeGain);
			}
		}

		public void UpdateTone(float fillRatio, bool hasContents) {
			if (!_playing)
				StartChannel();

			RecordPresence(hasContents ? 1f : 0f);

			if (_channel.hasHandle()) {
				float pitch = Mathf.Pow(OctaveRatio, fillRatio);
				_channel.setPitch(pitch);
			}
		}

		public void Stop() {
			if (!_playing) return;
			_fadeTarget = 0f;
			// If fade is already at zero, stop immediately
			if (_fadeGain <= 0f)
				StopChannel();
		}

		private void StartChannel() {
			if (!_toneSound.hasHandle()) return;
			var result = RuntimeManager.CoreSystem.playSound(
				_toneSound, default(ChannelGroup), true, out _channel);
			if (result != RESULT.OK) {
				Log.Warn($"Sonifier: playSound failed: {result}");
				return;
			}
			_channel.setVolume(0f);
			_channel.setPitch(1f);
			_channel.setPaused(false);
			_playing = true;
			_fadeGain = 0f;
			_fadeTarget = 1f;
			ClearPresenceBuffer();
		}

		private void StopChannel() {
			if (_channel.hasHandle())
				_channel.stop();
			_channel = default;
			_playing = false;
			_fadeGain = 0f;
			_fadeTarget = 0f;
			_smoothedVolume = 0f;
		}

		private void RecordPresence(float value) {
			if (_presenceIndex >= _presenceBuffer.Length)
				_presenceIndex = 0;
			_presenceBuffer[_presenceIndex] = value;
			_presenceIndex++;
			if (_presenceCount < _presenceBuffer.Length)
				_presenceCount++;
		}

		private void ClearPresenceBuffer() {
			Array.Clear(_presenceBuffer, 0, _presenceBuffer.Length);
			_presenceIndex = 0;
			_presenceCount = 0;
		}

		private float ComputeVolume() {
			if (_presenceCount == 0) return 0f;

			float sum = 0f;
			for (int i = 0; i < _presenceCount; i++)
				sum += _presenceBuffer[i];
			float average = sum / _presenceCount;

			// Map average to dB: at average=1 → 0dB, at average=0 → SilenceDb
			float gainDb = SilenceDb * (1f - average);
			return Mathf.Pow(10f, gainDb / 20f) * MaxVolume;
		}
	}
}
