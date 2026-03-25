namespace OniAccess.Input {
	/// <summary>
	/// Shared modifier-key helpers used by handlers that detect their own keys
	/// via UnityEngine.Input.GetKeyDown in Tick().
	/// On macOS, Ctrl maps to Option and Alt maps to Cmd to avoid OS-level
	/// conflicts (Mission Control, Spaces, special character input, VoiceOver).
	/// </summary>
	public static class InputUtil {
		public static readonly bool IsMac =
			UnityEngine.SystemInfo.operatingSystemFamily == UnityEngine.OperatingSystemFamily.MacOSX;

		public static bool AnyModifierHeld() => CtrlHeld() || ShiftHeld() || AltHeld();

		public static bool ShiftHeld() {
			return UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
				|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift);
		}

		public static bool CtrlHeld() {
			return IsMac
				? UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt)
					|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightAlt)
				: UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftControl)
					|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightControl);
		}

		public static bool AltHeld() {
			return IsMac
				? UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftCommand)
					|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightCommand)
				: UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt)
					|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightAlt);
		}

		/// <summary>
		/// Returns 0-9 if an Alpha or Keypad digit key was pressed this frame, -1 otherwise.
		/// </summary>
		public static int GetDigitKeyDown() {
			for (int i = 0; i <= 9; i++) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha0 + i)
					|| UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Keypad0 + i))
					return i;
			}
			return -1;
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
