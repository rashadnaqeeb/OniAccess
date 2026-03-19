namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips until the set of disease types changes. Checks all sources:
	/// tile surface, buildings (including stored items), pickupables,
	/// and conduit contents (liquid, gas, and solid).
	/// Uses a bitmask of present disease indices as the signature.
	/// </summary>
	public class DiseaseSkipStrategy: ISkipStrategy {
		public object GetSignature(int cell) {
			int mask = 0;
			AccumulateBit(ref mask, Grid.DiseaseIdx[cell], Grid.DiseaseCount[cell]);
			AddBuildings(cell, ref mask);
			AddPickupables(cell, ref mask);
			AddConduits(cell, ref mask);
			return mask;
		}

		private static void AccumulateBit(ref int mask, byte idx, int count) {
			if (idx != byte.MaxValue && count > 0)
				mask |= 1 << idx;
		}

		private static void AddBuildings(int cell, ref int mask) {
			AddBuildingLayer(cell, ObjectLayer.Building, ref mask);
			AddBuildingLayer(cell, ObjectLayer.FoundationTile, ref mask);
			AddStorage(cell, ObjectLayer.Building, ref mask);
			AddStorage(cell, ObjectLayer.FoundationTile, ref mask);
		}

		private static void AddBuildingLayer(
				int cell, ObjectLayer layer, ref int mask) {
			var go = Grid.Objects[cell, (int)layer];
			if (go == null) return;
			var pe = go.GetComponent<PrimaryElement>();
			if (pe == null) return;
			AccumulateBit(ref mask, pe.DiseaseIdx, pe.DiseaseCount);
		}

		private static void AddStorage(
				int cell, ObjectLayer layer, ref int mask) {
			var go = Grid.Objects[cell, (int)layer];
			if (go == null) return;
			var storage = go.GetComponent<Storage>();
			if (storage == null) return;
			foreach (var item in storage.items) {
				if (item == null) continue;
				var pe = item.GetComponent<PrimaryElement>();
				if (pe == null) continue;
				AccumulateBit(ref mask, pe.DiseaseIdx, pe.DiseaseCount);
			}
		}

		private static void AddPickupables(int cell, ref int mask) {
			var headGo = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (headGo == null) return;
			var pickupable = headGo.GetComponent<Pickupable>();
			if (pickupable == null) return;
			for (var item = pickupable.objectLayerListItem;
				item != null;
				item = item.nextItem) {
				var pe = item.gameObject.GetComponent<PrimaryElement>();
				if (pe == null) continue;
				AccumulateBit(ref mask, pe.DiseaseIdx, pe.DiseaseCount);
			}
		}

		private static void AddConduits(int cell, ref int mask) {
			AddConduitFlow(Game.Instance.liquidConduitFlow, cell, ref mask);
			AddConduitFlow(Game.Instance.gasConduitFlow, cell, ref mask);
			AddSolidConduit(cell, ref mask);
		}

		private static void AddConduitFlow(
				ConduitFlow flow, int cell, ref int mask) {
			var contents = flow.GetContents(cell);
			AccumulateBit(ref mask, contents.diseaseIdx, contents.diseaseCount);
		}

		private static void AddSolidConduit(int cell, ref int mask) {
			var contents = Game.Instance.solidConduitFlow.GetContents(cell);
			if (!contents.pickupableHandle.IsValid()) return;
			var pickupable = Game.Instance.solidConduitFlow.GetPickupable(
				contents.pickupableHandle);
			if (pickupable == null) return;
			var pe = pickupable.GetComponent<PrimaryElement>();
			if (pe == null) return;
			AccumulateBit(ref mask, pe.DiseaseIdx, pe.DiseaseCount);
		}
	}
}
