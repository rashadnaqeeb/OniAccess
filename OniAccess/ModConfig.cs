using OniAccess.Handlers.Tiles;

namespace OniAccess {
	public class ModConfig {
		public CoordinateMode CoordinateMode { get; set; } = CoordinateMode.Off;
		public bool AutoMoveCursor { get; set; } = false;
		public bool ScannerMassReadout { get; set; } = true;
		public bool LockZoom { get; set; } = true;
		public bool UtilityPresenceEarcons { get; set; } = false;
		public bool PipeShapeEarcons { get; set; } = false;
		public bool PassabilityEarcons { get; set; } = false;
		public bool AnnounceBiomeChanges { get; set; } = true;
		public bool FlowSonification { get; set; } = false;
		public bool TemperatureBandEarcons { get; set; } = false;
		public bool FollowMovementEarcons { get; set; } = false;
		public bool FootstepEarcons { get; set; } = true;
	}
}
