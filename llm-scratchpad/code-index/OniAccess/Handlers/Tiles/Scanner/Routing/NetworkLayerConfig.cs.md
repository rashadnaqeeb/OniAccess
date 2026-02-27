# NetworkLayerConfig.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Routing`

Static configuration mapping network types to grid layers and scanner category/subcategory pairs. Used by `GridScanner` to know which object layers to inspect for segment clustering and bridge detection.

```
struct NetworkType (line 7)
  string ScannerCategory (line 8)
  string ScannerSubcategory (line 9)
  ObjectLayer SegmentLayer (line 10)   -- the layer containing wire/conduit segment objects
  ObjectLayer BridgeLayer (line 11)   -- the layer containing bridge/crossing objects

static class NetworkLayerConfig (line 14)
  static NetworkType[] Types (line 15)
    -- 6 entries in order:
    --   Power:     Wire / WireConnectors            → Networks > Power
    --   Liquid:    LiquidConduit / LiquidConduitConnection → Networks > Liquid
    --   Gas:       GasConduit / GasConduitConnection → Networks > Gas
    --   Conveyor:  SolidConduit / SolidConduitConnection → Networks > Conveyor
    --   Transport: TravelTube / TravelTubeConnection → Networks > Transport
    --   Automation: LogicWire / LogicGate           → Automation > Wires
```
