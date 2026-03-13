using System.Collections.Generic;

namespace OniAccess.Audio {
	public class UtilityPresenceEarconSet : EarconSet {
		public override int Priority => 2;
		public override bool IsEnabled => ConfigManager.Config.UtilityPresenceEarcons;

		public override bool IsActive(HashedString overlayMode) {
			return overlayMode == OverlayModes.None.ID;
		}

		public override List<SoundBatch> GetBatches(int cell) {
			var batches = new List<SoundBatch>();

			var wireBatch = GetWireBatch(cell);
			if (wireBatch != null)
				batches.Add(wireBatch.Value);

			var pipeRailBatch = GetPipeRailBatch(cell);
			if (pipeRailBatch != null)
				batches.Add(pipeRailBatch.Value);

			return batches;
		}

		private SoundBatch? GetWireBatch(int cell) {
			bool hasPower = Grid.Objects[cell, (int)ObjectLayer.Wire] != null;
			bool hasLogic = Grid.Objects[cell, (int)ObjectLayer.LogicWire] != null;

			if (hasPower && hasLogic)
				return new SoundBatch(new SoundSpec("wire both"));
			if (hasPower)
				return new SoundBatch(new SoundSpec("wire power"));
			if (hasLogic)
				return new SoundBatch(new SoundSpec("wire automation"));
			return null;
		}

		private SoundBatch? GetPipeRailBatch(int cell) {
			bool hasLiquid = Grid.Objects[cell, (int)ObjectLayer.LiquidConduit] != null;
			bool hasGas = Grid.Objects[cell, (int)ObjectLayer.GasConduit] != null;
			bool hasRail = Grid.Objects[cell, (int)ObjectLayer.SolidConduit] != null;

			var specs = new List<SoundSpec>();

			if (hasLiquid && hasGas)
				specs.Add(new SoundSpec("pipe both"));
			else if (hasLiquid)
				specs.Add(new SoundSpec("pipe liquid"));
			else if (hasGas)
				specs.Add(new SoundSpec("pipe gas"));

			if (hasRail)
				specs.Add(new SoundSpec("rail"));

			if (specs.Count == 0) return null;
			return new SoundBatch(specs.ToArray());
		}
	}
}
