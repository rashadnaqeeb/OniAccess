using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Two-level navigation: subcategory headers (level 0) and buildings
	/// within each subcategory (level 1). Enter/Right drills into a
	/// subcategory; Left returns. Up/Down at level 1 crosses subcategory
	/// boundaries. Type-ahead always searches buildings (level 1).
	/// </summary>
	public class BuildingListHandler: NestedMenuHandler {
		private readonly HashedString _category;
		private readonly BuildingDef _initialDef;
		private List<BuildMenuData.SubcategoryGroup> _groups;
		private string _categoryName;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries;

		static BuildingListHandler() {
			var list = new List<HelpEntry>();
			list.AddRange(NestedNavHelpEntries);
			list.Add(new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE));
			_helpEntries = list.AsReadOnly();
		}

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public override string DisplayName => _categoryName ?? "";

		/// <summary>
		/// Open from a category selection.
		/// </summary>
		public BuildingListHandler(HashedString category) {
			_category = category;
			_initialDef = null;
		}

		/// <summary>
		/// Return from placement (Tab in BuildToolHandler). Cursor starts on
		/// the building matching initialDef.
		/// </summary>
		public BuildingListHandler(HashedString category, BuildingDef initialDef) {
			_category = category;
			_initialDef = initialDef;
		}

		// ========================================
		// NESTED MENU ABSTRACTS
		// ========================================

		protected override int MaxLevel => 1;
		protected override int SearchLevel => 1;

		protected override int GetItemCount(int level, int[] indices) {
			if (_groups == null) return 0;
			if (level == 0) return _groups.Count;
			if (indices[0] < 0 || indices[0] >= _groups.Count) return 0;
			return _groups[indices[0]].Buildings.Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (_groups == null) return null;
			if (level == 0) {
				if (indices[0] < 0 || indices[0] >= _groups.Count) return null;
				return _groups[indices[0]].Name;
			}
			if (indices[0] < 0 || indices[0] >= _groups.Count) return null;
			var buildings = _groups[indices[0]].Buildings;
			if (indices[1] < 0 || indices[1] >= buildings.Count) return null;
			return buildings[indices[1]].Label;
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level < 1 || _groups == null) return null;
			if (indices[0] < 0 || indices[0] >= _groups.Count) return null;
			return _groups[indices[0]].Name;
		}

		protected override void ActivateLeafItem(int[] indices) {
			if (_groups == null) return;
			if (indices[0] < 0 || indices[0] >= _groups.Count) return;
			var buildings = _groups[indices[0]].Buildings;
			if (indices[1] < 0 || indices[1] >= buildings.Count) return;

			var entry = buildings[indices[1]];

			// Replace before SelectBuilding: SelectBuilding triggers game
			// events that push handlers onto the stack. If BuildingListHandler
			// is still on top, those pushes land above it and Replace hits
			// the wrong handler.
			var handler = new BuildToolHandler(_category, entry.Def);
			HandlerStack.Replace(handler);
			handler.SuppressToolEvents = true;
			if (!BuildMenuData.SelectBuilding(entry.Def, _category)) {
				handler.SuppressToolEvents = false;
				HandlerStack.Pop();
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.NOT_BUILDABLE);
				return;
			}
			handler.SuppressToolEvents = false;
			handler.AnnounceInitialState();
		}

		// ========================================
		// SEARCH
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			if (_groups == null) return 0;
			int total = 0;
			for (int g = 0; g < _groups.Count; g++)
				total += _groups[g].Buildings.Count;
			return total;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			if (_groups == null) return null;
			int remaining = flatIndex;
			for (int g = 0; g < _groups.Count; g++) {
				int count = _groups[g].Buildings.Count;
				if (remaining < count)
					return _groups[g].Buildings[remaining].Label;
				remaining -= count;
			}
			return null;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			if (_groups == null) return;
			int remaining = flatIndex;
			for (int g = 0; g < _groups.Count; g++) {
				int count = _groups[g].Buildings.Count;
				if (remaining < count) {
					outIndices[0] = g;
					outIndices[1] = remaining;
					return;
				}
				remaining -= count;
			}
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			PlayOpenSound();
			_groups = BuildMenuData.GetGroupedBuildings(_category);
			_categoryName = GetCategoryName();

			if (_initialDef != null && _restoreFlatIndex < 0)
				FindDefFlatIndex(_initialDef);

			// base.OnActivate resets indices to 0 and speaks DisplayName.
			base.OnActivate();

			if (_restoreFlatIndex >= 0) {
				NestedSearchMoveTo(_restoreFlatIndex);
				_restoreFlatIndex = -1;
			} else if (_groups != null && _groups.Count > 0) {
				SpeechPipeline.SpeakQueued(_groups[0].Name);
			}
		}

		public override void OnDeactivate() {
			PlayCloseSound();
			base.OnDeactivate();
		}

		// ========================================
		// ESCAPE
		// ========================================

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

		// ========================================
		// PRIVATE HELPERS
		// ========================================

		private int _restoreFlatIndex = -1;

		private void FindDefFlatIndex(BuildingDef def) {
			if (_groups == null) return;
			int flat = 0;
			for (int g = 0; g < _groups.Count; g++) {
				var buildings = _groups[g].Buildings;
				for (int b = 0; b < buildings.Count; b++) {
					if (buildings[b].Def == def) {
						_restoreFlatIndex = flat;
						return;
					}
					flat++;
				}
			}
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
