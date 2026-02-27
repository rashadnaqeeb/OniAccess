// Runs an ordered list of ICellSection instances for a cell,
// concatenates their output tokens with ", ", and returns
// the final speech string.
//
// Constructed with an immutable section list. Phase 4
// OverlayProfileRegistry will construct different instances
// per overlay profile.

namespace OniAccess.Handlers.Tiles

class GlanceComposer (line 13)
  private readonly IReadOnlyList<ICellSection> _sections (line 14)

  // Shared stateless section singletons, reused across all overlay profiles.
  internal static readonly ICellSection Building (line 19)   // Sections.BuildingSection
  internal static readonly ICellSection Element (line 20)    // Sections.ElementSection
  internal static readonly ICellSection Entity (line 21)     // Sections.EntitySection
  internal static readonly ICellSection Order (line 22)      // Sections.OrderSection
  internal static readonly ICellSection Debris (line 23)     // Sections.DebrisSection
  internal static readonly ICellSection Light (line 24)      // Sections.LightSection
  internal static readonly ICellSection Radiation (line 25)  // Sections.RadiationSection
  internal static readonly ICellSection Decor (line 26)      // Sections.DecorSection
  internal static readonly ICellSection Disease (line 27)    // Sections.DiseaseSection
  internal static readonly ICellSection Power (line 28)      // Sections.PowerSection
  internal static readonly ICellSection Plumbing (line 29)   // Sections.PlumbingSection
  internal static readonly ICellSection Ventilation (line 30) // Sections.VentilationSection
  internal static readonly ICellSection Conveyor (line 31)   // Sections.ConveyorSection
  internal static readonly ICellSection Automation (line 32) // Sections.AutomationSection
  internal static readonly ICellSection Temperature (line 33) // Sections.TemperatureSection

  GlanceComposer(IReadOnlyList<ICellSection> sections) (line 35)

  string Compose(int cell) (line 44)
    // Builds the speech string for a visible cell. Creates a CellContext, runs
    // each section's Read(), collects non-empty tokens, joins with ", ".
    // Returns null if all sections produce empty output.
    // Fog-of-war gating is the caller's responsibility.
    // Exceptions from individual sections are caught and logged without aborting others.

  static GlanceComposer CreateDefault() (line 66)
    // Returns a GlanceComposer with sections [Element, Building, Entity, Order, Debris]
    // in that speech order. Used for the no-overlay (normal view) case.
