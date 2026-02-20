using System.Collections.Generic;
using OniAccess.Handlers.Tiles.Scanner.Routing;
using UnityEngine;

namespace OniAccess.Handlers.Tiles.Scanner.Backends {
	/// <summary>
	/// Backend for Zones > Orders. Handles both clustered orders
	/// (box-selection and same-type) and individual orders (attack, capture,
	/// empty pipe). Receives pre-processed data from GridScanner.
	/// </summary>
	public class OrderBackend : IScannerBackend {
		private List<OrderCluster> _clusters;
		private List<IndividualOrder> _individuals;

		public void SetGridData(
				List<OrderCluster> clusters,
				List<IndividualOrder> individuals) {
			_clusters = clusters;
			_individuals = individuals;
		}

		public IEnumerable<ScanEntry> Scan(int worldId) {
			if (_clusters != null) {
				foreach (var cluster in _clusters) {
					string itemName = BuildOrderItemName(cluster);
					yield return new ScanEntry {
						Cell = cluster.Cells[0],
						Backend = this,
						BackendData = cluster,
						Category = ScannerTaxonomy.Categories.Zones,
						Subcategory = ScannerTaxonomy.Subcategories.Orders,
						ItemName = itemName,
					};
				}
			}

			if (_individuals != null) {
				foreach (var order in _individuals) {
					string itemName = string.Format(
						(string)STRINGS.ONIACCESS.SCANNER.ORDER_LABEL,
						order.OrderType.Label, order.EntityName);
					yield return new ScanEntry {
						Cell = order.Cell,
						Backend = this,
						BackendData = order,
						Category = ScannerTaxonomy.Categories.Zones,
						Subcategory = ScannerTaxonomy.Subcategories.Orders,
						ItemName = itemName,
					};
				}
			}
		}

		public bool ValidateEntry(ScanEntry entry, int cursorCell) {
			if (entry.BackendData is IndividualOrder individual)
				return ValidateIndividual(individual);

			var cluster = (OrderCluster)entry.BackendData;
			return ValidateCluster(cluster, entry, cursorCell);
		}

		public string FormatName(ScanEntry entry) {
			if (entry.BackendData is OrderCluster cluster) {
				string targetName = cluster.TargetName ?? "";
				if (cluster.Cells.Count == 1)
					return string.Format(
						(string)STRINGS.ONIACCESS.SCANNER.ORDER_LABEL,
						cluster.OrderType.Label, targetName);
				return string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.ORDER_CLUSTER_LABEL,
					cluster.Cells.Count, cluster.OrderType.Label, targetName);
			}

			var individual = (IndividualOrder)entry.BackendData;
			return string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.ORDER_LABEL,
				individual.OrderType.Label, individual.EntityName);
		}

		private static string BuildOrderItemName(OrderCluster cluster) {
			string targetName = cluster.TargetName;
			if (string.IsNullOrEmpty(targetName))
				return cluster.OrderType.Label;
			return string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.ORDER_LABEL,
				cluster.OrderType.Label, targetName);
		}

		private static bool ValidateIndividual(IndividualOrder order) {
			if (order.Entity == null || order.Entity.IsNullOrDestroyed())
				return false;

			if (order.OrderType.Strategy != OrderRouter.ClusterStrategy.Individual)
				return true;

			// Re-check that the order marker is still active
			if (order.OrderType.Label == OrderRouter.Attack.Label) {
				var faction = order.Entity.GetComponent<FactionAlignment>();
				return faction != null && faction.IsPlayerTargeted();
			}
			if (order.OrderType.Label == OrderRouter.Capture.Label) {
				var capturable = order.Entity.GetComponent<Capturable>();
				return capturable != null && capturable.IsMarkedForCapture;
			}
			// Empty pipe: check the entity still has the workable
			var workable = order.Entity.GetComponent<IEmptyConduitWorkable>();
			return !workable.IsNullOrDestroyed();
		}

		private static bool ValidateCluster(
				OrderCluster cluster, ScanEntry entry, int cursorCell) {
			int bestCell = -1;
			int bestDist = int.MaxValue;

			for (int i = cluster.Cells.Count - 1; i >= 0; i--) {
				int cell = cluster.Cells[i];
				if (!IsOrderStillPresent(cell, cluster)) {
					cluster.Cells.RemoveAt(i);
					continue;
				}
				int dist = CellDistance(cursorCell, cell);
				if (dist < bestDist) {
					bestDist = dist;
					bestCell = cell;
				}
			}

			if (bestCell < 0) return false;
			entry.Cell = bestCell;
			return true;
		}

		private static bool IsOrderStillPresent(int cell, OrderCluster cluster) {
			switch (cluster.OrderType.Strategy) {
				case OrderRouter.ClusterStrategy.BoxSelection:
					return IsBoxOrderPresent(cell, cluster.OrderType);
				case OrderRouter.ClusterStrategy.SameType:
					return IsSameTypeOrderPresent(cell, cluster.OrderType);
				default:
					return false;
			}
		}

		private static bool IsBoxOrderPresent(
				int cell, OrderRouter.OrderType orderType) {
			if (orderType.Label == OrderRouter.Dig.Label)
				return OrderRouter.HasDigOrder(cell);
			if (orderType.Label == OrderRouter.Mop.Label)
				return OrderRouter.HasMopOrder(cell);
			if (orderType.Label == OrderRouter.Sweep.Label)
				return OrderRouter.HasSweepOrder(cell);
			if (orderType.Label == OrderRouter.Disinfect.Label)
				return OrderRouter.HasDisinfectOrder(cell);
			return false;
		}

		private static bool IsSameTypeOrderPresent(
				int cell, OrderRouter.OrderType orderType) {
			if (orderType.Label == OrderRouter.Build.Label)
				return OrderRouter.GetBuildOrderType(cell) != null;
			if (orderType.Label == OrderRouter.Deconstruct.Label)
				return OrderRouter.GetDeconstructOrderType(cell) != null;
			if (orderType.Label == OrderRouter.Harvest.Label)
				return OrderRouter.GetHarvestOrderType(cell) != null;
			if (orderType.Label == OrderRouter.Uproot.Label)
				return OrderRouter.GetUprootOrderType(cell) != null;
			return false;
		}

		private static int CellDistance(int a, int b) {
			int dr = Grid.CellRow(a) - Grid.CellRow(b);
			int dc = Grid.CellColumn(a) - Grid.CellColumn(b);
			if (dr < 0) dr = -dr;
			if (dc < 0) dc = -dc;
			return dr + dc;
		}
	}
}
