namespace OniAccess.Handlers.Tiles.Scanner.Routing {
	/// <summary>
	/// Per-network-type configuration mapping grid layers to scanner
	/// categories. Used by GridScanner to know which layers to check
	/// and how to route segment clusters and bridge instances.
	/// </summary>
	public struct NetworkType {
		public string ScannerCategory;
		public string ScannerSubcategory;
		public ObjectLayer SegmentLayer;
		public ObjectLayer BridgeLayer;
		public System.Func<IUtilityNetworkMgr> GetManager;
	}

	public static class NetworkLayerConfig {
		public static readonly NetworkType[] Types = {
			new NetworkType {
				ScannerCategory = ScannerTaxonomy.Categories.Networks,
				ScannerSubcategory = ScannerTaxonomy.Subcategories.Power,
				SegmentLayer = ObjectLayer.Wire,
				BridgeLayer = ObjectLayer.WireConnectors,
				GetManager = () => Game.Instance.electricalConduitSystem,
			},
			new NetworkType {
				ScannerCategory = ScannerTaxonomy.Categories.Networks,
				ScannerSubcategory = ScannerTaxonomy.Subcategories.Liquid,
				SegmentLayer = ObjectLayer.LiquidConduit,
				BridgeLayer = ObjectLayer.LiquidConduitConnection,
				GetManager = () => Game.Instance.liquidConduitSystem,
			},
			new NetworkType {
				ScannerCategory = ScannerTaxonomy.Categories.Networks,
				ScannerSubcategory = ScannerTaxonomy.Subcategories.Gas,
				SegmentLayer = ObjectLayer.GasConduit,
				BridgeLayer = ObjectLayer.GasConduitConnection,
				GetManager = () => Game.Instance.gasConduitSystem,
			},
			new NetworkType {
				ScannerCategory = ScannerTaxonomy.Categories.Networks,
				ScannerSubcategory = ScannerTaxonomy.Subcategories.Conveyor,
				SegmentLayer = ObjectLayer.SolidConduit,
				BridgeLayer = ObjectLayer.SolidConduitConnection,
				GetManager = () => Game.Instance.solidConduitSystem,
			},
			new NetworkType {
				ScannerCategory = ScannerTaxonomy.Categories.Networks,
				ScannerSubcategory = ScannerTaxonomy.Subcategories.Transport,
				SegmentLayer = ObjectLayer.TravelTube,
				BridgeLayer = ObjectLayer.TravelTubeConnection,
				GetManager = () => Game.Instance.travelTubeSystem,
			},
			new NetworkType {
				ScannerCategory = ScannerTaxonomy.Categories.Automation,
				ScannerSubcategory = ScannerTaxonomy.Subcategories.Wires,
				SegmentLayer = ObjectLayer.LogicWire,
				BridgeLayer = ObjectLayer.LogicGate,
				GetManager = () => Game.Instance.logicCircuitSystem,
			},
		};
	}
}
