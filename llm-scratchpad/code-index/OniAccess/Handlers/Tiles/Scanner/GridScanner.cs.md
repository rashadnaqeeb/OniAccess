# GridScanner.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner`

Contains intermediate cluster descriptor data classes (output of the scanning phase, input to backends) and the main `GridScanner` class. All cluster types are declared at the top of this file; GridScanner follows.

---

## Intermediate cluster data classes (lines 14-71)

These are plain data holders. Backends receive these and convert them to `ScanEntry` objects.

```
class ElementCluster (line 14)
  SimHashes ElementId (line 15)
  string Category (line 16)
  string Subcategory (line 17)
  string ElementName (line 18)
  List<int> Cells (line 19)

class TileCluster (line 22)
  string PrefabId (line 23)
  string Category (line 24)
  string Subcategory (line 25)
  List<int> Cells (line 26)

class NetworkSegmentCluster (line 29)
  string PrefabId (line 30)
  string ScannerCategory (line 31)
  string ScannerSubcategory (line 32)
  List<int> Cells (line 33)

class BridgeInstance (line 36)
  int Cell (line 37)
  GameObject Go (line 38)
  string ScannerCategory (line 39)
  string ScannerSubcategory (line 40)

class OrderCluster (line 43)
  OrderRouter.OrderType OrderType (line 44)
  string TargetName (line 45)  -- null for sweep/disinfect (no meaningful per-cell target)
  bool IsMixed (line 46)       -- true when a box-order cluster contains multiple target types
  List<int> Cells (line 47)

class IndividualOrder (line 50)
  OrderRouter.OrderType OrderType (line 51)
  int Cell (line 52)
  GameObject Entity (line 53)
  string EntityName (line 54)

class BiomeCluster (line 57)
  SubWorld.ZoneType ZoneType (line 58)
  string DisplayName (line 59)
  List<int> Cells (line 60)

class GridScanResult (line 63)
  List<ElementCluster> Elements (line 64)
  List<TileCluster> Tiles (line 65)
  List<NetworkSegmentCluster> NetworkSegments (line 66)
  List<BridgeInstance> Bridges (line 67)
  List<OrderCluster> OrderClusters (line 68)
  List<IndividualOrder> IndividualOrders (line 69)
  List<BiomeCluster> Biomes (line 70)
```

---

## class GridScanner (line 77)

Single-pass row-major grid scanner with union-find clustering. One `GridScanner` instance is created by `ScannerNavigator` and reused across refreshes (arrays are reset, not reallocated, when size matches).

### Private fields

```
  BiomeNameResolver _biomeNameResolver (line 78)
  UnionFind _ufElements (line 81)
  UnionFind _ufTiles (line 82)
  UnionFind[] _ufNetworks (line 83)      -- one per NetworkLayerConfig.Types entry
  UnionFind[] _ufBoxOrders (line 84)     -- one per box-order type (4 total)
  UnionFind _ufSameTypeOrders (line 85)
  UnionFind _ufBiomes (line 86)
  int[] _elementKey (line 90)            -- SimHashes cast to int; 0 = absent
  int[] _tileKey (line 91)               -- prefab ID hash; 0 = absent
  int[][] _networkKey (line 92)          -- [networkTypeIndex][cell]; 0 = absent
  int[][] _boxOrderKey (line 93)         -- [boxOrderIndex][cell]; 1 = present, 0 = absent
  string[] _sameTypeKey (line 94)        -- "Build:PrefabId" etc; null = absent
  int[] _biomeKey (line 95)              -- ZoneType cast to int; -1 = unset
  Dictionary<int, ElementCluster> _elementClusters (line 98)
  Dictionary<int, TileCluster> _tileClusters (line 100)
  Dictionary<int, NetworkSegmentCluster>[] _networkClusters (line 102)
  Dictionary<int, OrderCluster>[] _boxOrderClusters (line 103)
  Dictionary<int, OrderCluster> _sameTypeOrderClusters (line 104)
  Dictionary<int, BiomeCluster> _biomeClusters (line 106)
  List<BridgeInstance> _bridges (line 110)
  HashSet<int> _seenBridges (line 111)   -- dedup by GameObject instance ID
  List<IndividualOrder> _individualOrders (line 112)
  Dictionary<int, HashSet<int>> _sameTypeSeenIds (line 117)  -- dedup multi-cell buildings in same-type orders
  static BoxOrderDef[] BoxOrderDefs (line 121)
  static int[] ConduitLayers (line 128)
```

