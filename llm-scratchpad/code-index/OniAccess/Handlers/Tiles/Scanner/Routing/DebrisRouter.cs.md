# DebrisRouter.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Routing`

Routes pickupable items to `Debris` subcategories by tag priority. Also filters out entities that should not appear in the Debris category at all.

```
static class DebrisRouter (line 6)

  private static Tag[] _itemTags (line 7)
    -- Tags that classify a pickupable as Items subcategory (in priority order):
    -- Seed, Egg, Medicine, MedicalSupplies, Clothes, IndustrialProduct, IndustrialIngredient,
    -- TechComponents, Compostable, ChargedPortableBattery, BionicUpgrade, Dehydrated,
    -- StoryTraitResource, HighEnergyParticle, Artifact.

  private static Tag[] _materialTags (line 17)
    -- Tags that classify a pickupable as Materials subcategory:
    -- Metal, RefinedMetal, Alloy, BuildableRaw, BuildableProcessed, Filter, Liquifiable,
    -- ConsumableOre, Sublimating, Organics, Farmable, Agriculture, Other,
    -- ManufacturedMaterial, CookingIngredient, RareMaterials.

  static bool ShouldExclude(KPrefabID prefabId) (line 29)
    -- Returns true for: BaseMinion, Creature, Robot, GeyserFeature tags,
    -- or if the entity is an Uprootable that is still planted (GetPlanterStorage != null).

  static string GetSubcategory(KPrefabID prefabId) (line 44)
    -- Priority order:
    --   1. Liquid / Breathable / Unbreathable tags → Bottles
    --   2. Edible tag → Food
    --   3. Any _itemTags match → Items
    --   4. Any _materialTags match → Materials
    --   5. Default → Materials
```
