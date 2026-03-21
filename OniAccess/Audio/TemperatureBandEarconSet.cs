using System.Collections.Generic;
using OniAccess.Handlers.Tiles.Skip;

namespace OniAccess.Audio {
	public class TemperatureBandEarconSet: EarconSet {
		private int _lastBand = TemperatureBand.Vacuum;

		public override int Priority => 0;
		public override bool IsEnabled => ConfigManager.Config.TemperatureBandEarcons;
		public override float Volume => ConfigManager.Config.TemperatureBandVolume;
		public override bool IsActive(HashedString overlayMode) => true;

		public void Reset() {
			_lastBand = TemperatureBand.Vacuum;
		}

		public override List<SoundBatch> GetBatches(int cell) {
			int band = TemperatureBand.Classify(cell);
			int prev = _lastBand;
			_lastBand = band;

			if (prev == TemperatureBand.Vacuum || band == TemperatureBand.Vacuum)
				return new List<SoundBatch>();
			if (band == prev)
				return new List<SoundBatch>();

			string clip = band > prev ? "temp_rising" : "temp_falling";
			return new List<SoundBatch> { new SoundBatch(new SoundSpec(clip)) };
		}
	}
}