### Private struct BoxOrderDef (line 134)

```
  struct BoxOrderDef (line 134)
    Func<int, bool> Detect (line 135)
    OrderRouter.OrderType Type (line 136)
    int Index (line 137)
    BoxOrderDef(Func<int,bool> detect, OrderRouter.OrderType type, int index) (line 139)
```

### Constructor

```
  GridScanner(BiomeNameResolver biomeNameResolver) (line 147)
```

### Public methods

```
  GridScanResult Scan(int worldId) (line 169)
```
Entry point. Runs AllocateOrReset, ClearAccumulators, ForwardPass, ExtractClusters, ResolveBoxOrderTargets, BuildResult in sequence.

### Private — allocation / reset

```
  void AllocateOrReset(int cellCount) (line 186)
  static UnionFind ResetUF(UnionFind uf, int size) (line 210)
  static int[] ResetIntArray(int[] arr, int size, int sentinel) (line 216)
  void ClearAccumulators() (line 226)
```

### Private — forward pass (per-cell processing)

```
  void ForwardPass(WorldContainer world) (line 245)   -- row-major iteration; catches exceptions per-cell
  void ProcessCell(int cell) (line 267)               -- dispatches to all Process* methods
  void ProcessElement(int cell) (line 282)            -- natural elements (skips foundation tiles, vacuum, unbreakable)
  void ProcessTiles(int cell) (line 295)              -- KAnimTile constructed tiles on FoundationTile/LadderTile layers
  void ProcessNetwork(int cell, int typeIndex) (line 313)  -- utility segment buildings
  void ProcessBridges(int cell) (line 328)            -- utility bridge buildings (non-clustered, deduped by instance ID)
  void ProcessBoxOrders(int cell) (line 353)          -- dig/mop/sweep/disinfect box orders
  void ProcessSameTypeOrders(int cell) (line 363)     -- build/deconstruct/harvest/uproot per prefab type
  void TrySameTypeOrder(int cell, Func<int,string> detectFn, string orderLabel) (line 370)
  void ProcessPickupableOrders(int cell) (line 384)   -- attack/capture orders on pickupables linked list
  void ProcessEmptyPipeOrders(int cell) (line 421)    -- empty-pipe orders on conduit layers
  void ProcessBiome(int cell) (line 436)
```

### Private — union helpers

```
  static void UnionWithNeighbors(UnionFind uf, int[] keys, int cell) (line 447)
    -- Unions left and below neighbors when key is nonzero and equal. Used for int-keyed domains.
  static void UnionWithNeighborsSigned(UnionFind uf, int[] keys, int cell) (line 460)
    -- Same but treats -1 as absent (biome domain).
  void UnionSameTypeNeighbors(int cell) (line 474)
    -- String equality on _sameTypeKey.
```

### Private — cluster extraction (second pass)

```
  void ExtractClusters(int cellCount) (line 492)
  void ExtractElement(int cell) (line 511)
  void ExtractTile(int cell) (line 537)
  void ExtractNetwork(int cell, int typeIndex) (line 554)
  void ExtractBoxOrder(int cell, int orderIndex) (line 571)
  void ExtractSameTypeOrder(int cell) (line 583)
    -- Includes multi-cell building dedup via _sameTypeSeenIds
  void ExtractBiome(int cell) (line 613)
```

### Private — box-order target resolution

```
  void ResolveBoxOrderTargets() (line 632)
    -- After extraction: determines IsMixed and TargetName for each box-order cluster
  static void ResolveBoxOrderTarget(OrderCluster cluster, BoxOrderDef def) (line 640)
  static string GetBoxOrderTarget(int cell, BoxOrderDef def) (line 661)
    -- Dig → element name, Mop → element name, Sweep/Disinfect → null (no per-cell target)
```

### Private — result assembly

```
  GridScanResult BuildResult() (line 675)
```

### Private — helpers

```
  static OrderRouter.OrderType OrderTypeFromLabel(string label) (line 702)
  static string ReadSameTypeOrderName(int cell, string orderLabel) (line 714)
```
