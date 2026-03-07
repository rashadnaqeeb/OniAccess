using System;

namespace OniAccess.Util {
	/// <summary>
	/// Gates startup speech spam from Harmony patches. During save load, the
	/// game restores state through the same code paths used for live events,
	/// producing bogus announcements. LoadGate stays closed until the player
	/// unpauses and 1 second elapses, then opens permanently until the next
	/// Reset().
	/// </summary>
	internal static class LoadGate {
		private const float DelayAfterUnpause = 1.0f;

		internal static bool IsReady { get; private set; }

		private static bool _sawUnpause;
		private static float _unpauseTime;

		/// <summary>
		/// Injectable time source for offline testing.
		/// </summary>
		internal static Func<float> TimeSource = () => UnityEngine.Time.unscaledTime;

		/// <summary>
		/// Injectable pause check for offline testing.
		/// </summary>
		internal static Func<bool> IsPaused = () =>
			SpeedControlScreen.Instance != null && SpeedControlScreen.Instance.IsPaused;

		internal static void Tick() {
			if (IsReady) return;
			if (!_sawUnpause) {
				if (IsPaused()) return;
				_sawUnpause = true;
				_unpauseTime = TimeSource();
			}
			if (TimeSource() - _unpauseTime >= DelayAfterUnpause)
				IsReady = true;
		}

		internal static void Reset() {
			IsReady = false;
			_sawUnpause = false;
			_unpauseTime = 0f;
		}
	}
}
