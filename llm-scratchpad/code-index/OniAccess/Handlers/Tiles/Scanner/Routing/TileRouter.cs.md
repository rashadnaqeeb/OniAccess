# TileRouter.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Routing`

Routes KAnimTile buildings (plain construction tiles detected on `FoundationTile`/`LadderTile` layers) to their scanner category. All KAnimTiles currently go to `Solids > Tiles`. Functional tile buildings (doors, farm tiles, ladders, etc.) are skipped here and handled by `EntityBackend` via `BuildingRouter` instead.

```
static class TileRouter (line 8)

  static (string category, string subcategory) Route(string prefabId) (line 9)
    -- Always returns (Solids, Tiles). The prefabId parameter is accepted for interface uniformity
    -- but is not currently used (all tiles map to the same destination).
```
