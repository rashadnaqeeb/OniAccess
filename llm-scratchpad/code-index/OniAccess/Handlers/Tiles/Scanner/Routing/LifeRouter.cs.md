# LifeRouter.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Routing`

Entity classification helpers for the `Life` category. Determines whether critters are wild/tame and plants are wild/farmed.

```
static class LifeRouter (line 8)

  static bool IsWild(GameObject go) (line 9)
    -- Returns true if the entity has the GameTags.Creatures.Wild tag.

  static bool IsFarmPlant(Uprootable plant) (line 13)
    -- Returns true if the plant is planted in a planter (GetPlanterStorage != null).
```
