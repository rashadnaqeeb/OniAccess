using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Modal info panel for a building being placed. Shows the building
	/// description, operation requirements, operation effects, room type,
	/// and one material selector item per recipe ingredient.
	/// Enter on a material item opens MaterialPickerHandler.
	/// </summary>
	public class BuildInfoHandler : BaseMenuHandler {
		private readonly BuildingDef _def;
		private List<InfoItem> _items;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
			new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE),
		}.AsReadOnly();

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;
		public override string DisplayName => (string)STRINGS.ONIACCESS.BUILD_MENU.INFO_PANEL;

		public BuildInfoHandler(BuildingDef def) {
			_def = def;
		}

		public override int ItemCount => _items != null ? _items.Count : 0;

		public override string GetItemLabel(int index) {
			if (_items == null || index < 0 || index >= _items.Count) return null;
			return _items[index].Label;
		}

		public override void SpeakCurrentItem() {
			if (_items != null && _currentIndex >= 0 && _currentIndex < _items.Count)
				SpeechPipeline.SpeakInterrupt(_items[_currentIndex].Label);
		}

		public override void OnActivate() {
			PlayOpenSound();
			RebuildItems();
			_currentIndex = 0;
			_search.Clear();

			if (_items.Count > 0)
				SpeechPipeline.SpeakInterrupt(_items[0].Label);
			else
				SpeechPipeline.SpeakInterrupt(DisplayName);
		}

		public override void OnDeactivate() {
			PlayCloseSound();
			base.OnDeactivate();
		}

		protected override void ActivateCurrentItem() {
			if (_items == null || _currentIndex < 0 || _currentIndex >= _items.Count)
				return;

			var item = _items[_currentIndex];
			if (item.SelectorIndex >= 0) {
				HandlerStack.Push(new MaterialPickerHandler(_def, item.SelectorIndex));
			}
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

		private void RebuildItems() {
			_items = new List<InfoItem>();

			// Description
			string desc = _def.Desc;
			if (!string.IsNullOrEmpty(desc))
				_items.Add(new InfoItem(
					(string)STRINGS.ONIACCESS.BUILD_MENU.DESCRIPTION + ": " +
					STRINGS.UI.StripLinkFormatting(desc), -1));

			// Effect text
			string effect = _def.Effect;
			if (!string.IsNullOrEmpty(effect))
				_items.Add(new InfoItem(
					(string)STRINGS.ONIACCESS.BUILD_MENU.EFFECTS + ": " +
					STRINGS.UI.StripLinkFormatting(effect), -1));

			// Operation requirements and effects from descriptors
			AddDescriptorItems();

			// Material selectors
			AddMaterialItems();
		}

		private void AddDescriptorItems() {
			try {
				var allDescriptors = GameUtil.GetAllDescriptors(_def.BuildingComplete);
				var requirements = GameUtil.GetRequirementDescriptors(allDescriptors);
				if (requirements.Count > 0) {
					foreach (var d in requirements) {
						string text = STRINGS.UI.StripLinkFormatting(d.text).Trim();
						if (!string.IsNullOrEmpty(text))
							_items.Add(new InfoItem(
								(string)STRINGS.ONIACCESS.BUILD_MENU.REQUIREMENTS + ": " +
								text, -1));
					}
				}

				var effects = GameUtil.GetEffectDescriptors(allDescriptors);
				if (effects.Count > 0) {
					foreach (var d in effects) {
						string text = STRINGS.UI.StripLinkFormatting(d.text).Trim();
						if (!string.IsNullOrEmpty(text))
							_items.Add(new InfoItem(
								(string)STRINGS.ONIACCESS.BUILD_MENU.EFFECTS + ": " +
								text, -1));
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"BuildInfoHandler.AddDescriptorItems: {ex.Message}");
			}
		}

		private void AddMaterialItems() {
			var recipe = _def.CraftRecipe;
			if (recipe == null || recipe.Ingredients == null) return;

			var panel = PlanScreen.Instance.ProductInfoScreen.materialSelectionPanel;

			for (int i = 0; i < recipe.Ingredients.Count; i++) {
				var ingredient = recipe.Ingredients[i];
				string label = BuildMaterialLabel(ingredient, panel, i);
				_items.Add(new InfoItem(label, i));
			}
		}

		private static string BuildMaterialLabel(
				Recipe.Ingredient ingredient, MaterialSelectionPanel panel, int index) {
			string categoryName = GetIngredientCategoryName(ingredient.tag);
			string selectedName = (string)STRINGS.ONIACCESS.STATES.NONE;
			string quantity = "";
			bool insufficient = false;

			try {
				var selected = panel.GetSelectedElementAsList;
				if (selected != null && index < selected.Count) {
					var tag = selected[index];
					selectedName = tag.ProperName();
					float available = ClusterManager.Instance.activeWorld.worldInventory
						.GetAmount(tag, includeRelatedWorlds: true);
					quantity = GameUtil.GetFormattedMass(available);
					insufficient = available < ingredient.amount
						&& !MaterialSelector.AllowInsufficientMaterialBuild();
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"BuildInfoHandler.BuildMaterialLabel: {ex.Message}");
			}

			if (insufficient)
				return string.Format(
					(string)STRINGS.ONIACCESS.BUILD_MENU.MATERIAL_SLOT_INSUFFICIENT,
					categoryName, selectedName, quantity);
			return string.Format(
				(string)STRINGS.ONIACCESS.BUILD_MENU.MATERIAL_SLOT,
				categoryName, selectedName, quantity);
		}

		private static string GetIngredientCategoryName(Tag tag) {
			string[] parts = tag.ToString().Split('&');
			string name = parts[0].ToTag().ProperName();
			for (int i = 1; i < parts.Length; i++)
				name += " or " + parts[i].ToTag().ProperName();
			return name;
		}

		private static void PlayOpenSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Open");
		}

		private static void PlayCloseSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Close");
		}

		private struct InfoItem {
			public string Label;
			public int SelectorIndex;

			public InfoItem(string label, int selectorIndex) {
				Label = label;
				SelectorIndex = selectorIndex;
			}
		}
	}
}
