// Pairs an overlay's display name with the GlanceComposer
// that produces cell speech while that overlay is active.

namespace OniAccess.Handlers.Tiles.Overlays

sealed class OverlayProfile (line 6)
  string OverlayName { get; } (line 7)
  GlanceComposer Composer { get; } (line 8)

  OverlayProfile(string overlayName, GlanceComposer composer) (line 10)
