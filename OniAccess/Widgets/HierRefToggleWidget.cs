namespace OniAccess.Widgets {
	/// <summary>
	/// A toggle widget backed by a KButton + HierarchyReferences CheckMark pattern.
	/// Used by AudioOptionsScreen and GameOptionsScreen where on/off state is
	/// determined by a child "CheckMark"/"Checkmark" GameObject's visibility.
	/// </summary>
	public class HierRefToggleWidget: ToggleWidget {
		public HierarchyReferences HierRef { get; set; }

		public override bool IsValid() {
			if (GameObject != null && !GameObject.activeInHierarchy) return false;
			var kb = Component as KButton;
			if (kb != null) return kb.isInteractable;
			return Component != null || GameObject != null;
		}

		public override bool Activate() {
			var kb = Component as KButton;
			if (kb != null) {
				WidgetOps.ClickButton(kb);
				return true;
			}
			return false;
		}

		public override string GetSpeechText() {
			if (SpeechFunc != null) {
				string result = SpeechFunc()?.Trim();
				if (!string.IsNullOrEmpty(result)) return result;
			}

			if (HierRef != null) {
				string checkRef = HierRef.HasReference("CheckMark") ? "CheckMark" : "Checkmark";
				var ckGo = HierRef.GetReference(checkRef);
				if (ckGo != null) {
					bool on = ckGo.gameObject.activeSelf;
					string state = on
						? (string)STRINGS.ONIACCESS.STATES.ON
						: (string)STRINGS.ONIACCESS.STATES.OFF;
					return $"{Label}, {state}";
				}
			}
			return Label;
		}
	}
}
