# OrderBackend.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Backends`

Backend for `Zones > Orders`. Handles three kinds of orders:
- **Clustered orders** (box-selection: dig/mop/sweep/disinfect; same-type: build/deconstruct/harvest/uproot) — `BackendData` is `OrderCluster`.
- **Individual orders** (attack, capture, empty pipe) — `BackendData` is `IndividualOrder`.

All data comes from `GridScanner`; no direct game queries in `Scan()`.

```
class OrderBackend : IScannerBackend (line 11)

  private List<OrderCluster> _clusters (line 12)
  private List<IndividualOrder> _individuals (line 13)

  void SetGridData(List<OrderCluster> clusters, List<IndividualOrder> individuals) (line 15)
    -- Called by ScannerNavigator.Refresh() before Scan().

  IEnumerable<ScanEntry> Scan(int worldId) (line 22)
    -- Yields entries for all clusters (using BuildOrderItemName for the item name)
    -- then all individual orders (using ORDER_LABEL format string).

  bool ValidateEntry(ScanEntry entry, int cursorCell) (line 54)
    -- Dispatches to ValidateIndividual or ValidateCluster depending on BackendData type.

  string FormatName(ScanEntry entry) (line 62)
    -- For clusters: formats with order label and target name (with/without count prefix).
    --   Uses ORDER_LABEL (single tile) or ORDER_CLUSTER_LABEL (multi-tile).
    --   Uses ORDER_CLUSTER_COUNT when TargetName is empty.
    -- For individuals: formats with ORDER_LABEL using entity name.

  private static string BuildOrderItemName(OrderCluster cluster) (line 87)
    -- Builds the item name (used at snapshot-build time, not speech time).
    -- Returns just the order label if TargetName is empty; ORDER_LABEL format otherwise.

  private static bool ValidateIndividual(IndividualOrder order) (line 96)
    -- Checks entity is not null/destroyed.
    -- For attack: re-checks FactionAlignment.IsPlayerTargeted().
    -- For capture: re-checks Capturable.IsMarkedForCapture.
    -- For empty pipe: checks IEmptyConduitWorkable component is not destroyed.

  private static bool ValidateCluster(
      OrderCluster cluster, ScanEntry entry, int cursorCell) (line 117)
    -- Iterates cluster.Cells in reverse; removes cells where the order is no longer present.
    -- Updates entry.Cell to closest remaining cell. Returns false if no cells survive.

  private static bool IsOrderStillPresent(int cell, OrderCluster cluster) (line 140)
    -- Dispatches to IsBoxOrderPresent or IsSameTypeOrderPresent based on cluster strategy.

  private static bool IsBoxOrderPresent(int cell, OrderRouter.OrderType orderType) (line 151)
    -- Re-queries OrderRouter.Has*Order methods for the given order type.

  private static bool IsSameTypeOrderPresent(int cell, OrderRouter.OrderType orderType) (line 164)
    -- Re-queries OrderRouter.Get*OrderType methods; returns true if non-null.
```
