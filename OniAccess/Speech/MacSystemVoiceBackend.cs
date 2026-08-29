using System;
using System.Collections.Generic;
using OniAccess.Util;

namespace OniAccess.Speech {
	/// <summary>
	/// The macOS system voice, spoken by the mod itself rather than through
	/// Prism. <see cref="MacSpeechStream"/> renders each message through AVSpeech
	/// and plays the audio back-to-back through an engine of the mod's own,
	/// because AVSpeech's own queue leaves well over 100 ms of silence between
	/// utterances, which a screen reader user hears as speech stopping and
	/// restarting on every line. AVSpeech is only spoken through directly if
	/// that stream cannot be set up. Voice, rate, and volume come from
	/// <see cref="SpeechOutputSelector"/>, on AVSpeech's own [0, 1] scales.
	/// Only constructed on macOS; every call happens on the game's main thread.
	/// </summary>
	public class MacSystemVoiceBackend: ISpeechBackend, IVoiceControl {
		const int BoundaryImmediate = 0; // AVSpeechBoundaryImmediate

		// The name is AVSpeech's own, which already carries "(Enhanced)" or
		// "(Premium)" for a downloaded voice quality, as System Settings shows it.
		private class Voice {
			public string Identifier;
			public string Name;
			public string Language;
		}

		private IntPtr _synth = IntPtr.Zero;  // owned (+1): AVSpeechSynthesizer for the fallback; also marks the backend as available
		private MacSpeechStream _stream;
		private readonly List<Voice> _voices = new List<Voice>();
		private int _current = -1;            // index into _voices; -1 speaks AVSpeech's default voice
		private float _rate = 0.5f;
		private float _volume = 1f;
		private bool _initialized;

		public bool IsInitialized => _initialized;
		public bool IsAvailable => _synth != IntPtr.Zero;

		/// <summary>True while messages go through the gapless stream rather than AVSpeech's own queue.</summary>
		public bool IsStreaming => _stream != null;

		public bool Initialize() {
			if (_initialized) return IsAvailable;
			_initialized = true;
			try {
				ObjC.LoadSpeechFrameworks();
				_synth = ObjC.Send(ObjC.Send(ObjC.Class("AVSpeechSynthesizer"), ObjC.Sel("alloc")), ObjC.Sel("init"));
				if (_synth == IntPtr.Zero) {
					Log.Error("Mac system voice: AVSpeechSynthesizer init returned nil");
					return false;
				}
				ReadVoices();
				CreateStream();
				Log.Info($"Mac system voice initialized: {_voices.Count} voices, {(IsStreaming ? "streamed" : "AVSpeech queue")}");
				return true;
			} catch (Exception ex) {
				Log.Error($"Mac system voice failed to initialize: {ex}");
				DropStream();
				ReleaseSynth();
				return false;
			}
		}

		/// <summary>Set up the streaming queue. If that fails, messages go to AVSpeech's own queue instead.</summary>
		private void CreateStream() {
			DropStream();
			try {
				_stream = new MacSpeechStream();
				_stream.SetVolume(_volume);
			} catch (Exception ex) {
				Log.Error($"Mac system voice: streamed queue unavailable, speaking through AVSpeech's own queue with gaps between lines: {ex}");
				DropStream();
			}
		}

		private void DropStream() {
			_stream?.Dispose();
			_stream = null;
		}

