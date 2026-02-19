using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Overlays {
	/// <summary>
	/// Maps overlay mode IDs to OverlayProfile instances.
	/// Falls back to the default composer for overlays without
	/// a custom profile.
	/// </summary>
	public sealed class OverlayProfileRegistry {
		private readonly Dictionary<HashedString, OverlayProfile> _profiles
			= new Dictionary<HashedString, OverlayProfile>();
		private readonly GlanceComposer _defaultComposer;
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

		public string GetOverlayName(HashedString modeId) {
			if (_profiles.TryGetValue(modeId, out var profile))
				return profile.OverlayName;
			return _defaultName;
		}

		/// <summary>
		/// Construct the full registry with all overlay names and
		/// custom profiles for Light, Radiation, Decor, Disease.
		/// </summary>
		public static OverlayProfileRegistry Build() {
			var defaultComposer = GlanceComposer.CreateDefault();
			var registry = new OverlayProfileRegistry(
				defaultComposer,
				(string)STRINGS.ONIACCESS.TILE_CURSOR.OVERLAY_NONE);

			var defaultSections = new[] {
				GlanceComposer.Building, GlanceComposer.Entity,
				GlanceComposer.Element, GlanceComposer.Order,
				GlanceComposer.Debris
			};

			// Custom profiles: overlay section prepended to defaults
			RegisterCustomProfile(registry, OverlayModes.Light.ID,
				(string)STRINGS.UI.OVERLAYS.LIGHTING.BUTTON,
				GlanceComposer.Light, defaultSections);

			RegisterCustomProfile(registry, OverlayModes.Radiation.ID,
				(string)STRINGS.UI.OVERLAYS.RADIATION.BUTTON,
				GlanceComposer.Radiation, defaultSections);

			RegisterCustomProfile(registry, OverlayModes.Decor.ID,
				(string)STRINGS.UI.OVERLAYS.DECOR.BUTTON,
				GlanceComposer.Decor, defaultSections);

			RegisterCustomProfile(registry, OverlayModes.Disease.ID,
				(string)STRINGS.UI.OVERLAYS.DISEASE.BUTTON,
				GlanceComposer.Disease, defaultSections);

			// Name-only entries for all other overlays (use default composer)
			RegisterNameOnly(registry, OverlayModes.Oxygen.ID,
				(string)STRINGS.UI.OVERLAYS.OXYGEN.BUTTON, defaultComposer);
			RegisterCustomProfile(registry, OverlayModes.Temperature.ID,
				(string)STRINGS.UI.OVERLAYS.TEMPERATURE.BUTTON,
				GlanceComposer.Temperature, defaultSections);
			RegisterCustomProfile(registry, OverlayModes.Power.ID,
				(string)STRINGS.UI.OVERLAYS.ELECTRICAL.BUTTON,
				GlanceComposer.Power, defaultSections);

			RegisterCustomProfile(registry, OverlayModes.LiquidConduits.ID,
				(string)STRINGS.UI.OVERLAYS.LIQUIDPLUMBING.BUTTON,
				GlanceComposer.Plumbing, defaultSections);

			RegisterCustomProfile(registry, OverlayModes.GasConduits.ID,
				(string)STRINGS.UI.OVERLAYS.GASPLUMBING.BUTTON,
				GlanceComposer.Ventilation, defaultSections);

			RegisterCustomProfile(registry, OverlayModes.SolidConveyor.ID,
				(string)STRINGS.UI.OVERLAYS.CONVEYOR.BUTTON,
				GlanceComposer.Conveyor, defaultSections);

			RegisterCustomProfile(registry, OverlayModes.Logic.ID,
				(string)STRINGS.UI.OVERLAYS.LOGIC.BUTTON,
				GlanceComposer.Automation, defaultSections);
			RegisterNameOnly(registry, OverlayModes.Crop.ID,
				(string)STRINGS.UI.OVERLAYS.CROPS.BUTTON, defaultComposer);
			RegisterNameOnly(registry, OverlayModes.Rooms.ID,
				(string)STRINGS.UI.OVERLAYS.ROOMS.BUTTON, defaultComposer);
			RegisterNameOnly(registry, OverlayModes.Suit.ID,
				(string)STRINGS.UI.OVERLAYS.SUIT.BUTTON, defaultComposer);
			RegisterNameOnly(registry, OverlayModes.TileMode.ID,
				(string)STRINGS.UI.OVERLAYS.TILEMODE.BUTTON, defaultComposer);

			return registry;
		}

		private static void RegisterCustomProfile(
				OverlayProfileRegistry registry, HashedString modeId,
				string name, ICellSection overlaySection,
				ICellSection[] defaultSections) {
			var sections = new List<ICellSection>(defaultSections.Length + 1);
			sections.Add(overlaySection);
			sections.AddRange(defaultSections);
			var composer = new GlanceComposer(sections.AsReadOnly());
			registry.Register(modeId, new OverlayProfile(name, composer));
		}

		private static void RegisterNameOnly(
				OverlayProfileRegistry registry, HashedString modeId,
				string name, GlanceComposer composer) {
			registry.Register(modeId, new OverlayProfile(name, composer));
		}
	}
}
