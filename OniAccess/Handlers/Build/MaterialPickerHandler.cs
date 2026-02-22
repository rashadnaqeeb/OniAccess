using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Modal material picker for a single recipe ingredient slot.
	/// Lists discovered materials with available quantities.
	/// Enter selects the material and pops back to BuildInfoHandler.
	/// </summary>
	public class MaterialPickerHandler : BaseMenuHandler {
		private readonly BuildingDef _def;
		private readonly int _selectorIndex;
		private List<MaterialEntry> _materials;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
			new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE),
		}.AsReadOnly();

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;
		public override string DisplayName => "";

		public MaterialPickerHandler(BuildingDef def, int selectorIndex) {
			_def = def;
			_selectorIndex = selectorIndex;
		}

		public override int ItemCount => _materials != null ? _materials.Count : 0;

		public override string GetItemLabel(int index) {
			if (_materials == null || index < 0 || index >= _materials.Count) return null;
			return _materials[index].Label;
		}

		public override void SpeakCurrentItem() {
			if (_materials != null && _currentIndex >= 0 && _currentIndex < _materials.Count)
				SpeechPipeline.SpeakInterrupt(_materials[_currentIndex].Label);
		}

		public override void OnActivate() {
			PlayOpenSound();
			RebuildList();
			_currentIndex = 0;
			_search.Clear();

			// Position cursor on the currently selected material
			PositionOnSelected();

			if (_materials.Count > 0)
				SpeechPipeline.SpeakInterrupt(_materials[_currentIndex].Label);
		}

		public override void OnDeactivate() {
			PlayCloseSound();
			base.OnDeactivate();
		}

		protected override void ActivateCurrentItem() {
			if (_materials == null || _currentIndex < 0 || _currentIndex >= _materials.Count)
				return;

			var entry = _materials[_currentIndex];
			SelectMaterial(entry.Tag);
			HandlerStack.Pop();
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e))
				return true;
			if (e.TryConsume(Action.Escape)) {
				HandlerStack.Pop();
				return true;
			}
			return false;
		}

		private void RebuildList() {
			_materials = new List<MaterialEntry>();

			var recipe = _def.CraftRecipe;
			if (recipe == null || _selectorIndex >= recipe.Ingredients.Count)
				return;

			var ingredient = recipe.Ingredients[_selectorIndex];
			var validTags = MaterialSelector.GetValidMaterials(ingredient.tag);

			var sufficient = new List<MaterialEntry>();
			var insufficient = new List<MaterialEntry>();

			foreach (var tag in validTags) {
				if (!DiscoveredResources.Instance.IsDiscovered(tag))
					continue;

				float available = ClusterManager.Instance.activeWorld.worldInventory
					.GetAmount(tag, includeRelatedWorlds: true);
				string name = tag.ProperName();
				string quantity = GameUtil.GetFormattedMass(available);
				bool hasSufficient = available >= ingredient.amount
					|| MaterialSelector.AllowInsufficientMaterialBuild();

				string label;
				if (hasSufficient)
					label = string.Format(
						(string)STRINGS.ONIACCESS.BUILD_MENU.MATERIAL_ENTRY,
						name, quantity);
				else
					label = string.Format(
						(string)STRINGS.ONIACCESS.BUILD_MENU.MATERIAL_INSUFFICIENT,
						name, quantity);

				var entry = new MaterialEntry { Tag = tag, Label = label };
				if (hasSufficient)
					sufficient.Add(entry);
				else
					insufficient.Add(entry);
			}

			_materials.AddRange(sufficient);
			_materials.AddRange(insufficient);
		}

		private void PositionOnSelected() {
			try {
				var selected = PlanScreen.Instance.ProductInfoScreen.materialSelectionPanel
					.GetSelectedElementAsList;
				if (selected != null && _selectorIndex < selected.Count) {
					var currentTag = selected[_selectorIndex];
					for (int i = 0; i < _materials.Count; i++) {
						if (_materials[i].Tag == currentTag) {
							_currentIndex = i;
							break;
						}
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"MaterialPickerHandler.PositionOnSelected: {ex.Message}");
			}
		}

		private void SelectMaterial(Tag tag) {
			try {
				var panel = PlanScreen.Instance.ProductInfoScreen.materialSelectionPanel;
				if (_selectorIndex == 0)
					panel.ForceSelectPrimaryTag(tag);
				else {
					// Access the materialSelectors list via reflection
					var field = HarmonyLib.AccessTools.Field(
						typeof(MaterialSelectionPanel), "materialSelectors");
					var selectors = (List<MaterialSelector>)field.GetValue(panel);
					if (_selectorIndex < selectors.Count)
						selectors[_selectorIndex].OnSelectMaterial(
							tag, _def.CraftRecipe, false);
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"MaterialPickerHandler.SelectMaterial: {ex}");
			}
		}

		private static void PlayOpenSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Open");
		}

		private static void PlayCloseSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Close");
		}

		private struct MaterialEntry {
			public Tag Tag;
			public string Label;
		}
	}
}
