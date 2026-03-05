using OniAccess.Handlers.Tiles.AreaScan;

namespace OniAccess.Handlers.Tiles.Overlays {
	/// <summary>
	/// Pairs an overlay's display name with the GlanceComposer
	/// that produces cell speech while that overlay is active,
	/// and an optional IAreaScanner for big cursor area summaries.
	/// </summary>
	public sealed class OverlayProfile {
		public string OverlayName { get; }
		public GlanceComposer Composer { get; }
		public IAreaScanner AreaScanner { get; }

		public OverlayProfile(string overlayName, GlanceComposer composer,
				IAreaScanner areaScanner = null) {
			OverlayName = overlayName;
			Composer = composer;
			AreaScanner = areaScanner;
		}
	}
}
