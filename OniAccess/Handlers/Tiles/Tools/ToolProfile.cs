namespace OniAccess.Handlers.Tiles.Tools {
	/// <summary>
	/// Pairs a tool's name with the GlanceComposer
	/// that produces cell speech while that tool is active.
	/// </summary>
	public sealed class ToolProfile {
		public string ToolName { get; }
		public GlanceComposer Composer { get; }

		public ToolProfile(string toolName, GlanceComposer composer) {
			ToolName = toolName;
			Composer = composer;
		}
	}
}
