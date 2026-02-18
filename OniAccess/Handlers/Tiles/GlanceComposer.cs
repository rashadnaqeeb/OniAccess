using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Runs an ordered list of ICellSection instances for a cell,
	/// concatenates their output tokens with ", ", and returns
	/// the final speech string.
	///
	/// Constructed with an immutable section list. Phase 4
	/// OverlayProfileRegistry will construct different instances
	/// per overlay profile.
	/// </summary>
	public class GlanceComposer {
		private readonly IReadOnlyList<ICellSection> _sections;

		/// <summary>
		/// Shared stateless section instances, reused across profiles.
		/// </summary>
		internal static readonly ICellSection Building = new Sections.BuildingSection();
		internal static readonly ICellSection Element = new Sections.ElementSection();
		internal static readonly ICellSection Entity = new Sections.EntitySection();
		internal static readonly ICellSection Order = new Sections.OrderSection();
		internal static readonly ICellSection Debris = new Sections.DebrisSection();
		internal static readonly ICellSection Light = new Sections.LightSection();
		internal static readonly ICellSection Radiation = new Sections.RadiationSection();
		internal static readonly ICellSection Decor = new Sections.DecorSection();
		internal static readonly ICellSection Disease = new Sections.DiseaseSection();

		public GlanceComposer(IReadOnlyList<ICellSection> sections) {
			_sections = sections;
		}

		/// <summary>
		/// Build the speech string for a visible cell. Returns null
		/// if all sections produce empty output.
		/// Fog-of-war gating is the caller's responsibility.
		/// </summary>
		public string Compose(int cell) {
			var tokens = new List<string>();
			foreach (var section in _sections) {
				try {
					foreach (var token in section.Read(cell)) {
						if (!string.IsNullOrEmpty(token))
							tokens.Add(token);
					}
				} catch (System.Exception ex) {
					Util.Log.Error(
						$"GlanceComposer: {section.GetType().Name} threw: {ex}");
				}
			}
			if (tokens.Count == 0) return null;
			return string.Join(", ", tokens);
		}

		/// <summary>
		/// Create the default (no-overlay) glance composer with all
		/// five standard sections in speech order.
		/// </summary>
		public static GlanceComposer CreateDefault() {
			return new GlanceComposer(new List<ICellSection> {
				Building, Entity, Element, Order, Debris
			}.AsReadOnly());
		}
	}
}
