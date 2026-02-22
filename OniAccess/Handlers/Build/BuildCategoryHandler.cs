using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Modal list of build categories. Opened by Tab from the tile cursor.
	/// Enter on a category opens the building list.
	/// </summary>
	public class BuildCategoryHandler : BaseMenuHandler {
		private List<BuildMenuData.CategoryEntry> _categories;

		public override string DisplayName => (string)STRINGS.ONIACCESS.BUILD_MENU.CATEGORY_LIST;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
			new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE),
		}.AsReadOnly();

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public override int ItemCount => _categories != null ? _categories.Count : 0;

		public override string GetItemLabel(int index) {
			if (_categories == null || index < 0 || index >= _categories.Count) return null;
			return _categories[index].DisplayName;
		}

		public override void SpeakCurrentItem() {
			if (_categories != null && _currentIndex >= 0 && _currentIndex < _categories.Count)
				SpeechPipeline.SpeakInterrupt(_categories[_currentIndex].DisplayName);
		}

		public override void OnActivate() {
			PlayOpenSound();
			_categories = BuildMenuData.GetVisibleCategories();
			_currentIndex = 0;
			_search.Clear();
			if (_categories.Count > 0)
				SpeechPipeline.SpeakInterrupt(_categories[0].DisplayName);
		}

		public override void OnDeactivate() {
			PlayCloseSound();
			base.OnDeactivate();
		}

		protected override void ActivateCurrentItem() {
			if (_categories == null || _currentIndex < 0 || _currentIndex >= _categories.Count)
				return;
			HandlerStack.Replace(new BuildingListHandler(_categories[_currentIndex].Category));
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e))
				return true;
			if (e.TryConsume(Action.Escape)) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLTIP.CLOSED);
				HandlerStack.Pop();
				return true;
			}
			return false;
		}

		private static void PlayOpenSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Open");
		}

		private static void PlayCloseSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Close");
		}
	}
}
