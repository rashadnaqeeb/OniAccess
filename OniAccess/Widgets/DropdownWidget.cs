namespace OniAccess.Widgets {
	/// <summary>
	/// A dropdown/radio-group widget. Supports Left/Right cycling.
	/// Cycling logic stays in handlers (too handler-specific).
	/// </summary>
	public class DropdownWidget: Widget {
		public override bool IsAdjustable => true;
	}
}
