namespace OniAccess.Input {
	public class TextEditHelper {
		public bool IsEditing { get; private set; }
		private string _cachedValue;
		private System.Func<KInputTextField> _fieldAccessor;

		public void Begin(KInputTextField field) {
			_cachedValue = field.text;
			_fieldAccessor = () => field;
			IsEditing = true;
			field.ActivateInputField();
			Speech.SpeechPipeline.SpeakInterrupt($"Editing, {field.text}");
		}

		public void Begin(System.Func<KInputTextField> accessor) {
			var field = accessor();
			if (field == null) return;
			_cachedValue = field.text;
			_fieldAccessor = accessor;
			IsEditing = true;
			field.gameObject.SetActive(true);
			field.text = _cachedValue;
			field.Select();
			field.ActivateInputField();
			Speech.SpeechPipeline.SpeakInterrupt($"Editing, {_cachedValue}");
		}

		public void Confirm() {
			IsEditing = false;
			var field = _fieldAccessor?.Invoke();
			if (field != null) {
				field.DeactivateInputField();
				Speech.SpeechPipeline.SpeakInterrupt($"Confirmed, {field.text}");
			} else {
				Speech.SpeechPipeline.SpeakInterrupt($"Cancelled, {_cachedValue}");
			}
		}

		public void Cancel() {
			IsEditing = false;
			var field = _fieldAccessor?.Invoke();
			if (field != null) {
				field.text = _cachedValue;
				field.DeactivateInputField();
			}
			Speech.SpeechPipeline.SpeakInterrupt($"Cancelled, {_cachedValue}");
		}
	}
}
