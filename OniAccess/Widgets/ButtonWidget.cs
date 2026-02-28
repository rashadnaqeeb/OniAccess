namespace OniAccess.Widgets {
	/// <summary>
	/// A clickable button widget (KButton or MultiToggle used as button).
	/// </summary>
	public class ButtonWidget: Widget {
		public override bool IsValid() {
			if (GameObject != null && !GameObject.activeInHierarchy) return false;
			var btn = Component as KButton;
			if (btn != null) return btn.isInteractable;
			var toggle = Component as KToggle;
			if (toggle != null) return toggle.IsInteractable();
			if (Component is MultiToggle) return true;
			return Component != null || GameObject != null;
		}

		public override bool Activate() {
			var kbutton = Component as KButton;
			if (kbutton != null) {
				WidgetOps.ClickButton(kbutton);
				return true;
			}
			var toggle = Component as KToggle;
			if (toggle != null) {
				toggle.Click();
				return true;
			}
			var mt = Component as MultiToggle;
			if (mt != null) {
				WidgetOps.ClickMultiToggle(mt);
				return true;
			}
			return false;
		}
	}
}
