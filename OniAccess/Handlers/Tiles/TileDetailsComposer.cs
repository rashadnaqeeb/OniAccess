using System.Collections.Generic;
using OniAccess.Handlers.Tiles.Scanner.Routing;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Composes an environmental details string for a single cell.
	/// Reads temperature, room type, germs, radiation, light, decor,
	/// and biome — skipping irrelevant items (no room, clean, 0 rads).
	/// </summary>
	public class TileDetailsComposer {
		private readonly BiomeNameResolver _biomeResolver;
		private readonly CellContext _ctx = new CellContext();

		public TileDetailsComposer(BiomeNameResolver biomeResolver) {
			_biomeResolver = biomeResolver;
		}

		public string Compose(int cell) {
			var tokens = new List<string>();

			AddSection(tokens, GlanceComposer.Temperature, cell);
			AddRoom(tokens, cell);
			AddDisease(tokens, cell);

			if (Grid.Radiation[cell] > 0f)
				AddSection(tokens, GlanceComposer.Radiation, cell);

			AddSection(tokens, GlanceComposer.Light, cell);
			AddSection(tokens, GlanceComposer.Decor, cell);
			AddBiome(tokens, cell);

			if (tokens.Count == 0) return null;
			return string.Join(", ", tokens);
		}

		private void AddSection(List<string> tokens, ICellSection section, int cell) {
			try {
				foreach (var token in section.Read(cell, _ctx)) {
					if (!string.IsNullOrEmpty(token))
						tokens.Add(token);
				}
			} catch (System.Exception ex) {
				Util.Log.Error(
					$"TileDetailsComposer: {section.GetType().Name} threw: {ex}");
			}
		}

		private static void AddRoom(List<string> tokens, int cell) {
			var cavity = Game.Instance.roomProber.GetCavityForCell(cell);
			if (cavity?.room == null) return;
			if (cavity.room.roomType == Db.Get().RoomTypes.Neutral) return;
			tokens.Add(cavity.room.roomType.Name);
		}

		private void AddDisease(List<string> tokens, int cell) {
			try {
				foreach (var token in GlanceComposer.Disease.Read(cell, _ctx)) {
					if (!string.IsNullOrEmpty(token)
						&& token != (string)STRINGS.ONIACCESS.GLANCE.DISEASE_CLEAR)
						tokens.Add(token);
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"TileDetailsComposer: DiseaseSection threw: {ex}");
			}
		}

		private void AddBiome(List<string> tokens, int cell) {
			if (World.Instance?.zoneRenderData == null) return;
			var zoneType = World.Instance.zoneRenderData.GetSubWorldZoneType(cell);
			tokens.Add(_biomeResolver.GetName(zoneType));
		}
	}
}
