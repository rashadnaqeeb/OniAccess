# AnnouncementFormatter.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner`

Static utility class for building scanner speech strings. Distance is expressed as exact tile offsets from cursor (vertical component first, then horizontal). All strings use `STRINGS.ONIACCESS.SCANNER` LocStrings.

```
static class AnnouncementFormatter (line 6)

  static string FormatEntityInstance(
      string name, int cursorCell, int targetCell, int index, int count) (line 7)
    -- Formats a single-entity instance announcement.
    -- Includes distance string (e.g., "3 up, 2 right") and "X of Y" position.
    -- Uses INSTANCE_WITH_DISTANCE or INSTANCE_NO_DISTANCE depending on whether cursor == target.

  static string FormatClusterInstance(
      int tileCount, string name, int cursorCell, int targetCell, int index, int count) (line 21)
    -- Wraps FormatEntityInstance. If tileCount > 1, prepends a CLUSTER_LABEL ("N name") to the name.

  static string FormatOrderClusterInstance(
      int tileCount, string orderType, string targetName,
      int cursorCell, int targetCell, int index, int count) (line 31)
    -- Formats an order cluster. Uses ORDER_LABEL for single-tile, ORDER_CLUSTER_LABEL for multi-tile,
    -- then passes to FormatEntityInstance.

  static string FormatDistance(int cursorCell, int targetCell) (line 46)
    -- Computes row/column delta, formats using DISTANCE_VERTICAL and DISTANCE_HORIZONTAL strings.
    -- Returns DISTANCE_BOTH when both axes are nonzero, single-axis string otherwise, "" if at cursor.
```
