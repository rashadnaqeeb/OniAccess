namespace OniAccess.Input {
	/// <summary>
	/// Shared modifier-key helpers used by handlers that detect their own keys
	/// via UnityEngine.Input.GetKeyDown in Tick().
	/// </summary>
	public static class InputUtil {
		public static bool AnyModifierHeld() {
			return UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftControl)
				|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightControl)
				|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
				|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift)
				|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt)
				|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightAlt);
		}

		public static bool ShiftHeld() {
			return UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
				|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift);
		}

		public static bool CtrlHeld() {
			return UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftControl)
				|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightControl);
		}

		public static bool AltHeld() {
			return UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt)
				|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightAlt);
		}
	}
}
