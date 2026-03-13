using OniAccess.Handlers.Tiles;

namespace OniAccess {
	public class ModConfig {
		public CoordinateMode CoordinateMode { get; set; } = CoordinateMode.Off;
		public bool AutoMoveCursor { get; set; } = false;
		public bool LockZoom { get; set; } = true;
		public bool UtilityPresenceEarcons { get; set; } = true;
		public bool PipeShapeEarcons { get; set; } = true;
		public bool PassabilityEarcons { get; set; } = true;
		public bool AnnounceBiomeChanges { get; set; } = true;
	}
}
