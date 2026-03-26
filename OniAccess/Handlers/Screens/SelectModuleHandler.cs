using System.Collections.Generic;
using System.Reflection;

using OniAccess.Handlers.Build;
using OniAccess.Speech;

namespace OniAccess.Handlers.Screens {
	public class SelectModuleHandler: NestedMenuHandler {
		private SelectModuleSideScreen ModuleScreen =>
			(SelectModuleSideScreen)_screen;

		private bool _pendingActivation;

		// Reflection fields
		private static readonly FieldInfo _selectedModuleDefField = typeof(SelectModuleSideScreen)
			.GetField("selectedModuleDef", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo _moduleBuildableStateField = typeof(SelectModuleSideScreen)
			.GetField("moduleBuildableState", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo _materialSelectionPanelField = typeof(SelectModuleSideScreen)
			.GetField("materialSelectionPanel", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo _facadeSelectionPanelField = typeof(SelectModuleSideScreen)
			.GetField("facadeSelectionPanel", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo _materialSelectorsField = typeof(MaterialSelectionPanel)
			.GetField("materialSelectors", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo _activeFacadeTogglesField = typeof(FacadeSelectionPanel)
			.GetField("activeFacadeToggles", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo _activeRecipeField = typeof(MaterialSelector)
			.GetField("activeRecipe", BindingFlags.NonPublic | BindingFlags.Instance);

		static SelectModuleHandler() {
			if (_selectedModuleDefField == null) Util.Log.Warn("SelectModuleHandler: selectedModuleDef field not found");
			if (_moduleBuildableStateField == null) Util.Log.Warn("SelectModuleHandler: moduleBuildableState field not found");
			if (_materialSelectionPanelField == null) Util.Log.Warn("SelectModuleHandler: materialSelectionPanel field not found");
			if (_facadeSelectionPanelField == null) Util.Log.Warn("SelectModuleHandler: facadeSelectionPanel field not found");
			if (_materialSelectorsField == null) Util.Log.Warn("SelectModuleHandler: materialSelectors field not found");
			if (_activeFacadeTogglesField == null) Util.Log.Warn("SelectModuleHandler: activeFacadeToggles field not found");
			if (_activeRecipeField == null) Util.Log.Warn("SelectModuleHandler: activeRecipe field not found");
		}

		private enum Section { Modules, Materials, Skin, Build }

		public override string DisplayName => null;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		protected override int MaxLevel => 2;
		protected override int SearchLevel => 1;

		public SelectModuleHandler(SelectModuleSideScreen screen) : base(screen) {
			HelpEntries = new List<HelpEntry>(NestedNavHelpEntries).AsReadOnly();
		}

		// ========================================
		// DATA ACCESS
		// ========================================

		private BuildingDef GetSelectedModuleDef() {
			return _selectedModuleDefField.GetValue(ModuleScreen) as BuildingDef;
		}

		private Dictionary<BuildingDef, bool> GetBuildableState() {
			return _moduleBuildableStateField.GetValue(ModuleScreen)
				as Dictionary<BuildingDef, bool>;
		}

		private MaterialSelectionPanel GetMaterialPanel() {
			return _materialSelectionPanelField.GetValue(ModuleScreen)
				as MaterialSelectionPanel;
		}

		private FacadeSelectionPanel GetFacadePanel() {
			return _facadeSelectionPanelField.GetValue(ModuleScreen)
				as FacadeSelectionPanel;
		}

		private List<MaterialSelector> GetMaterialSelectors() {
			var panel = GetMaterialPanel();
			if (panel == null) return null;
			return _materialSelectorsField.GetValue(panel) as List<MaterialSelector>;
		}

		private Dictionary<string, object> GetActiveFacadeToggles() {
			var panel = GetFacadePanel();
			if (panel == null) return null;
			// activeFacadeToggles is Dictionary<string, FacadeToggle> where FacadeToggle is a private struct.
			// We need to access it via reflection as a non-generic IDictionary.
			var value = _activeFacadeTogglesField.GetValue(panel);
			if (value == null) return null;
			var dict = value as System.Collections.IDictionary;
			if (dict == null) return null;
			var result = new Dictionary<string, object>();
			foreach (System.Collections.DictionaryEntry entry in dict)
				result[(string)entry.Key] = entry.Value;
			return result;
		}

		// ========================================
		// MODULE LIST
		// ========================================

		private List<BuildingDef> GetVisibleModules() {
			var result = new List<BuildingDef>();
			foreach (var id in SelectModuleSideScreen.moduleButtonSortOrder) {
				foreach (var kvp in ModuleScreen.buttons) {
					if (kvp.Key.PrefabID == id && kvp.Value.activeSelf)
						result.Add(kvp.Key);
				}
			}
			return result;
		}

		// ========================================
		// SECTION HELPERS
		// ========================================

		private int SectionCount => (int)Section.Build + 1;

		private bool HasMaterialSection() {
			return GetSelectedModuleDef() != null && GetMaterialPanel() != null;
		}

		private bool HasFacadeSection() {
			var panel = GetFacadePanel();
			if (panel == null || !panel.gameObject.activeSelf) return false;
			var toggles = GetActiveFacadeToggles();
			return toggles != null && toggles.Count > 0;
		}

		private bool IsSectionDrillable(int section) {
			switch ((Section)section) {
				case Section.Modules: return true;
				case Section.Materials: return HasMaterialSection();
				case Section.Skin: return HasFacadeSection();
				case Section.Build: return false;
				default: return false;
			}
		}

		private List<MaterialSelector> GetActiveSelectors() {
			var selectors = GetMaterialSelectors();
			if (selectors == null) return new List<MaterialSelector>();
			var result = new List<MaterialSelector>();
			foreach (var s in selectors) {
				if (s.gameObject.activeSelf)
					result.Add(s);
			}
			return result;
		}

		// ========================================
		// NESTED MENU ABSTRACTS
		// ========================================

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0) return SectionCount;
			if (level == 1) {
				var section = (Section)indices[0];
				switch (section) {
					case Section.Modules: return GetVisibleModules().Count;
					case Section.Materials: return GetActiveSelectors().Count;
					case Section.Skin: return GetFacadeKeys().Count;
					default: return 0;
				}
			}
			if (level == 2) {
				if ((Section)indices[0] == Section.Materials)
					return GetMaterialsForSlot(indices[1]).Count;
				return 0;
			}
			return 0;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (level == 0) return GetSectionLabel(indices[0]);
			if (level == 1) return GetLevel1Label(indices[0], indices[1]);
			if (level == 2 && (Section)indices[0] == Section.Materials)
				return GetMaterialItemLabel(indices[1], indices[2]);
			return null;
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level == 1) return GetSectionLabel(indices[0]);
			if (level == 2) return GetLevel1Label(indices[0], indices[1]);
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			if (Level == 0) {
				if ((Section)indices[0] == Section.Build)
					ClickBuild();
				return;
			}
			if (Level == 2) {
				if ((Section)indices[0] == Section.Materials)
					SelectMaterialAtIndex(indices[1], indices[2]);
				return;
			}
			// Level 1
			var section = (Section)indices[0];
			switch (section) {
				case Section.Modules:
					SelectModuleAtIndex(indices[1]);
					break;
				case Section.Skin:
					SelectFacadeAtIndex(indices[1]);
					break;
			}
		}

		protected override int GetSearchItemCount(int[] indices) {
			// Search across modules only (level 1, section 0)
			return GetVisibleModules().Count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			var modules = GetVisibleModules();
			if (flatIndex < 0 || flatIndex >= modules.Count) return null;
			return modules[flatIndex].Name;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			outIndices[0] = (int)Section.Modules;
			outIndices[1] = flatIndex;
		}

		// ========================================
		// LEFT/RIGHT OVERRIDE
		// ========================================

		protected override void HandleLeftRight(int direction, int stepLevel) {
			if (direction > 0 && Level == 0 && !IsSectionDrillable(GetIndex(0))) {
				return;
			}
			base.HandleLeftRight(direction, stepLevel);
		}

		// ========================================
		// ESCAPE
		// ========================================

		public override bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				CloseScreen();
				return true;
			}
			return base.HandleKeyDown(e);
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			base.OnActivate();
			_pendingActivation = true;
		}

		public override bool Tick() {
			if (_pendingActivation) {
				_pendingActivation = false;
				string title = (string)STRINGS.UI.UISIDESCREENS.SELECTMODULESIDESCREEN.TITLE;
				string firstSection = GetSectionLabel(0);
				SpeechPipeline.SpeakInterrupt(title + ", " + firstSection);
				return false;
			}
			return base.Tick();
		}

		// ========================================
		// LABELS
		// ========================================

		private string GetSectionLabel(int section) {
			switch ((Section)section) {
				case Section.Modules: {
						string label = (string)STRINGS.ONIACCESS.MODULE_SCREEN.MODULES;
						var selected = GetSelectedModuleDef();
						if (selected != null)
							label += ", " + selected.Name;
						return label;
					}
				case Section.Materials:
					return (string)STRINGS.ONIACCESS.MODULE_SCREEN.MATERIALS;
				case Section.Skin:
					return (string)STRINGS.ONIACCESS.MODULE_SCREEN.FACADE;
				case Section.Build: {
						string label = (string)STRINGS.UI.UISIDESCREENS.SELECTMODULESIDESCREEN.BUILDBUTTON;
						if (!ModuleScreen.buildSelectedModuleButton.isInteractable)
							label += ", " + (string)STRINGS.ONIACCESS.STATES.DISABLED;
						return label;
					}
				default:
					return null;
			}
		}

		private string GetLevel1Label(int section, int itemIndex) {
			switch ((Section)section) {
				case Section.Modules: return GetModuleLabel(itemIndex);
				case Section.Materials: return GetMaterialSlotLabel(itemIndex);
				case Section.Skin: return GetFacadeLabel(itemIndex);
				default: return null;
			}
		}

		private string GetModuleLabel(int index) {
			var modules = GetVisibleModules();
			if (index < 0 || index >= modules.Count) return null;
			var def = modules[index];
			string label = def.Name;
			var buildable = GetBuildableState();
			if (buildable != null && buildable.TryGetValue(def, out bool isBuildable) && !isBuildable)
				label += ", " + (string)STRINGS.ONIACCESS.BUILD_MENU.NOT_BUILDABLE;
			if (def == GetSelectedModuleDef())
				label += ", " + (string)STRINGS.ONIACCESS.STATES.SELECTED;
			string desc = def.Desc;
			if (!string.IsNullOrEmpty(desc))
				label += ", " + TextFilter.FilterForSpeech(desc);
			string effect = def.Effect;
			if (!string.IsNullOrEmpty(effect))
				label += ", " + TextFilter.FilterForSpeech(effect);
			return label;
		}

		private string GetMaterialSlotLabel(int slotIndex) {
			var selectors = GetActiveSelectors();
			if (slotIndex < 0 || slotIndex >= selectors.Count) return null;
			var selector = selectors[slotIndex];
			var headerLocText = selector.Headerbar.GetComponentInChildren<LocText>();
			string header = headerLocText != null
				? TextFilter.FilterForSpeech(headerLocText.GetParsedText())
				: (string)STRINGS.ONIACCESS.MODULE_SCREEN.MATERIALS;
			if (selector.CurrentSelectedElement != null)
				header += ", " + selector.CurrentSelectedElement.ProperName();
			return header;
		}

		private List<Tag> GetMaterialsForSlot(int slotIndex) {
			var selectors = GetActiveSelectors();
			if (slotIndex < 0 || slotIndex >= selectors.Count) return new List<Tag>();
			var selector = selectors[slotIndex];
			var tags = new List<Tag>();
			foreach (var kvp in selector.ElementToggles) {
				if (kvp.Value.gameObject.activeSelf)
					tags.Add(kvp.Key);
			}
			return tags;
		}

		private string GetMaterialItemLabel(int slotIndex, int materialIndex) {
			var materials = GetMaterialsForSlot(slotIndex);
			if (materialIndex < 0 || materialIndex >= materials.Count) return null;
			var tag = materials[materialIndex];
			string label = tag.ProperName();
			float available = ClusterManager.Instance.activeWorld.worldInventory
				.GetAmount(tag, includeRelatedWorlds: true);
			label += ", " + GameUtil.GetFormattedByTag(tag, available);
			var selectors = GetActiveSelectors();
			if (slotIndex < selectors.Count && selectors[slotIndex].CurrentSelectedElement == tag)
				label += ", " + (string)STRINGS.ONIACCESS.STATES.SELECTED;
			return label;
		}

		private List<string> GetFacadeKeys() {
			var toggles = GetActiveFacadeToggles();
			if (toggles == null) return new List<string>();
			var keys = new List<string>(toggles.Keys);
			keys.Sort();
			// Default facade first
			if (keys.Remove(BuildMenuData.DefaultFacadeId))
				keys.Insert(0, BuildMenuData.DefaultFacadeId);
			return keys;
		}

		private string GetFacadeLabel(int index) {
			var keys = GetFacadeKeys();
			if (index < 0 || index >= keys.Count) return null;
			string id = keys[index];
			string label;
			if (id == BuildMenuData.DefaultFacadeId) {
				var selectedDef = GetSelectedModuleDef();
				label = selectedDef != null ? selectedDef.Name : id;
			} else {
				var facade = Db.GetBuildingFacades().TryGet(id);
				label = facade != null ? facade.Name : id;
			}
			var facadePanel = GetFacadePanel();
			if (facadePanel != null && facadePanel.SelectedFacade == id)
				label += ", " + (string)STRINGS.ONIACCESS.STATES.SELECTED;
			return label;
		}

		// ========================================
		// ACTIONS
		// ========================================

		private void SelectModuleAtIndex(int index) {
			var modules = GetVisibleModules();
			if (index < 0 || index >= modules.Count) return;
			ModuleScreen.SelectModule(modules[index]);
			Level = 0;
			_search.Clear();
			SpeakCurrentItem();
		}

		private void SelectMaterialAtIndex(int slotIndex, int materialIndex) {
			var materials = GetMaterialsForSlot(slotIndex);
			if (materialIndex < 0 || materialIndex >= materials.Count) return;
			var selectors = GetActiveSelectors();
			if (slotIndex < 0 || slotIndex >= selectors.Count) return;
			var selector = selectors[slotIndex];
			var recipe = _activeRecipeField.GetValue(selector) as Recipe;
			selector.OnSelectMaterial(materials[materialIndex], recipe);
			Level = 0;
			_search.Clear();
			SpeakCurrentItem();
		}

		private void SelectFacadeAtIndex(int index) {
			var keys = GetFacadeKeys();
			if (index < 0 || index >= keys.Count) return;
			var facadePanel = GetFacadePanel();
			if (facadePanel == null) return;
			facadePanel.SelectedFacade = keys[index];
			Level = 0;
			_search.Clear();
			SpeakCurrentItem();
		}

		private void ClickBuild() {
			if (!ModuleScreen.buildSelectedModuleButton.isInteractable) {
				PlaySound("Slider_Boundary_Low");
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.STATES.DISABLED);
				return;
			}
			Widgets.WidgetOps.ClickButton(ModuleScreen.buildSelectedModuleButton);
		}

		private void CloseScreen() {
			DetailsScreenHandler.PreserveNavigationOnReactivate = true;
			HandlerStack.Pop();
			DetailsScreen.Instance?.ClearSecondarySideScreen();
		}
	}
}
