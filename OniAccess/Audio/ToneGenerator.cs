using System;
using System.Runtime.InteropServices;
using FMOD;
using FMODUnity;
using OniAccess.Util;

namespace OniAccess.Audio {
	public static class ToneGenerator {
		private const int SampleRate = 48000;
		private const int ChannelCount = 1;

		public static Sound CreateLoopingSineWave(float frequencyHz, float durationSeconds) {
			int sampleCount = (int)(SampleRate * durationSeconds);
			uint byteLength = (uint)(sampleCount * sizeof(float));

			var exinfo = new CREATESOUNDEXINFO();
			exinfo.cbsize = Marshal.SizeOf(exinfo);
			exinfo.length = byteLength;
			exinfo.numchannels = ChannelCount;
			exinfo.defaultfrequency = SampleRate;
			exinfo.format = SOUND_FORMAT.PCMFLOAT;

			var result = RuntimeManager.CoreSystem.createSound(
				(string)null, MODE.OPENRAW | MODE.LOOP_NORMAL | MODE.OPENUSER,
				ref exinfo, out var sound);
			if (result != RESULT.OK) {
				Log.Error($"ToneGenerator: createSound failed: {result}");
				return default;
			}

			result = sound.@lock(0, byteLength, out var ptr1, out var ptr2,
				out var len1, out var len2);
			if (result != RESULT.OK) {
				Log.Error($"ToneGenerator: lock failed: {result}");
				sound.release();
				return default;
			}

			var samples = new float[sampleCount];
			for (int i = 0; i < sampleCount; i++) {
				float t = (float)i / SampleRate;
				samples[i] = (float)Math.Sin(2.0 * Math.PI * frequencyHz * t);
			}
			Marshal.Copy(samples, 0, ptr1, (int)(len1 / sizeof(float)));
			if (ptr2 != IntPtr.Zero && len2 > 0)
				Marshal.Copy(samples, (int)(len1 / sizeof(float)), ptr2,
					(int)(len2 / sizeof(float)));

			result = sound.unlock(ptr1, ptr2, len1, len2);
			if (result != RESULT.OK) {
				Log.Error($"ToneGenerator: unlock failed: {result}");
				sound.release();
				return default;
			}
			sound.setLoopCount(-1);
			return sound;
		}
	}
}
