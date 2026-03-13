using System.Collections;
using System.Collections.Generic;
using System.IO;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Audio {
	public class EarconScheduler : MonoBehaviour {
		public static EarconScheduler Instance { get; private set; }

		private const int PoolSize = 8;
		private const float BatchDelaySeconds = 0.125f;

		private AudioLibrary _library;
		private AudioSource[] _pool;
		private bool[] _poolInUse;
		private Coroutine _activeSequence;
		private readonly List<Coroutine> _releaseCoroutines = new List<Coroutine>();

		private void Awake() {
			Instance = this;
			try {
				_library = new AudioLibrary();
				_pool = new AudioSource[PoolSize];
				_poolInUse = new bool[PoolSize];
				for (int i = 0; i < PoolSize; i++)
					_pool[i] = gameObject.AddComponent<AudioSource>();

				string audioDir = Path.Combine(Mod.ModDir, "audio");
				StartCoroutine(_library.Load(audioDir));
			} catch (System.Exception ex) {
				Log.Error($"EarconScheduler.Awake failed: {ex}");
			}
		}

		private void OnDestroy() {
			if (Instance == this) Instance = null;
		}

		public void Play(List<SoundBatch> batches) {
			CancelAll();
			if (batches.Count == 0) return;
			_activeSequence = StartCoroutine(RunSequence(batches));
		}

		public void CancelAll() {
			if (_activeSequence != null) {
				StopCoroutine(_activeSequence);
				_activeSequence = null;
			}
			foreach (var co in _releaseCoroutines)
				StopCoroutine(co);
			_releaseCoroutines.Clear();
			for (int i = 0; i < PoolSize; i++) {
				_pool[i].Stop();
				_poolInUse[i] = false;
			}
		}

		private IEnumerator RunSequence(List<SoundBatch> batches) {
			for (int i = 0; i < batches.Count; i++) {
				PlayBatch(batches[i]);
				if (i < batches.Count - 1)
					yield return new WaitForSecondsRealtime(BatchDelaySeconds);
			}
			_activeSequence = null;
		}

		private void PlayBatch(SoundBatch batch) {
			foreach (var spec in batch.Specs) {
				if (!_library.TryGet(spec.ClipName, out var clip)) {
					Log.Warn($"EarconScheduler: clip not found: {spec.ClipName}");
					continue;
				}
				int index = AcquireSource();
				if (index < 0) {
					Log.Warn("EarconScheduler: audio source pool exhausted");
					return;
				}
				var source = _pool[index];
				source.clip = clip;
				source.pitch = spec.Pitch;
				source.panStereo = spec.Pan;
				source.Play();
				_releaseCoroutines.Add(StartCoroutine(ReleaseWhenDone(index)));
			}
		}

		private int AcquireSource() {
			for (int i = 0; i < PoolSize; i++) {
				if (!_poolInUse[i]) {
					_poolInUse[i] = true;
					return i;
				}
			}
			return -1;
		}

		private IEnumerator ReleaseWhenDone(int index) {
			while (_pool[index].isPlaying)
				yield return null;
			_poolInUse[index] = false;
		}
	}
}
