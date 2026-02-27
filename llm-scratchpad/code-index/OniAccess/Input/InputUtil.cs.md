// Shared modifier-key helpers for handlers that detect keys via UnityEngine.Input.GetKeyDown in Tick().
// All UnityEngine.Input calls are fully qualified because bare "Input" resolves to this namespace.
static class InputUtil (line 6)
  static bool AnyModifierHeld() (line 7)    // true if any Ctrl, Shift, or Alt is held
  static bool ShiftHeld() (line 16)
  static bool CtrlHeld() (line 21)
  static bool AltHeld() (line 26)

  private static readonly float[] WholeSteps (line 31)       // { 1, 10, 100, 1000 }
  private static readonly float[] FractionalSteps (line 32)  // { 0.01, 0.1, 0.25, 0.5 }

  // Whole-number slider step for the given modifier level (0-3 -> 1, 10, 100, 1000).
  static float StepForLevel(int level) (line 37)

  // Fractional slider step as a proportion of range for the given level (0-3 -> 1%, 10%, 25%, 50%).
  static float FractionForLevel(int level) (line 44)

  // Returns a step level (0-3) based on held modifiers: 0=plain, 1=Shift, 2=Ctrl, 3=Ctrl+Shift.
  static int GetStepLevel() (line 52)
