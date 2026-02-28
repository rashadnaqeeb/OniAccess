using System;
using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Notifications {
	/// <summary>
	/// Announces new notifications via SpeechPipeline with a 200ms batching window.
	/// Same-title notifications within the window collapse into "{title} x{count}".
	/// Different-title notifications queue FIFO.
	///
	/// Called from TileCursorHandler.Tick() each frame. The batching window prevents
	/// rapid-fire notifications from interrupting each other.
	/// </summary>
	internal sealed class NotificationAnnouncer {
		private const float BatchWindowSeconds = 0.2f;

		private readonly NotificationTracker _tracker;
		private bool _batchPending;
		private float _batchStart;

		/// <summary>
		/// Tracks the last-announced group counts by titleText. On flush, only groups
		/// with new or increased counts are announced. This avoids re-announcing
		/// pre-existing notifications and ensures removal events are silent.
		/// </summary>
		private readonly Dictionary<string, int> _knownCounts = new Dictionary<string, int>();

		/// <summary>
		/// Injectable time source for offline testing. Defaults to Unity unscaledTime.
		/// </summary>
		internal static Func<float> TimeSource = () => UnityEngine.Time.unscaledTime;

		internal NotificationAnnouncer(NotificationTracker tracker) {
			_tracker = tracker;
			_tracker.OnChanged += OnChanged;

			// Seed _knownCounts from the tracker's current state so pre-existing
			// notifications are never announced.
			var groups = _tracker.Groups;
			for (int i = 0; i < groups.Count; i++)
				_knownCounts[groups[i].TitleText] = groups[i].Count;
		}

		internal void Detach() {
			_tracker.OnChanged -= OnChanged;
		}

		private void OnChanged() {
			if (!_tracker.HasNew) return;
			if (!_batchPending) {
				_batchPending = true;
				_batchStart = TimeSource();
			}
		}

		internal void Tick() {
			if (!_batchPending) return;
			if (TimeSource() - _batchStart < BatchWindowSeconds) return;
			Flush();
		}

		private void Flush() {
			_batchPending = false;
			_tracker.ClearNew();

			var groups = _tracker.Groups;
			bool first = true;
			for (int i = 0; i < groups.Count; i++) {
				var group = groups[i];
				_knownCounts.TryGetValue(group.TitleText, out int knownCount);

				if (group.Count <= knownCount) {
					// No new notifications in this group
					_knownCounts[group.TitleText] = group.Count;
					continue;
				}

				string text;
				if (group.Count > 1)
					text = string.Format(
						(string)STRINGS.ONIACCESS.NOTIFICATIONS.GROUP_COUNT,
						group.TitleText, group.Count);
				else
					text = group.TitleText;

				if (first) {
					SpeechPipeline.SpeakInterrupt(text);
					first = false;
				} else {
					SpeechPipeline.SpeakQueued(text);
				}

				_knownCounts[group.TitleText] = group.Count;
			}

			// Clean up known counts for groups that no longer exist
			var toRemove = new List<string>();
			foreach (var key in _knownCounts.Keys) {
				bool found = false;
				for (int i = 0; i < groups.Count; i++) {
					if (groups[i].TitleText == key) { found = true; break; }
				}
				if (!found) toRemove.Add(key);
			}
			for (int i = 0; i < toRemove.Count; i++)
				_knownCounts.Remove(toRemove[i]);
		}
	}
}
