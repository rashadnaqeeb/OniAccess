// Single IInputHandler registered in ONI's KInputHandler tree at priority 50.
// Intercepts KButtonEvents before PlayerController (20), KScreenManager (10),
// CameraController (1), and DebugHandler (-1).
//
// On KeyDown: walks handler stack top-to-bottom. A handler returning true from HandleKeyDown
// consumes the event. A CapturesAllInput barrier blocks non-passthrough keys.
// When ModToggle is off, all events pass through untouched.
class ModInputRouter : IInputHandler (line 16)
  static ModInputRouter Instance { get; private set; } (line 17)

  string handlerName => "OniAccess" (line 19)
  KInputHandler inputHandler { get; set; } (line 20)

  ModInputRouter() (line 22)

  // Walks handler stack for the KeyDown event. Delegates to HandleKeyDown per handler,
  // then enforces CapturesAllInput barrier if no handler consumed.
  void OnKeyDown(KButtonEvent e) (line 26)

  // Walks handler stack for KeyUp. Only enforces CapturesAllInput and ConsumedKeys barriers;
  // handlers do not have a HandleKeyUp method.
  void OnKeyUp(KButtonEvent e) (line 45)

  // Checks if the event matches any action currently bound to the handler's ConsumedKeys list.
  // Iterates global binding table to find actions mapped to each consumed key+modifier.
  // Rebind-safe: only declared physical keys are consumed regardless of game bindings.
  private static bool IsConsumedKey(KButtonEvent e, IAccessHandler handler) (line 71)

  // Returns true for actions that must always pass through even inside a CapturesAllInput barrier:
  // mouse buttons, zoom, and Escape. Escape must reach game screens so KScreen.Deactivate
  // fires our Harmony patch to pop the handler.
  private static bool IsPassThroughAction(KButtonEvent e) (line 93)
