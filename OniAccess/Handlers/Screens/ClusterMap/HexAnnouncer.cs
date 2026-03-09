using System.Collections.Generic;

namespace OniAccess.Handlers.Screens.ClusterMap {
	/// <summary>
	/// Reads hex state (fog of war, entities, status items) and builds
	/// speech strings. All data is queried live from ClusterGrid.
	/// </summary>
	public static class HexAnnouncer {
		/// <summary>
		/// Build speech for the entities/state at a hex cell.
		/// Returns fog text for hidden/peeked, entity names for visible.
		/// </summary>
		public static string AnnounceHex(AxialI location) {
			var revealLevel = ClusterGrid.Instance.GetCellRevealLevel(location);
			switch (revealLevel) {
				case ClusterRevealLevel.Hidden:
					return (string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED;
				case ClusterRevealLevel.Peeked:
					return AnnouncePeeked(location);
				default:
					return AnnounceVisible(location);
			}
		}

		/// <summary>
		/// Read detailed info for all entities at a hex: name + status items.
		/// </summary>
		public static string AnnounceTooltip(AxialI location) {
			var revealLevel = ClusterGrid.Instance.GetCellRevealLevel(location);
			switch (revealLevel) {
				case ClusterRevealLevel.Hidden:
					return (string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED;
				case ClusterRevealLevel.Peeked:
					return AnnouncePeeked(location);
				default:
					return AnnounceVisibleDetailed(location);
			}
		}

		private static string AnnouncePeeked(AxialI location) {
			if (!ClusterGrid.Instance.IsValidCell(location))
				return (string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED;
			// Check if there are peeked entities (asteroids/POIs show as "unknown")
			var entities = ClusterGrid.Instance.GetHiddenEntitiesOfLayerAtCell(location);
			if (entities != null && entities.Count > 0)
				return (string)STRINGS.UI.CLUSTERMAP.TOOLTIP_PEEKED_HEX_WITH_OBJECT;
			return (string)STRINGS.ONIACCESS.CLUSTER_MAP.UNSEEN;
		}

		private static string AnnounceVisible(AxialI location) {
			var entities = ClusterGrid.Instance.GetVisibleEntitiesAtCell(location);
			if (entities == null || entities.Count == 0)
				return (string)STRINGS.UI.CLUSTERMAP.TOOLTIP_EMPTY_HEX;
			var names = new List<string>();
			foreach (var entity in entities) {
				if (entity.IsVisible)
					names.Add(entity.Name);
			}
			if (names.Count == 0)
				return (string)STRINGS.UI.CLUSTERMAP.TOOLTIP_EMPTY_HEX;
			return string.Join(", ", names);
		}

		private static string AnnounceVisibleDetailed(AxialI location) {
			var entities = ClusterGrid.Instance.GetVisibleEntitiesAtCell(location);
			if (entities == null || entities.Count == 0)
				return (string)STRINGS.UI.CLUSTERMAP.TOOLTIP_EMPTY_HEX;
			var parts = new List<string>();
			foreach (var entity in entities) {
				if (!entity.IsVisible) continue;
				string detail = BuildEntityDetail(entity);
				if (detail != null)
					parts.Add(detail);
			}
			if (parts.Count == 0)
				return (string)STRINGS.UI.CLUSTERMAP.TOOLTIP_EMPTY_HEX;
			return string.Join(". ", parts);
		}

		private static string BuildEntityDetail(ClusterGridEntity entity) {
			try {
				var selectable = entity.GetComponent<KSelectable>();
				if (selectable == null) return entity.Name;
				var group = selectable.GetStatusItemGroup();
				if (group == null) return entity.Name;
				var items = new List<string> { entity.Name };
				foreach (var statusEntry in group) {
					string name = statusEntry.GetName();
					if (!string.IsNullOrEmpty(name))
						items.Add(name);
				}
				return items.Count > 1
					? string.Join(", ", items)
					: entity.Name;
			} catch (System.Exception ex) {
				Util.Log.Warn($"HexAnnouncer.BuildEntityDetail: {ex.Message}");
				return entity.Name;
			}
		}

		/// <summary>
		/// Check if a hex contains an entity that can be selected via GetHiddenEntitiesOfLayerAtCell.
		/// Uses the game's API to detect hidden peeked entities.
		/// </summary>
		private static List<ClusterGridEntity> GetHiddenEntitiesOfLayerAtCell(
				this ClusterGrid grid, AxialI location) {
			if (!grid.cellContents.TryGetValue(location, out var entities))
				return null;
			var hidden = new List<ClusterGridEntity>();
			foreach (var entity in entities) {
				if (entity.IsVisible && entity.IsVisibleInFOW == ClusterRevealLevel.Peeked)
					hidden.Add(entity);
			}
			return hidden.Count > 0 ? hidden : null;
		}
	}
}
