namespace OniAccess.Handlers.Tiles.Overlays {
	/// <summary>
	/// Pairs an overlay's display name with the GlanceComposer
	/// that produces cell speech while that overlay is active.
	/// </summary>
	public sealed class OverlayProfile {
		public string OverlayName { get; }
		public GlanceComposer Composer { get; }

		public OverlayProfile(string overlayName, GlanceComposer composer) {
			OverlayName = overlayName;
			Composer = composer;
		}
	}
}
