using System.Collections.Generic;

using OniAccess.Handlers.Screens.Schedule;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for the ScheduleScreen. Manages two tabs:
	/// Schedules (2D timetable grid with painting and options)
	/// and Duplicants (flat dupe list with schedule reassignment).
	///
	/// Tab cycling via Tab/Shift+Tab. Each tab is a composed object.
	///
	/// Lifecycle: OnShow-patch on ScheduleScreen.OnShow(bool).
	/// </summary>
	public class ScheduleScreenHandler: TabbedScreenHandler {
		private enum TabId { Schedules, Duplicants }

		private readonly SchedulesTab _schedulesTab;
		private readonly DupesTab _dupesTab;

		public ScheduleScreenHandler(KScreen screen) : base(screen) {
			_schedulesTab = new SchedulesTab(this);
			_dupesTab = new DupesTab(this);
			SetTabs(_schedulesTab, _dupesTab);
		}

		public override string DisplayName => STRINGS.ONIACCESS.SCHEDULE.HANDLER_NAME;

		public override bool CapturesAllInput => true;

		private static readonly List<HelpEntry> _schedulesHelpEntries = new List<HelpEntry> {
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Left/Right", STRINGS.ONIACCESS.SCHEDULE.HELP_NAVIGATE_BLOCKS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.SCHEDULE.HELP_JUMP_BLOCK),
			new HelpEntry("1/2/3/4", STRINGS.ONIACCESS.SCHEDULE.HELP_SELECT_BRUSH),
			new HelpEntry("Space", STRINGS.ONIACCESS.SCHEDULE.HELP_PAINT),
			new HelpEntry("Shift+Left/Right", STRINGS.ONIACCESS.SCHEDULE.HELP_PAINT_MOVE),
			new HelpEntry("Shift+Home/End", STRINGS.ONIACCESS.SCHEDULE.HELP_PAINT_RANGE),
			new HelpEntry("Ctrl+Up/Down", STRINGS.ONIACCESS.SCHEDULE.HELP_REORDER_SCHEDULE),
			new HelpEntry("Ctrl+Left/Right", STRINGS.ONIACCESS.SCHEDULE.HELP_ROTATE),
			new HelpEntry("Enter", STRINGS.ONIACCESS.SCHEDULE.HELP_OPTIONS),
			new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL),
		};

		private static readonly List<HelpEntry> _dupesHelpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Left/Right", STRINGS.ONIACCESS.SCHEDULE.HELP_CHANGE_SCHEDULE),
			new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries =>
			ActiveTabIndex == (int)TabId.Schedules ? _schedulesHelpEntries : _dupesHelpEntries;

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			base.OnActivate();
			ActiveTabIndex = (int)TabId.Schedules;
			_schedulesTab.OnTabActivated(announce: false);
		}

		// ========================================
		// TAB MANAGEMENT
		// ========================================

		internal ScheduleScreen ScheduleScreen => _screen as ScheduleScreen;
	}
}
