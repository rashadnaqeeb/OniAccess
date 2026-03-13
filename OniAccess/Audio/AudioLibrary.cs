using System;
using System.Collections.Generic;
using System.IO;
using FMOD;
using FMODUnity;
using OniAccess.Util;

namespace OniAccess.Audio {
	public class AudioLibrary {
		private readonly Dictionary<string, Sound> _sounds
			= new Dictionary<string, Sound>();

		public void Load(string audioDir) {
			if (!Directory.Exists(audioDir)) {
				Log.Warn($"AudioLibrary: audio directory not found: {audioDir}");
				return;
			}

			string[] files = Directory.GetFiles(audioDir, "*.ogg");
			foreach (string file in files) {
				try {
					var result = RuntimeManager.CoreSystem.createSound(
						file, MODE.DEFAULT, out var sound);
					if (result != RESULT.OK) {
						Log.Warn($"AudioLibrary: FMOD createSound failed for {file}: {result}");
						continue;
					}
					string name = Path.GetFileNameWithoutExtension(file);
					_sounds[name] = sound;
				} catch (Exception ex) {
					Log.Warn($"AudioLibrary: failed to load {file}: {ex}");
				}
			}

			Log.Info($"AudioLibrary: loaded {_sounds.Count} sound(s) from {audioDir}");
		}

		public bool TryGet(string clipName, out Sound sound) {
			return _sounds.TryGetValue(clipName, out sound);
		}

		public void ReleaseAll() {
			foreach (var sound in _sounds.Values)
				sound.release();
			_sounds.Clear();
		}
	}
}
