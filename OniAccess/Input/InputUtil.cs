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

		private static readonly float[] WholeSteps = { 1f, 10f, 100f, 1000f };
		private static readonly float[] FractionalSteps = { 0.01f, 0.1f, 0.25f, 0.5f };

		/// <summary>
		/// Whole-number slider step for the given level (1, 10, 100, 1000).
		/// </summary>
		public static float StepForLevel(int level) {
			return WholeSteps[UnityEngine.Mathf.Clamp(level, 0, WholeSteps.Length - 1)];
		}

		/// <summary>
		/// Fractional slider step as a proportion of the range (1%, 10%, 25%, 50%).
		/// </summary>
		public static float FractionForLevel(int level) {
			return FractionalSteps[UnityEngine.Mathf.Clamp(level, 0, FractionalSteps.Length - 1)];
		}

		/// <summary>
		/// Returns a step level based on modifier keys held:
		/// 0 = plain, 1 = Shift, 2 = Ctrl, 3 = Ctrl+Shift.
		/// </summary>
		public static int GetStepLevel() {
			bool ctrl = CtrlHeld();
			bool shift = ShiftHeld();
			if (ctrl && shift) return 3;
			if (ctrl) return 2;
			if (shift) return 1;
			return 0;
		}
	}
}
