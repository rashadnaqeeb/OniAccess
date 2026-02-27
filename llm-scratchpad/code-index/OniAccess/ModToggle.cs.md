// Mod on/off toggle with full handler-stack integration.
//
// Toggle OFF order: speak "off" -> DeactivateAll -> disable pipeline -> set flag.
// Toggle ON order: set flag -> enable pipeline -> speak "on" -> DetectAndActivate.
// Speech fires BEFORE disable so the confirmation is always heard.
// When off, only Ctrl+Shift+F12 is active (handled directly in KeyPoller).
static class ModToggle (line 16)
  // Whether the mod is currently enabled. Starts true.
  // When false, ModInputRouter passes all keys through and KeyPoller only checks toggle hotkey.
  static bool IsEnabled { get; private set; } (line 22)

  // Toggle the mod on or off. Called by KeyPoller on Ctrl+Shift+F12.
  static void Toggle() (line 27)
