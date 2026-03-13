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

		public TileDetailsComposer(BiomeNameResolver biomeResolver) {
			_biomeResolver = biomeResolver;
		}

		public string Compose(int cell) {
			var ctx = new CellContext();
			var tokens = new List<string>();

			AddSection(tokens, GlanceComposer.Temperature, cell, ctx);
			AddRoom(tokens, cell);
			AddSection(tokens, GlanceComposer.Disease, cell, ctx,
				skip: (string)STRINGS.ONIACCESS.GLANCE.DISEASE_CLEAR);

			if (Grid.Radiation[cell] > 0f)
				AddSection(tokens, GlanceComposer.Radiation, cell, ctx);

			if (Grid.LightIntensity[cell] > 0)
				AddSection(tokens, GlanceComposer.Light, cell, ctx);
			AddSection(tokens, GlanceComposer.Decor, cell, ctx);
			AddBiome(tokens, cell);

			if (tokens.Count == 0) return null;
			return string.Join(", ", tokens);
		}

		private static void AddSection(List<string> tokens, ICellSection section,
				int cell, CellContext ctx, string skip = null) {
			try {
				foreach (var token in section.Read(cell, ctx)) {
					if (!string.IsNullOrEmpty(token) && token != skip)
						tokens.Add(token);
				}
			} catch (System.Exception ex) {
				Util.Log.Error(
					$"TileDetailsComposer: {section.GetType().Name} threw: {ex}");
			}
		}

		private static void AddRoom(List<string> tokens, int cell) {
			try {
				var cavity = Game.Instance.roomProber.GetCavityForCell(cell);
				if (cavity?.room == null) return;
				if (cavity.room.roomType == Db.Get().RoomTypes.Neutral) return;
				tokens.Add(cavity.room.roomType.Name);
			} catch (System.Exception ex) {
				Util.Log.Error($"TileDetailsComposer: AddRoom threw: {ex}");
			}
		}

		private void AddBiome(List<string> tokens, int cell) {
			if (World.Instance?.zoneRenderData == null) return;
			var zoneType = World.Instance.zoneRenderData.GetSubWorldZoneType(cell);
			tokens.Add(_biomeResolver.GetName(zoneType));
		}
	}
}
