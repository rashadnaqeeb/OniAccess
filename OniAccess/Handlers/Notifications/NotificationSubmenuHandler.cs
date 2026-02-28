using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Notifications {
	/// <summary>
	/// Submenu listing individual notifications within a grouped entry.
	/// Pushed by NotificationMenuHandler when Enter is pressed on a group.
	///
	/// Reads live from the tracker's current group matching the title key.
	/// If the group disappears (all members removed), auto-pops.
	/// </summary>
	internal sealed class NotificationSubmenuHandler : BaseMenuHandler {
		private readonly NotificationTracker _tracker;
		private readonly string _titleKey;

		internal NotificationSubmenuHandler(
			NotificationTracker tracker, string titleKey) : base(screen: null) {
			_tracker = tracker;
			_titleKey = titleKey;
		}

		public override string DisplayName => _titleKey;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry> {
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
				new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
				new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
			}.AsReadOnly();

		private NotificationGroup FindGroup() {
			var groups = _tracker.Groups;
			for (int i = 0; i < groups.Count; i++) {
				if (groups[i].TitleText == _titleKey)
					return groups[i];
			}
			return null;
		}

		public override int ItemCount {
			get {
				var group = FindGroup();
				return group?.Count ?? 0;
			}
		}

		public override string GetItemLabel(int index) {
			var group = FindGroup();
			if (group == null || index < 0 || index >= group.Count) return null;
			var n = group.Members[index];
			string name = StripBullet(n.NotifierName);
			if (string.IsNullOrEmpty(name))
				return string.Format(
					(string)STRINGS.ONIACCESS.NOTIFICATIONS.NUMBERED_ENTRY,
					_titleKey, index + 1);

			if (n.Notifier != null) {
				string coords = Util.GridCoordinates.Format(
					Grid.PosToCell(n.Notifier.transform.GetPosition()));
				return name + ", " + coords;
			}
			return name;
		}

		private static string StripBullet(string name) {
			if (string.IsNullOrEmpty(name)) return name;
			if (name.StartsWith("\u2022 "))
				return name.Substring(2);
			return name;
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			string label = GetItemLabel(_currentIndex);
			if (label == null) return;
			if (!string.IsNullOrEmpty(parentContext))
				label = parentContext + ", " + label;
			SpeechPipeline.SpeakInterrupt(label);
		}

		public override void OnActivate() {
			base.OnActivate();
			if (ItemCount > 0)
				SpeechPipeline.SpeakQueued(GetItemLabel(0));
		}

		public override bool Tick() {
			if (ItemCount == 0) {
				HandlerStack.Pop();
				return true;
			}
			if (_currentIndex >= ItemCount)
				_currentIndex = ItemCount - 1;
			return base.Tick();
		}

		protected override void ActivateCurrentItem() {
			var group = FindGroup();
			if (group == null || _currentIndex < 0 || _currentIndex >= group.Count) return;
			var n = group.Members[_currentIndex];

			PlayOpenSound();
			// Pop submenu, then pop parent menu
			HandlerStack.Pop();
			HandlerStack.Pop();
			NotificationActivator.Activate(n);
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				HandlerStack.Pop();
				return true;
			}
			return base.HandleKeyDown(e);
		}
	}
}