		/// <summary>Every installed AVSpeech voice.</summary>
		private void ReadVoices() {
			_voices.Clear();
			IntPtr pool = ObjC.AutoreleasePoolPush();
			try {
				IntPtr all = ObjC.Send(ObjC.Class("AVSpeechSynthesisVoice"), ObjC.Sel("speechVoices"));
				long count = ObjC.Count(all);
				for (long i = 0; i < count; i++) {
					IntPtr voice = ObjC.Send(all, ObjC.Sel("objectAtIndex:"), (IntPtr)i);
					string id = ObjC.ToManagedString(ObjC.Send(voice, ObjC.Sel("identifier")));
					string name = ObjC.ToManagedString(ObjC.Send(voice, ObjC.Sel("name")));
					if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name)) continue;
					string language = ObjC.ToManagedString(ObjC.Send(voice, ObjC.Sel("language")));
					_voices.Add(new Voice { Identifier = id, Name = name, Language = string.IsNullOrEmpty(language) ? null : language });
				}
			} finally {
				ObjC.AutoreleasePoolPop(pool);
			}
		}

		public void Shutdown() {
			if (!_initialized) return;
			DropStream();
			ReleaseSynth();
			_voices.Clear();
			_current = -1;
			_initialized = false;
			Log.Info("Mac system voice shut down");
		}

		private void ReleaseSynth() {
			if (_synth == IntPtr.Zero) return;
			try {
				ObjC.Send(_synth, ObjC.Sel("stopSpeakingAtBoundary:"), (IntPtr)BoundaryImmediate);
				ObjC.Release(_synth);
			} catch (Exception ex) {
				Log.Warn($"Mac system voice: release failed: {ex.Message}");
			}
			_synth = IntPtr.Zero;
		}

		public void Say(string text, bool interrupt) {
			if (!IsAvailable || string.IsNullOrEmpty(text)) return;
			try {
				if (interrupt) Stop();
				if (_stream == null) {
					Utter(text);
				} else if (!_stream.Enqueue(text, CurrentIdentifier, _rate)) {
					Log.Error("Mac system voice: the streamed queue can no longer render; speaking through AVSpeech's own queue from now on");
					DropStream();
					Utter(text);
				}
			} catch (Exception ex) {
				Log.Error($"Mac system voice: speak failed: {ex}");
			}
		}

		public void Stop() {
			if (!IsAvailable) return;
			try {
				if (_stream != null)
					_stream.Clear();
				else
					ObjC.Send(_synth, ObjC.Sel("stopSpeakingAtBoundary:"), (IntPtr)BoundaryImmediate);
			} catch (Exception ex) {
				Log.Error($"Mac system voice: stop failed: {ex}");
			}
		}

		/// <summary>Once per frame: drive the streamed queue.</summary>
		public void Update() {
			_stream?.Update();
		}

		/// <summary>Speak through AVSpeech's own queue: the fallback when the stream could not be set up.</summary>
		private void Utter(string text) {
			IntPtr pool = ObjC.AutoreleasePoolPush();
			try {
				IntPtr voice = CurrentIdentifier == null ? IntPtr.Zero : MacSpeechStream.LookupVoice(CurrentIdentifier);
				IntPtr utterance = MacSpeechStream.MakeUtterance(text, voice, _rate);
				if (utterance == IntPtr.Zero) {
					Log.Error("Mac system voice: AVSpeechUtterance returned nil");
					return;
				}
				ObjC.SendFloat(utterance, ObjC.Sel("setVolume:"), _volume);
				ObjC.Send(_synth, ObjC.Sel("speakUtterance:"), utterance);
			} finally {
				ObjC.AutoreleasePoolPop(pool);
			}
		}

		// ---- IVoiceControl ----

		private string CurrentIdentifier => _current >= 0 ? _voices[_current].Identifier : null;

		public int VoiceCount => _voices.Count;

		public string GetVoiceName(int index) => index >= 0 && index < _voices.Count ? _voices[index].Name : null;

		public string GetVoiceLanguage(int index) => index >= 0 && index < _voices.Count ? _voices[index].Language : null;

		public string GetVoiceIdentifier(int index) => index >= 0 && index < _voices.Count ? _voices[index].Identifier : null;

		public int CurrentVoice => _current;

		/// <summary>Switch later messages to a voice (-1 for AVSpeech's default); ones already queued keep the voice they were queued with.</summary>
		public bool SetVoice(int index) {
			if (index < -1 || index >= _voices.Count) {
				Log.Warn($"Mac system voice: no voice at index {index}");
				return false;
			}
			_current = index;
			return true;
		}

		public bool SetRate(float rate) {
			if (rate < 0f || rate > 1f) {
				Log.Warn($"Mac system voice: rate {rate} is outside [0, 1]");
				return false;
			}
			_rate = rate;
			return true;
		}

		public float GetRate() => _rate;

		public bool SetVolume(float volume) {
			if (volume < 0f || volume > 1f) {
				Log.Warn($"Mac system voice: volume {volume} is outside [0, 1]");
				return false;
			}
			_volume = volume;
			_stream?.SetVolume(volume);
			return true;
		}
	}
}
