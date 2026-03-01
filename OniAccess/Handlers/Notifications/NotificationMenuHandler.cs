using System.Collections.Generic;

using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Notifications {
	/// <summary>
	/// Browsable notification menu opened by Shift+N from the tile cursor.
	/// Screenless popup extending BaseMenuHandler (like LinkMenuHandler).
	///
	/// Reads live Notification object references from the shared NotificationTracker.
	/// Groups by titleText, sorted by (Type, Idx). Supports Enter to activate,
	/// Delete to dismiss, and Escape to close.
	/// </summary>
	internal sealed class NotificationMenuHandler: BaseMenuHandler {
		private readonly NotificationTracker _tracker;

		internal NotificationMenuHandler(NotificationTracker tracker) : base(screen: null) {
			_tracker = tracker;
		}

		public override string DisplayName =>
			(string)STRINGS.ONIACCESS.NOTIFICATIONS.MENU_TITLE;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry>(MenuHelpEntries) {
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
				new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
				new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
				new HelpEntry("Delete", STRINGS.ONIACCESS.NOTIFICATIONS.DISMISS_HELP),
			}.AsReadOnly();

		public override int ItemCount => _tracker.Groups.Count;

		public override string GetItemLabel(int index) {
			var groups = _tracker.Groups;
			if (index < 0 || index >= groups.Count) return null;
			var group = groups[index];
			string label = group.TitleText;
			if (group.Count > 1)
				label = string.Format(
					(string)STRINGS.ONIACCESS.NOTIFICATIONS.GROUP_COUNT,
					label, group.Count);
			return label;
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			var groups = _tracker.Groups;
			if (_currentIndex < 0 || _currentIndex >= groups.Count) return;
			var group = groups[_currentIndex];

			string label = GetItemLabel(_currentIndex);
			string tooltip = group.GetTooltipText();
			if (!string.IsNullOrEmpty(tooltip))
				label = label + ". " + tooltip;

			if (!string.IsNullOrEmpty(parentContext))
				label = parentContext + ", " + label;

			SpeechPipeline.SpeakInterrupt(label);
		}

		public override void OnActivate() {
			base.OnActivate();
			_tracker.OnChanged += OnTrackerChanged;
			if (ItemCount > 0)
				SpeechPipeline.SpeakQueued(BuildCurrentLabel());
			else
				SpeechPipeline.SpeakQueued((string)STRINGS.ONIACCESS.NOTIFICATIONS.EMPTY);
		}

		public override void OnDeactivate() {
			_tracker.OnChanged -= OnTrackerChanged;
			base.OnDeactivate();
		}

		private void OnTrackerChanged() {
			if (ItemCount == 0) {
				_currentIndex = 0;
				return;
			}
			if (_currentIndex >= ItemCount)
				_currentIndex = ItemCount - 1;
		}

		public override bool Tick() {
			if (ItemCount == 0) {
				// All notifications expired while menu was open
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape)
					|| UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
					HandlerStack.Pop();
					return true;
				}
				return false;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Delete)
				&& !InputUtil.AnyModifierHeld()) {
				DismissCurrent();
				return true;
			}

			return base.Tick();
		}

		protected override void ActivateCurrentItem() {
			var groups = _tracker.Groups;
			if (_currentIndex < 0 || _currentIndex >= groups.Count) return;
			var group = groups[_currentIndex];

			if (group.Count == 1) {
				PlayOpenSound();
				HandlerStack.Pop();
				NotificationActivator.Activate(group.Members[0]);
			} else {
				PlayOpenSound();
				HandlerStack.Push(new NotificationSubmenuHandler(
					_tracker, group.TitleText));
			}
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				HandlerStack.Pop();
				return true;
			}
			return base.HandleKeyDown(e);
		}

		private void DismissCurrent() {
			var groups = _tracker.Groups;
			if (_currentIndex < 0 || _currentIndex >= groups.Count) return;
			var group = groups[_currentIndex];
			if (group.Count == 0) return;

			if (!group.Members[0].showDismissButton) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.NOTIFICATIONS.CANNOT_DISMISS);
				return;
			}

			NotificationActivator.DismissGroup(group);

			// Cursor clamping is handled by OnTrackerChanged via the remove events.
			if (ItemCount > 0) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.NOTIFICATIONS.DISMISSED);
				SpeechPipeline.SpeakQueued(BuildCurrentLabel());
			} else {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.NOTIFICATIONS.EMPTY);
			}
		}

		private string BuildCurrentLabel() {
			var groups = _tracker.Groups;
			if (_currentIndex < 0 || _currentIndex >= groups.Count) return null;
			var group = groups[_currentIndex];
			string label = GetItemLabel(_currentIndex);
			string tooltip = group.GetTooltipText();
			if (!string.IsNullOrEmpty(tooltip))
				label = label + ". " + tooltip;
			return label;
		}
	}
}
