namespace OniAccess.Handlers.Tiles.Scanner.Routing {
	/// <summary>
	/// Routes pickupable items to Debris subcategories by tag priority.
	/// Also handles exclusions (geysers, still-planted uprootables).
	/// </summary>
	public static class DebrisRouter {
		private static readonly Tag[] _itemTags = {
			GameTags.Seed, GameTags.Egg, GameTags.Medicine,
			GameTags.MedicalSupplies, GameTags.Clothes,
			GameTags.IndustrialProduct, GameTags.IndustrialIngredient,
			GameTags.TechComponents, GameTags.Compostable,
			GameTags.ChargedPortableBattery, GameTags.BionicUpgrade,
			GameTags.Dehydrated, GameTags.StoryTraitResource,
			GameTags.HighEnergyParticle, GameTags.Artifact,
		};

		private static readonly Tag[] _materialTags = {
			GameTags.Metal, GameTags.RefinedMetal, GameTags.Alloy,
			GameTags.BuildableRaw, GameTags.BuildableProcessed,
			GameTags.Filter, GameTags.Liquifiable, GameTags.ConsumableOre,
			GameTags.Sublimating, GameTags.Organics, GameTags.Farmable,
			GameTags.Agriculture, GameTags.Other, GameTags.ManufacturedMaterial,
			GameTags.CookingIngredient, GameTags.RareMaterials,
		};

		/// <summary>
		/// Returns true if this pickupable should be excluded from debris.
		/// </summary>
		public static bool ShouldExclude(KPrefabID prefabId) {
			if (prefabId.HasTag(GameTags.BaseMinion))
				return true;
			if (prefabId.HasTag(GameTags.Creature))
				return true;
			if (prefabId.HasTag(GameTags.Robot))
				return true;
			if (prefabId.HasTag(GameTags.GeyserFeature))
				return true;
			var uprootable = prefabId.GetComponent<Uprootable>();
			if (uprootable != null && uprootable.GetPlanterStorage != null)
				return true;
			return false;
		}

		public static string GetSubcategory(KPrefabID prefabId) {
			if (prefabId.HasTag(GameTags.Liquid)
				|| prefabId.HasTag(GameTags.Breathable)
				|| prefabId.HasTag(GameTags.Unbreathable))
				return ScannerTaxonomy.Subcategories.Bottles;

			if (prefabId.HasTag(GameTags.Edible))
				return ScannerTaxonomy.Subcategories.Food;

			for (int i = 0; i < _itemTags.Length; i++) {
				if (prefabId.HasTag(_itemTags[i]))
					return ScannerTaxonomy.Subcategories.Items;
			}

			for (int i = 0; i < _materialTags.Length; i++) {
				if (prefabId.HasTag(_materialTags[i]))
					return ScannerTaxonomy.Subcategories.Materials;
			}

			return ScannerTaxonomy.Subcategories.Materials;
		}
	}
}
