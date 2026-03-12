using System;

namespace OniAccess.Config {
	public abstract class ConfigItem {
		public string Label { get; }

		protected ConfigItem(string label) {
			Label = label;
		}

		public abstract string GetDisplayValue();
		public abstract void Cycle(int direction);
	}

	public class BoolConfigItem : ConfigItem {
		private readonly Func<bool> _getter;
		private readonly Action<bool> _setter;

		public BoolConfigItem(string label, Func<bool> getter, Action<bool> setter)
			: base(label) {
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
	}

	public class EnumConfigItem<T> : ConfigItem where T : struct {
		private readonly Func<T> _getter;
		private readonly Action<T> _setter;
		private readonly T[] _values;
		private readonly Func<T, string> _valueLabeler;

		public EnumConfigItem(string label, Func<T> getter, Action<T> setter,
				T[] values, Func<T, string> valueLabeler)
			: base(label) {
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
	}
}
