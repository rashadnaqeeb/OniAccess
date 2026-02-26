using System.Collections.Generic;

using Database;

using OniAccess.Handlers.Screens.Skills;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for the SkillsScreen. Manages three tabs:
	/// Duplicants (flat dupe list), Skills (categorized skill browser),
	/// and Tree (DAG graph navigation).
	///
	/// Tab cycling via Tab/Shift+Tab. Each tab is a composed object.
	/// Tracks the currently selected duplicant as shared state.
	///
	/// Lifecycle: Show-patch on SkillsScreen.Show(bool).
	/// </summary>
	public class SkillsScreenHandler: BaseScreenHandler {
		private enum TabId { Duplicants, Skills, Tree }

		private readonly DupeTab _dupeTab;
		private readonly SkillsTab _skillsTab;
		private readonly TreeTab _treeTab;
		private readonly ISkillsTab[] _tabs;

		private TabId _activeTab;
		private IAssignableIdentity _selectedDupe;

		public SkillsScreenHandler(KScreen screen) : base(screen) {
			_dupeTab = new DupeTab(this);
			_skillsTab = new SkillsTab(this);
			_treeTab = new TreeTab(this);
			_tabs = new ISkillsTab[] { _dupeTab, _skillsTab, _treeTab };
		}

		public override string DisplayName => STRINGS.ONIACCESS.SKILLS.HANDLER_NAME;

		public override bool CapturesAllInput => true;

		internal IAssignableIdentity SelectedDupe => _selectedDupe;

		private static readonly List<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter/Right", STRINGS.ONIACCESS.HELP.OPEN_GROUP),
			new HelpEntry("Left", STRINGS.ONIACCESS.HELP.GO_BACK),
			new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL),
			new HelpEntry("Space", STRINGS.ONIACCESS.SKILLS.JUMP_TO_TREE_HELP),
			new HelpEntry("Enter", STRINGS.ONIACCESS.SKILLS.LEARN_HELP),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			base.OnActivate();

			// Pick initial dupe from the screen's selection, or first live minion
			var screen = _screen as SkillsScreen;
			if (screen != null && screen.CurrentlySelectedMinion != null)
				_selectedDupe = screen.CurrentlySelectedMinion;
			else if (Components.LiveMinionIdentities.Count > 0)
				_selectedDupe = Components.LiveMinionIdentities.Items[0];

			_activeTab = TabId.Duplicants;
			_dupeTab.OnTabActivated(announce: false);
		}

		public override void OnDeactivate() {
			ActiveTab.OnTabDeactivated();
			base.OnDeactivate();
		}

		// ========================================
		// INPUT
		// ========================================

		public override bool Tick() {
			if (base.Tick()) return true;

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Tab)) {
				int dir = InputUtil.ShiftHeld() ? -1 : 1;
				CycleTab(dir);
				return true;
			}

			return ActiveTab.HandleInput();
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			return ActiveTab.HandleKeyDown(e);
		}

		// ========================================
		// TAB MANAGEMENT
		// ========================================

		internal void SelectDupeAndJumpToSkills(IAssignableIdentity dupe) {
			SetSelectedDupe(dupe);
			ActiveTab.OnTabDeactivated();
			_activeTab = TabId.Skills;
			PlayHoverSound();
			ActiveTab.OnTabActivated(announce: true);
		}

		internal void JumpToTreeTab(Skill skill) {
			ActiveTab.OnTabDeactivated();
			_activeTab = TabId.Tree;
			_treeTab.OnTabActivatedAt(skill);
		}

		private void SetSelectedDupe(IAssignableIdentity dupe) {
			_selectedDupe = dupe;
			// Sync with the game screen
			var screen = _screen as SkillsScreen;
			if (screen != null)
				screen.CurrentlySelectedMinion = dupe;
		}

		private ISkillsTab ActiveTab => _tabs[(int)_activeTab];

		private void CycleTab(int direction) {
			ActiveTab.OnTabDeactivated();
			int next = ((int)_activeTab + direction + _tabs.Length) % _tabs.Length;
			bool wrapped = direction > 0 ? next <= (int)_activeTab : next >= (int)_activeTab;
			_activeTab = (TabId)next;
			if (wrapped) PlayWrapSound();
			else PlayHoverSound();
			ActiveTab.OnTabActivated(announce: true);
		}

		// ========================================
		// SOUNDS
		// ========================================

		static void PlayHoverSound() {
			try { KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Mouseover")); }
			catch (System.Exception ex) { Util.Log.Warn($"SkillsScreenHandler: hover sound failed: {ex.Message}"); }
		}

		static void PlayWrapSound() {
			try { KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click")); }
			catch (System.Exception ex) { Util.Log.Warn($"SkillsScreenHandler: wrap sound failed: {ex.Message}"); }
		}
	}
}
