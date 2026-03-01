namespace OniAccess.Input {
	public class TextEditHelper {
		public bool IsEditing { get; private set; }
		private string _cachedValue;
		private System.Func<KInputTextField> _fieldAccessor;
		private System.Action _onEnd;

		public void Begin(KInputTextField field, System.Action onEnd = null) {
			_cachedValue = field.text;
			_fieldAccessor = () => field;
			_onEnd = onEnd;
			IsEditing = true;
			field.ActivateInputField();
			Speech.SpeechPipeline.SpeakInterrupt($"{STRINGS.ONIACCESS.TEXT_EDIT.EDITING}, {field.text}");
		}

		public void Begin(System.Func<KInputTextField> accessor, System.Action onEnd = null) {
			var field = accessor();
			if (field == null) return;
			_cachedValue = field.text;
			_fieldAccessor = accessor;
			_onEnd = onEnd;
			IsEditing = true;
			field.gameObject.SetActive(true);
			field.text = _cachedValue;
			field.Select();
			field.ActivateInputField();
			Speech.SpeechPipeline.SpeakInterrupt($"{STRINGS.ONIACCESS.TEXT_EDIT.EDITING}, {_cachedValue}");
		}

		/// <summary>
		/// Call from the owner's Tick(). Returns true while editing (caller should
		/// block further input). Handles Enter to confirm.
		/// </summary>
		public bool HandleTick() {
			if (!IsEditing) return false;
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
				Confirm();
				_onEnd?.Invoke();
			}
			return true;
		}

		/// <summary>
		/// Call from the owner's HandleKeyDown(). Returns true if the event was
		/// consumed (Escape to cancel).
		/// </summary>
		public bool HandleKeyDown(KButtonEvent e) {
			if (!IsEditing) return false;
			if (e.TryConsume(Action.Escape)) {
				Cancel();
				_onEnd?.Invoke();
				return true;
			}
			return false;
		}

		public void Confirm() {
			IsEditing = false;
			var field = _fieldAccessor?.Invoke();
			if (field != null) {
				field.DeactivateInputField();
				Speech.SpeechPipeline.SpeakInterrupt($"{STRINGS.ONIACCESS.TEXT_EDIT.CONFIRMED}, {field.text}");
			} else {
				Util.Log.Warn("TextEditHelper.Confirm: field accessor returned null, treating as cancel");
				Speech.SpeechPipeline.SpeakInterrupt($"{STRINGS.ONIACCESS.TEXT_EDIT.CANCELLED}, {_cachedValue}");
			}
		}

		public void Cancel() {
			IsEditing = false;
			var field = _fieldAccessor?.Invoke();
			if (field != null) {
				field.text = _cachedValue;
				field.DeactivateInputField();
				Speech.SpeechPipeline.SpeakInterrupt($"{STRINGS.ONIACCESS.TEXT_EDIT.CANCELLED}, {field.text}");
			} else {
				Util.Log.Warn("TextEditHelper.Cancel: field accessor returned null");
				Speech.SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TEXT_EDIT.CANCELLED);
			}
		}
	}
}
