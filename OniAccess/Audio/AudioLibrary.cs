using System.Collections;
using System.Collections.Generic;
using System.IO;
using OniAccess.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace OniAccess.Audio {
	public class AudioLibrary {
		private readonly Dictionary<string, AudioClip> _clips
			= new Dictionary<string, AudioClip>();

		public bool LoadComplete { get; private set; }

		public IEnumerator Load(string audioDir) {
			if (!Directory.Exists(audioDir)) {
				Log.Warn($"AudioLibrary: audio directory not found: {audioDir}");
				LoadComplete = true;
				yield break;
			}

			string[] files = Directory.GetFiles(audioDir, "*.ogg");
			foreach (string file in files) {
				string uri = "file:///" + file.Replace('\\', '/');
				using (var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS)) {
					yield return request.SendWebRequest();
					if (request.result != UnityWebRequest.Result.Success) {
						Log.Warn($"AudioLibrary: failed to load {file}: {request.error}");
						continue;
					}
					var clip = DownloadHandlerAudioClip.GetContent(request);
					string name = Path.GetFileNameWithoutExtension(file);
					clip.name = name;
					_clips[name] = clip;
				}
			}

			LoadComplete = true;
			Log.Info($"AudioLibrary: loaded {_clips.Count} clip(s) from {audioDir}");
		}

		public bool TryGet(string clipName, out AudioClip clip) {
			return _clips.TryGetValue(clipName, out clip);
		}
	}
}
