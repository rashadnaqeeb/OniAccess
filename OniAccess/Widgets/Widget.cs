using System.Collections.Generic;

namespace OniAccess.Widgets {
	/// <summary>
	/// Base class for all navigable UI widgets. Subclasses define
	/// type-specific behavior (speech, validity, activation, adjustment)
	/// so dispatch happens via virtual calls instead of switch statements.
	/// </summary>
	public class Widget {
		public string Label { get; set; }
		public UnityEngine.Component Component { get; set; }
		public UnityEngine.GameObject GameObject { get; set; }
		public object Tag { get; set; }
		public System.Func<string> SpeechFunc { get; set; }
		public bool SuppressTooltip { get; set; }
		public List<Widget> Children { get; set; }

		/// <summary>
		/// Build speech text for this widget. SpeechFunc short-circuits
		/// if set; otherwise subclasses provide type-specific formatting.
		/// </summary>
		public virtual string GetSpeechText() {
			if (SpeechFunc != null) {
				string result = SpeechFunc()?.Trim();
				if (!string.IsNullOrEmpty(result)) return result;
			}
			return Label;
		}

		/// <summary>
		/// Whether the widget is still valid for navigation (active, interactable).
		/// </summary>
		public virtual bool IsValid() {
			if (GameObject != null && !GameObject.activeInHierarchy) return false;
			return Component != null || GameObject != null;
		}

		/// <summary>
		/// Activate the widget (click, toggle, begin editing). Returns true if handled.
		/// </summary>
		public virtual bool Activate() => false;

		/// <summary>
		/// Adjust the widget's value (slider step, dropdown cycle).
		/// Returns true if the value changed.
		/// </summary>
		public virtual bool Adjust(int direction, int stepLevel) => false;

		/// <summary>
		/// Whether this widget supports Left/Right adjustment.
		/// </summary>
		public virtual bool IsAdjustable => false;
	}
}
