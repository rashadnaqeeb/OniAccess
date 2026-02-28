using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Notifications {
	/// <summary>
	/// Tracks active notifications by subscribing to NotificationManager events.
	/// Maintains a grouped, sorted list of live Notification object references.
	/// Shared between NotificationAnnouncer (live speech) and NotificationMenuHandler
	/// (browsable menu).
	///
	/// Stores only references to live Notification objects — never caches string
	/// values. All properties are read from the Notification at access time.
	/// </summary>
	internal sealed class NotificationTracker {
		private readonly List<Notification> _notifications = new List<Notification>();
		private readonly List<NotificationGroup> _groups = new List<NotificationGroup>();

		internal event System.Action OnChanged;

		/// <summary>
		/// True when at least one notification arrived since the last ClearNew() call.
		/// The announcer reads this to distinguish new arrivals from removals.
		/// </summary>
		internal bool HasNew { get; private set; }

		internal void ClearNew() => HasNew = false;

		internal IReadOnlyList<Notification> Notifications => _notifications;
		internal IReadOnlyList<NotificationGroup> Groups => _groups;

		/// <summary>
		/// Subscribe to NotificationManager events and seed from existing notifications.
		/// Existing notifications are added silently (HasNew stays false, OnChanged
		/// does not fire) — they predate the player's session context.
		/// </summary>
		internal void Attach() {
			if (NotificationManager.Instance == null) {
				Util.Log.Warn("NotificationTracker.Attach: NotificationManager.Instance is null");
				return;
			}
			NotificationManager.Instance.notificationAdded += OnNotificationAdded;
			NotificationManager.Instance.notificationRemoved += OnNotificationRemoved;

			// Seed from existing active notifications (e.g., restored from save).
			// The notifications field is private; use Traverse to read it.
			try {
				var existing = HarmonyLib.Traverse.Create(NotificationManager.Instance)
					.Field<List<Notification>>("notifications").Value;
				if (existing != null) {
					for (int i = 0; i < existing.Count; i++)
						_notifications.Add(existing[i]);
					SortAndRegroup();
				}
			} catch (Exception ex) {
				Util.Log.Error($"NotificationTracker.Attach: failed to seed existing notifications: {ex}");
			}
		}

		internal void Detach() {
			if (NotificationManager.Instance != null) {
				NotificationManager.Instance.notificationAdded -= OnNotificationAdded;
				NotificationManager.Instance.notificationRemoved -= OnNotificationRemoved;
			}
			_notifications.Clear();
			_groups.Clear();
		}

		private void OnNotificationAdded(Notification n) {
			_notifications.Add(n);
			HasNew = true;
			SortAndRegroup();
			OnChanged?.Invoke();
		}

		private void OnNotificationRemoved(Notification n) {
			_notifications.Remove(n);
			SortAndRegroup();
			OnChanged?.Invoke();
		}

		/// <summary>
		/// Sort notifications by (Type, Idx) matching game behavior, then rebuild
		/// the grouped view. Groups are ordered by their first member's sort position.
		/// </summary>
		private void SortAndRegroup() {
			_notifications.Sort((a, b) =>
				a.Type != b.Type ? (int)a.Type - (int)b.Type : a.Idx - b.Idx);

			_groups.Clear();
			// Build groups preserving sort order. Use a dictionary to find existing
			// groups by titleText, but the groups list maintains insertion order
			// (first occurrence order after sorting).
			var groupIndex = new Dictionary<string, int>();
			for (int i = 0; i < _notifications.Count; i++) {
				var n = _notifications[i];
				string key = n.titleText;
				if (groupIndex.TryGetValue(key, out int idx)) {
					_groups[idx].Members.Add(n);
				} else {
					groupIndex[key] = _groups.Count;
					var group = new NotificationGroup(key);
					group.Members.Add(n);
					_groups.Add(group);
				}
			}
		}
	}

	/// <summary>
	/// A group of notifications sharing the same titleText.
	/// Members is a live list rebuilt on every tracker change; it contains
	/// references to live Notification objects.
	/// </summary>
	internal sealed class NotificationGroup {
		internal string TitleText { get; }
		internal List<Notification> Members { get; } = new List<Notification>();
		internal int Count => Members.Count;

		internal NotificationGroup(string titleText) {
			TitleText = titleText;
		}

		/// <summary>
		/// Evaluate the tooltip delegate for this group's representative notification.
		/// Returns null if no tooltip is available.
		/// </summary>
		internal string GetTooltipText() {
			if (Members.Count == 0) return null;
			var rep = Members[0];
			if (rep.ToolTip == null) return null;
			try {
				string raw = rep.ToolTip(Members, rep.tooltipData);
				return Widgets.WidgetOps.CleanTooltipEntry(raw);
			} catch (Exception ex) {
				Util.Log.Warn($"NotificationGroup.GetTooltipText failed for '{TitleText}': {ex.Message}");
				return null;
			}
		}
	}
}
