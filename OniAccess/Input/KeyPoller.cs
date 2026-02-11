using OniAccess.Toggle;

namespace OniAccess.Input
{
    /// <summary>
    /// MonoBehaviour that polls UnityEngine.Input.GetKeyDown for keys that ONI
    /// doesn't generate KButtonEvents for. Per research: plain F12 (no modifiers)
    /// and arrow keys have NO Action binding, so no KButtonEvent is ever created.
    ///
    /// This bridges those unbound keys into the handler system by calling
    /// IAccessHandler.HandleUnboundKey on the active handler.
    ///
    /// Also handles the Ctrl+Shift+F12 toggle key, which must work even when
    /// the mod is off (the only key active in VanillaMode OFF state).
    ///
    /// All UnityEngine.Input references are fully qualified per Phase 1 decision:
    /// bare Input resolves to the OniAccess.Input namespace, not UnityEngine.Input.
    /// </summary>
    public class KeyPoller : UnityEngine.MonoBehaviour
    {
        public static KeyPoller Instance { get; private set; }

        /// <summary>
        /// Keys to poll -- these have no ONI Action binding and generate no KButtonEvent.
        /// </summary>
        private static readonly UnityEngine.KeyCode[] PollKeys = new[]
        {
            UnityEngine.KeyCode.F12,
            UnityEngine.KeyCode.UpArrow,
            UnityEngine.KeyCode.DownArrow,
            UnityEngine.KeyCode.LeftArrow,
            UnityEngine.KeyCode.RightArrow,
            UnityEngine.KeyCode.Home,
            UnityEngine.KeyCode.End,
            UnityEngine.KeyCode.Tab,
            UnityEngine.KeyCode.Return,
        };

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            // Toggle key: Ctrl+Shift+F12 -- ALWAYS check, even when mod is off.
            // This is the only key that works when mod is disabled.
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F12)
                && IsCtrlShiftHeld())
            {
                VanillaMode.Toggle();
                return; // Don't process F12 further this frame
            }

            // When mod is off, don't process anything else
            if (!VanillaMode.IsEnabled) return;

            if (HandlerStack.Count == 0) return;

            // Tick handlers that need per-frame updates (e.g., WorldGenHandler progress)
            var active = HandlerStack.ActiveHandler;
            if (active is ITickable tickable)
                tickable.Tick();

            // Poll each unbound key
            foreach (var key in PollKeys)
            {
                if (UnityEngine.Input.GetKeyDown(key))
                {
                    // Skip F12 if modifiers are held (those ARE bound to game debug actions
                    // and will come through as KButtonEvents via ModInputRouter)
                    if (key == UnityEngine.KeyCode.F12 && AnyModifierHeld()) continue;

                    HandlerStack.DispatchUnboundKey(key);
                }
            }
        }

        private static bool IsCtrlShiftHeld()
        {
            return (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftControl)
                    || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightControl))
                && (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
                    || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift));
        }

        private static bool AnyModifierHeld()
        {
            return UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftControl)
                || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightControl)
                || UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
                || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift)
                || UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt)
                || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightAlt);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
