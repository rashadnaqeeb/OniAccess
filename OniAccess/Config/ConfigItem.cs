using System;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Config {
	public abstract class ConfigItem {
		public string Label { get; }

		/// <summary>
		/// Optional supplementary text spoken after the row's body and role, routed
		/// through the same tooltip slot as live widgets. Null when the row has none.
		/// </summary>
		public Func<string> Tooltip { get; }

		/// <summary>
		/// Optional live predicate deciding whether the row is listed at all. Null
		/// means always. Evaluated on every tree build, so a row can appear or vanish
		/// as a sibling setting changes (voice rows follow the speech backend).
		/// </summary>
		public Func<bool> Visible { get; set; }

		protected ConfigItem(string label, Func<string> tooltip = null) {
			Label = label;
			Tooltip = tooltip;
		}

		public bool IsVisible() => Visible == null || Visible();

		public abstract string GetDisplayValue();
		public abstract void Cycle(int direction);

		/// <summary>
		/// Verbose control role spoken for this row (toggle, picker, slider, button).
		/// </summary>
		public abstract string RoleKey { get; }
	}

	public class BoolConfigItem: ConfigItem {
		private readonly Func<bool> _getter;
		private readonly Action<bool> _setter;

		public BoolConfigItem(string label, Func<bool> getter, Action<bool> setter,
				Func<string> tooltip = null)
			: base(label, tooltip) {
			_getter = getter;
			_setter = setter;
		}

		public override string GetDisplayValue() {
			return _getter()
				? (string)STRINGS.ONIACCESS.STATES.ON
				: (string)STRINGS.ONIACCESS.STATES.OFF;
		}

		public override void Cycle(int direction) {
			_setter(!_getter());
			ConfigManager.Save();
		}

		public override string RoleKey => NavRoles.Toggle;
	}

	public class FloatConfigItem: ConfigItem {
		private readonly Func<float> _getter;
		private readonly Action<float> _setter;
		private readonly float _min;
		private readonly float _max;

		public FloatConfigItem(string label, Func<float> getter, Action<float> setter,
				float min, float max)
			: base(label) {
			_getter = getter;
			_setter = setter;
			_min = min;
			_max = max;
		}

		public override string GetDisplayValue() {
			return _getter().ToString("F2");
		}

		public override void Cycle(int direction) {
			Adjust(direction, 0.01f);
		}

		public void Adjust(int direction, float step) {
			float value = _getter() + direction * step;
			value = Mathf.Clamp(value, _min, _max);
			value = Mathf.Round(value * 100f) / 100f;
			_setter(value);
			ConfigManager.Save();
		}

		public override string RoleKey => NavRoles.Slider;
	}

	/// <summary>
	/// A whole-number slider (voice rate and volume on a 0-100 scale). Enter and
	/// Left/Right step by 1; Shift and Ctrl use the larger whole-number steps.
	/// </summary>
	public class IntConfigItem: ConfigItem {
		private readonly Func<int> _getter;
		private readonly Action<int> _setter;
		private readonly int _min;
		private readonly int _max;

		public IntConfigItem(string label, Func<int> getter, Action<int> setter,
				int min, int max)
			: base(label) {
			_getter = getter;
			_setter = setter;
			_min = min;
			_max = max;
		}

		public override string GetDisplayValue() {
			return _getter().ToString();
		}

		public override void Cycle(int direction) {
			Adjust(direction, 1);
		}

		public void Adjust(int direction, int step) {
			int value = Mathf.Clamp(_getter() + direction * step, _min, _max);
			_setter(value);
			ConfigManager.Save();
		}

		public override string RoleKey => NavRoles.Slider;
	}

	/// <summary>
	/// A config row that runs an action on Enter instead of cycling a value
	/// (e.g. opening a sub-menu). The optional value provider supplies the
	/// spoken suffix; when null the row speaks only its label.
	/// </summary>
	public class ActionConfigItem: ConfigItem {
		private readonly System.Action _action;
		private readonly Func<string> _valueProvider;

		public ActionConfigItem(string label, System.Action action, Func<string> valueProvider = null)
			: base(label) {
			_action = action;
			_valueProvider = valueProvider;
		}

		public override string GetDisplayValue() {
			return _valueProvider != null ? _valueProvider() : "";
		}

		public override void Cycle(int direction) {
			_action();
		}

		public override string RoleKey => NavRoles.Button;
	}

	public class EnumConfigItem<T>: ConfigItem where T : struct {
		private readonly Func<T> _getter;
		private readonly Action<T> _setter;
		private readonly T[] _values;
		private readonly Func<T, string> _valueLabeler;

		public EnumConfigItem(string label, Func<T> getter, Action<T> setter,
				T[] values, Func<T, string> valueLabeler, Func<string> tooltip = null)
			: base(label, tooltip) {
			_getter = getter;
			_setter = setter;
			_values = values;
			_valueLabeler = valueLabeler;
		}

		public override string GetDisplayValue() {
			return _valueLabeler(_getter());
		}

		public override void Cycle(int direction) {
			T current = _getter();
			int index = Array.IndexOf(_values, current);
			if (index < 0) index = 0;
			index = (index + direction + _values.Length) % _values.Length;
			_setter(_values[index]);
			ConfigManager.Save();
		}

		public override string RoleKey => NavRoles.Dropdown;
	}
}
