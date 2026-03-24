using System.Collections.Generic;
using OniAccess.Handlers.Tiles.AreaScan;

namespace OniAccess.Handlers.Tiles.Overlays {
	/// <summary>
	/// Maps overlay mode IDs to OverlayProfile instances.
	/// Falls back to the default composer/scanner for overlays without
	/// a custom profile.
	/// </summary>
	public sealed class OverlayProfileRegistry {
		private readonly Dictionary<HashedString, OverlayProfile> _profiles
			= new Dictionary<HashedString, OverlayProfile>();
		private readonly GlanceComposer _defaultComposer;
		private readonly IAreaScanner _defaultScanner = new DefaultAreaScanner();
		private readonly string _defaultName;

		public OverlayProfileRegistry(GlanceComposer defaultComposer, string defaultName) {
			_defaultComposer = defaultComposer;
			_defaultName = defaultName;
		}

		public void Register(HashedString modeId, OverlayProfile profile) {
			_profiles[modeId] = profile;
		}

		public GlanceComposer GetComposer(HashedString modeId) {
			if (_profiles.TryGetValue(modeId, out var profile))
				return profile.Composer;
			return _defaultComposer;
		}

		public IAreaScanner GetAreaScanner(HashedString modeId) {
			if (_profiles.TryGetValue(modeId, out var profile)
				&& profile.AreaScanner != null)
				return profile.AreaScanner;
			return _defaultScanner;
		}

		public string GetOverlayName(HashedString modeId) {
			if (_profiles.TryGetValue(modeId, out var profile))
				return profile.OverlayName;
			return _defaultName;
		}

		/// <summary>
		/// Construct the full registry with all overlay profiles,
		/// glance composers, and area scanners.
		/// </summary>
		public static OverlayProfileRegistry Build() {
			var defaultComposer = GlanceComposer.CreateDefault();
			var registry = new OverlayProfileRegistry(
				defaultComposer,
				(string)STRINGS.ONIACCESS.TILE_CURSOR.OVERLAY_NONE);

			var defaultSections = new[] {
				GlanceComposer.Building, GlanceComposer.Element,
				GlanceComposer.Entity, GlanceComposer.Order,
				GlanceComposer.Debris
			};

			// Custom profiles: overlay section prepended to defaults
			RegisterCustomProfile(registry, OverlayModes.Light.ID,
				(string)STRINGS.UI.OVERLAYS.LIGHTING.BUTTON,
				GlanceComposer.Light, defaultSections,
				new LightAreaScanner());

			RegisterCustomProfile(registry, OverlayModes.Radiation.ID,
				(string)STRINGS.UI.OVERLAYS.RADIATION.BUTTON,
				GlanceComposer.Radiation, defaultSections,
				new RadiationAreaScanner());

			RegisterCustomProfile(registry, OverlayModes.Decor.ID,
				(string)STRINGS.UI.OVERLAYS.DECOR.BUTTON,
				GlanceComposer.Decor, defaultSections,
				new DecorAreaScanner());

			RegisterCustomProfile(registry, OverlayModes.Disease.ID,
				(string)STRINGS.UI.OVERLAYS.DISEASE.BUTTON,
				GlanceComposer.Disease, defaultSections,
				new DiseaseAreaScanner());

			RegisterNameOnly(registry, OverlayModes.Oxygen.ID,
				(string)STRINGS.UI.OVERLAYS.OXYGEN.BUTTON, defaultComposer,
				new OxygenAreaScanner());
			var tempSections = new[] {
				GlanceComposer.Element, GlanceComposer.Building,
				GlanceComposer.Entity, GlanceComposer.Debris,
				GlanceComposer.Order
			};
			RegisterCustomProfile(registry, OverlayModes.Temperature.ID,
				(string)STRINGS.UI.OVERLAYS.TEMPERATURE.BUTTON,
				GlanceComposer.Temperature, tempSections,
				new TemperatureAreaScanner());

			// Utility overlays: use overlay-specific Order sections so
			// deconstruction orders on conduit layers are announced.
			RegisterCustomProfile(registry, OverlayModes.Power.ID,
				(string)STRINGS.UI.OVERLAYS.ELECTRICAL.BUTTON,
				GlanceComposer.Power,
				SectionsWithOrder(defaultSections, GlanceComposer.OrderPower));

			RegisterCustomProfile(registry, OverlayModes.LiquidConduits.ID,
				(string)STRINGS.UI.OVERLAYS.LIQUIDPLUMBING.BUTTON,
				GlanceComposer.Plumbing,
				SectionsWithOrder(defaultSections, GlanceComposer.OrderPlumbing));

			RegisterCustomProfile(registry, OverlayModes.GasConduits.ID,
				(string)STRINGS.UI.OVERLAYS.GASPLUMBING.BUTTON,
				GlanceComposer.Ventilation,
				SectionsWithOrder(defaultSections, GlanceComposer.OrderVentilation));

			RegisterCustomProfile(registry, OverlayModes.SolidConveyor.ID,
				(string)STRINGS.UI.OVERLAYS.CONVEYOR.BUTTON,
				GlanceComposer.Conveyor,
				SectionsWithOrder(defaultSections, GlanceComposer.OrderConveyor));

			RegisterCustomProfile(registry, OverlayModes.Logic.ID,
				(string)STRINGS.UI.OVERLAYS.LOGIC.BUTTON,
				GlanceComposer.Automation,
				SectionsWithOrder(defaultSections, GlanceComposer.OrderAutomation));

			RegisterNameOnly(registry, OverlayModes.Crop.ID,
				(string)STRINGS.UI.OVERLAYS.CROPS.BUTTON, defaultComposer,
				new CropsAreaScanner());
			RegisterNameOnly(registry, OverlayModes.Rooms.ID,
				(string)STRINGS.UI.OVERLAYS.ROOMS.BUTTON, defaultComposer,
				new RoomsAreaScanner());
			RegisterNameOnly(registry, OverlayModes.Suit.ID,
				(string)STRINGS.UI.OVERLAYS.SUIT.BUTTON, defaultComposer);
			RegisterNameOnly(registry, OverlayModes.TileMode.ID,
				(string)STRINGS.UI.OVERLAYS.TILEMODE.BUTTON, defaultComposer,
				new MaterialsAreaScanner());

			return registry;
		}

		private static ICellSection[] SectionsWithOrder(
				ICellSection[] baseSections, ICellSection orderSection) {
			var result = new ICellSection[baseSections.Length];
			for (int i = 0; i < baseSections.Length; i++)
				result[i] = baseSections[i] == GlanceComposer.Order
					? orderSection : baseSections[i];
			return result;
		}

		private static void RegisterCustomProfile(
				OverlayProfileRegistry registry, HashedString modeId,
				string name, ICellSection overlaySection,
				ICellSection[] defaultSections,
				IAreaScanner areaScanner = null) {
			var sections = new List<ICellSection>(defaultSections.Length + 1);
			sections.Add(overlaySection);
			sections.AddRange(defaultSections);
			var composer = new GlanceComposer(sections.AsReadOnly());
			registry.Register(modeId,
				new OverlayProfile(name, composer, areaScanner));
		}

		private static void RegisterNameOnly(
				OverlayProfileRegistry registry, HashedString modeId,
				string name, GlanceComposer composer,
				IAreaScanner areaScanner = null) {
			registry.Register(modeId,
				new OverlayProfile(name, composer, areaScanner));
		}
	}
}
