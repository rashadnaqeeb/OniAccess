using System.Collections.Generic;

using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Codex {
	/// <summary>
	/// Handler for the CodexScreen (in-game Database/Incyclopedia).
	/// Two tabs: Categories (NestedMenuHandler) and Content (flat reader).
	/// Tab cycling via Tab/Shift+Tab.
	///
	/// Lifecycle: Show-patch on CodexScreen.OnShow(bool).
	/// ChangeArticle postfix resets the content tab.
	/// </summary>
	public class CodexScreenHandler: BaseScreenHandler {
		private enum TabId { Categories, Content }

		private readonly CategoriesTab _categoriesTab;
		private readonly ContentTab _contentTab;
		private readonly ICodexTab[] _tabs;

		private TabId _activeTab;

		public CodexScreenHandler(KScreen screen) : base(screen) {
			_categoriesTab = new CategoriesTab(this);
			_contentTab = new ContentTab(this);
			_tabs = new ICodexTab[] { _categoriesTab, _contentTab };
		}

		public override string DisplayName => STRINGS.UI.CODEX.TITLE;

		public override bool CapturesAllInput => true;

		internal CodexScreen CodexScreen => _screen as CodexScreen;

		internal ContentTab ContentTabRef => _contentTab;

		private static readonly List<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Ctrl+Up/Down", STRINGS.ONIACCESS.HELP.JUMP_GROUP),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter/Right", STRINGS.ONIACCESS.HELP.OPEN_GROUP),
			new HelpEntry("Left", STRINGS.ONIACCESS.HELP.GO_BACK),
			new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL),
			new HelpEntry("Enter", STRINGS.ONIACCESS.CODEX.FOLLOW_LINK_HELP),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			base.OnActivate();
			_activeTab = TabId.Categories;
			_categoriesTab.OnTabActivated(announce: false);
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
				if (_activeTab == TabId.Content) {
					JumpToCategoriesOnArticle();
				} else {
					int dir = InputUtil.ShiftHeld() ? -1 : 1;
					CycleTab(dir);
				}
				return true;
			}

			return ActiveTab.HandleInput();
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			// Escape from content tab returns to categories instead of closing
			if (_activeTab == TabId.Content && e.TryConsume(Action.Escape)) {
				JumpToCategoriesOnArticle();
				return true;
			}
			return ActiveTab.HandleKeyDown(e);
		}

		// ========================================
		// TAB MANAGEMENT
		// ========================================

		/// <summary>
		/// Switch to content tab. Called by CategoriesTab when a leaf entry is activated,
		/// and by OnArticleChanged for external navigations.
		/// </summary>
		internal void JumpToContentTab() {
			if (_activeTab == TabId.Content) return;
			ActiveTab.OnTabDeactivated();
			_activeTab = TabId.Content;
			PlayHoverSound();
			ActiveTab.OnTabActivated(announce: true);
		}

		/// <summary>
		/// Switch from content tab to categories, landing on the current article.
		/// </summary>
		private void JumpToCategoriesOnArticle() {
			ActiveTab.OnTabDeactivated();
			_activeTab = TabId.Categories;
			PlayHoverSound();
			string entryId = CodexScreen?.activeEntryID;
			_categoriesTab.OnTabActivatedOnEntry(announce: true, entryId: entryId);
		}

		/// <summary>
		/// Called from the ChangeArticle postfix patch.
		/// When the content tab is active, rebuilds and speaks the new article.
		/// When on the categories tab (external navigation via OpenCodexToEntry),
		/// switches to content tab automatically.
		/// When called from CategoriesTab.ActivateLeafItem, the categories tab is
		/// still active, so this switches to content. The subsequent JumpToContentTab
		/// call is then a no-op since we're already on content.
		/// </summary>
		internal void OnArticleChanged() {
			if (_activeTab == TabId.Content)
				_contentTab.OnArticleChanged();
			else
				JumpToContentTab();
		}

		private ICodexTab ActiveTab => _tabs[(int)_activeTab];

		private void CycleTab(int direction) {
			ActiveTab.OnTabDeactivated();
			int next = ((int)_activeTab + direction + _tabs.Length) % _tabs.Length;
			bool wrapped = direction > 0 ? next <= (int)_activeTab : next >= (int)_activeTab;
			_activeTab = (TabId)next;
			if (wrapped) PlayWrapSound();
			else PlayHoverSound();
			ActiveTab.OnTabActivated(announce: true);
		}
	}
}
