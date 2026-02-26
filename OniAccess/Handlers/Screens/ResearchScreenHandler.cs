using System.Collections.Generic;

using OniAccess.Handlers.Screens.Research;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for the ResearchScreen. Manages three tabs:
	/// Browse (categorized tech list), Queue (current research queue),
	/// and Tree (DAG graph navigation).
	///
	/// Tab cycling via Tab/Shift+Tab. Each tab is a composed object
	/// extending the appropriate base class for its navigation pattern.
	///
	/// Lifecycle: Show-patch on ResearchScreen.OnShow(bool).
	/// </summary>
	public class ResearchScreenHandler : BaseScreenHandler {
		private enum TabId { Browse, Queue, Tree }

		private readonly BrowseTab _browseTab;
		private readonly QueueTab _queueTab;
		private readonly TreeTab _treeTab;
		private readonly IResearchTab[] _tabs;

		private TabId _activeTab;

		public ResearchScreenHandler(KScreen screen) : base(screen) {
			_browseTab = new BrowseTab(this);
			_queueTab = new QueueTab(this);
			_treeTab = new TreeTab(this);
			_tabs = new IResearchTab[] { _browseTab, _queueTab, _treeTab };
		}

		public override string DisplayName => STRINGS.ONIACCESS.RESEARCH.HANDLER_NAME;

		public override bool CapturesAllInput => true;

		private static readonly List<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter/Right", STRINGS.ONIACCESS.HELP.OPEN_GROUP),
			new HelpEntry("Left", STRINGS.ONIACCESS.HELP.GO_BACK),
			new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL),
			new HelpEntry("Space", STRINGS.ONIACCESS.RESEARCH.JUMP_TO_TREE_HELP),
			new HelpEntry("Enter", STRINGS.ONIACCESS.RESEARCH.QUEUE_CANCEL_HELP),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			base.OnActivate();
			_activeTab = TabId.Browse;
			_browseTab.OnTabActivated(announce: false);

			string points = ResearchHelper.BuildPointInventoryString();
			if (points != null)
				SpeechPipeline.SpeakQueued(points);
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

			// Intercept Tab for tab cycling before delegating to the active tab.
			// GetKeyDown returns true for one frame only, so the active tab's
			// base.Tick() will see GetKeyDown(Tab) == false.
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

		/// <summary>
		/// Jump to the Tree tab focused on a specific tech.
		/// Called by Browse and Queue tabs when the player presses Space.
		/// </summary>
		internal void JumpToTreeTab(Tech tech) {
			ActiveTab.OnTabDeactivated();
			_activeTab = TabId.Tree;
			_treeTab.OnTabActivatedAt(tech);
		}

		private IResearchTab ActiveTab => _tabs[(int)_activeTab];

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
			catch (System.Exception ex) { Util.Log.Warn($"ResearchScreenHandler: hover sound failed: {ex.Message}"); }
		}

		static void PlayWrapSound() {
			try { KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click")); }
			catch (System.Exception ex) { Util.Log.Warn($"ResearchScreenHandler: wrap sound failed: {ex.Message}"); }
		}
	}
}
