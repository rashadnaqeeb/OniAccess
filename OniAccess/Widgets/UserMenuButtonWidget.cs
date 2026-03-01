namespace OniAccess.Widgets {
	/// <summary>
	/// A ButtonWidget whose click fires a captured System.Action directly,
	/// for user menu buttons that have no KButton/KToggle/MultiToggle component.
	/// </summary>
	public class UserMenuButtonWidget: ButtonWidget {
		public System.Action OnClick { get; set; }
		public System.Func<bool> IsInteractableFunc { get; set; }

		public override bool IsValid() {
			if (IsInteractableFunc != null && !IsInteractableFunc()) return false;
			return true;
		}

		public override bool Activate() {
			if (OnClick == null) return false;
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click_Open"));
			} catch (System.Exception ex) {
				Util.Log.Warn($"UserMenuButtonWidget click sound: {ex.Message}");
			}
			try {
				OnClick();
			} catch (System.Exception ex) {
				Util.Log.Warn($"UserMenuButtonWidget.Activate: {ex.Message}");
			}
			return true;
		}
	}
}
