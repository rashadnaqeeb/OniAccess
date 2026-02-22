using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Modal list of buildings in a category. Available buildings are sorted
	/// to the top. Enter on a Complete building activates the build tool.
	/// </summary>
	public class BuildingListHandler : BaseMenuHandler {
		private readonly HashedString _category;
		private readonly int _initialIndex;
		private readonly BuildToolHandler _returnToHandler;
		private List<BuildMenuData.BuildingEntry> _buildings;
		private string _categoryName;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
			new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE),
		}.AsReadOnly();

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public override string DisplayName => _categoryName ?? "";

		/// <summary>
		/// Open from a category selection.
		/// </summary>
		public BuildingListHandler(HashedString category) {
			_category = category;
			_initialIndex = -1;
			_returnToHandler = null;
		}

		/// <summary>
		/// Return from placement (Tab in BuildToolHandler). Cursor starts on
		/// the building at initialIndex. On selection or Escape, control returns
		/// to the existing BuildToolHandler.
		/// </summary>
		public BuildingListHandler(HashedString category, int initialIndex, BuildToolHandler returnTo) {
			_category = category;
			_initialIndex = initialIndex;
			_returnToHandler = returnTo;
		}

		public override int ItemCount => _buildings != null ? _buildings.Count : 0;

		public override string GetItemLabel(int index) {
			if (_buildings == null || index < 0 || index >= _buildings.Count) return null;
			return _buildings[index].Label;
		}

		public override void SpeakCurrentItem() {
			if (_buildings != null && _currentIndex >= 0 && _currentIndex < _buildings.Count)
				SpeechPipeline.SpeakInterrupt(_buildings[_currentIndex].Label);
		}

		public override void OnActivate() {
			PlayOpenSound();
			_buildings = BuildMenuData.GetVisibleBuildings(_category);
			_categoryName = GetCategoryName();
			_currentIndex = 0;
			_search.Clear();

			if (_initialIndex >= 0 && _initialIndex < _buildings.Count)
				_currentIndex = _initialIndex;

			if (_buildings.Count > 0)
				SpeechPipeline.SpeakInterrupt(_buildings[_currentIndex].Label);
			else
				SpeechPipeline.SpeakInterrupt(_categoryName);
		}

		public override void OnDeactivate() {
			PlayCloseSound();
			base.OnDeactivate();
		}

		protected override void ActivateCurrentItem() {
			if (_buildings == null || _currentIndex < 0 || _currentIndex >= _buildings.Count)
				return;

			var entry = _buildings[_currentIndex];
			if (entry.State != PlanScreen.RequirementsState.Complete) {
				PlayNegativeSound();
				string reason = PlanScreen.GetTooltipForRequirementsState(entry.Def, entry.State);
				if (!string.IsNullOrEmpty(reason))
					SpeechPipeline.SpeakInterrupt(reason);
				else
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.NOT_BUILDABLE);
				return;
			}

			if (_returnToHandler != null) {
				if (!BuildMenuData.SelectBuilding(entry.Def, _category)) {
					PlayNegativeSound();
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.NOT_BUILDABLE);
					return;
				}
				_returnToHandler.SwitchBuilding(entry.Def, _currentIndex);
				HandlerStack.Pop();
			} else {
				var handler = new BuildToolHandler(_category, _currentIndex, entry.Def);
				HandlerStack.Replace(handler);
				if (!BuildMenuData.SelectBuilding(entry.Def, _category)) {
					HandlerStack.Pop();
					PlayNegativeSound();
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.NOT_BUILDABLE);
					return;
				}
				SpeechPipeline.SpeakQueued(BuildMenuData.GetMaterialSummary(entry.Def));
			}
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e))
				return true;
			if (e.TryConsume(Action.Escape)) {
				if (_returnToHandler != null) {
					HandlerStack.Pop();
				} else {
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLTIP.CLOSED);
					HandlerStack.Pop();
				}
				return true;
			}
			return false;
		}

		private string GetCategoryName() {
			string text = HashCache.Get().Get(_category).ToUpper();
			string name = Strings.Get("STRINGS.UI.BUILDCATEGORIES." + text + ".NAME");
			return STRINGS.UI.StripLinkFormatting(name);
		}

		private static void PlayOpenSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Open");
		}

		private static void PlayCloseSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Close");
		}

		private static void PlayNegativeSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Negative"));
			} catch (System.Exception ex) {
				Util.Log.Error($"BuildingListHandler.PlayNegativeSound: {ex.Message}");
			}
		}
	}
}
