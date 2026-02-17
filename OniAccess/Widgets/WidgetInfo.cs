namespace OniAccess.Widgets {
	/// <summary>
	/// Type classification for interactive widgets in a screen's navigation list.
	/// Used by BaseMenuHandler to dispatch activation and adjustment behavior.
	/// </summary>
	public enum WidgetType {
		Button,
		Toggle,
		Slider,
		Dropdown,
		Label,
		TextInput,
	}

	/// <summary>
	/// Metadata for a single interactive widget in a screen's navigation list.
	/// Captures everything a handler needs to speak, activate, and adjust the widget.
	/// </summary>
	public class WidgetInfo {
		/// <summary>
		/// Speakable text for the widget (e.g., button label, setting name).
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// The KButton/KToggle/KSlider/etc reference for programmatic interaction.
		/// </summary>
		public UnityEngine.Component Component { get; set; }

		/// <summary>
		/// What kind of widget this is, for dispatching activation/adjustment.
		/// </summary>
		public WidgetType Type { get; set; }

		/// <summary>
		/// The widget's GameObject, for tooltip access and hierarchy queries.
		/// </summary>
		public UnityEngine.GameObject GameObject { get; set; }

		/// <summary>
		/// Optional handler-specific data attached to this widget.
		/// Used by ColonySetupHandler to store cluster keys for activation.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Optional lambda that reads live widget state at speech time.
		/// When set, GetWidgetSpeechText uses this instead of the WidgetType switch.
		/// </summary>
		public System.Func<string> SpeechFunc { get; set; }
	}
}
