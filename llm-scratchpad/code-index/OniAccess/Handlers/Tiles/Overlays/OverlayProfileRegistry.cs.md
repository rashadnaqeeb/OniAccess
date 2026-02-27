// Maps overlay mode IDs to OverlayProfile instances.
// Falls back to the default composer for overlays without
// a custom profile.

namespace OniAccess.Handlers.Tiles.Overlays

sealed class OverlayProfileRegistry (line 9)
  private readonly Dictionary<HashedString, OverlayProfile> _profiles (line 10)
  private readonly GlanceComposer _defaultComposer (line 12)
  private readonly string _defaultName (line 13)

  OverlayProfileRegistry(GlanceComposer defaultComposer, string defaultName) (line 15)

  void Register(HashedString modeId, OverlayProfile profile) (line 20)

  GlanceComposer GetComposer(HashedString modeId) (line 24)
    // Returns the registered profile's composer, or _defaultComposer if not found.

  string GetOverlayName(HashedString modeId) (line 30)
    // Returns the registered profile's name, or _defaultName if not found.

  static OverlayProfileRegistry Build() (line 40)
    // Constructs the full registry with all known overlay IDs.
    // Custom profiles (overlay section prepended to defaults):
    //   Light, Radiation, Decor, Disease, Temperature, Power,
    //   LiquidConduits, GasConduits, SolidConveyor, Logic
    // Name-only entries (use default composer):
    //   Oxygen, Crop, Rooms, Suit, TileMode
    // Default sections array used in all custom profiles:
    //   [Element, Building, Entity, Order, Debris]

  private static void RegisterCustomProfile(
      OverlayProfileRegistry registry, HashedString modeId,
      string name, ICellSection overlaySection,
      ICellSection[] defaultSections) (line 106)
    // Creates a new GlanceComposer with overlaySection prepended to defaultSections,
    // wraps it in an OverlayProfile, and registers it.

  private static void RegisterNameOnly(
      OverlayProfileRegistry registry, HashedString modeId,
      string name, GlanceComposer composer) (line 117)
    // Registers an OverlayProfile with the given name and composer (no overlay section added).
