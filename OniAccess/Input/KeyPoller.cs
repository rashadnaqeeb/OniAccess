using OniAccess.Toggle;

namespace OniAccess.Input
{
    /// <summary>
    /// MonoBehaviour that drives the per-frame Tick() on the active handler.
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

        private bool _startupDone;

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

            // One-time: MainMenu.Activate fires before Harmony patches, so our
            // KScreen.Activate postfix misses it. Find it on first frame.
            if (!_startupDone)
            {
                _startupDone = true;
                try
                {
                    var mainMenu = UnityEngine.Object.FindObjectOfType<MainMenu>();
                    if (mainMenu != null)
                        ContextDetector.OnScreenActivated(mainMenu);
                }
                catch (System.Exception ex)
                {
                    Util.Log.Warn($"Startup screen detect: {ex.Message}");
                }
            }

            // Call Tick() on the active handler
            HandlerStack.ActiveHandler?.Tick();
        }

        private static bool IsCtrlShiftHeld()
        {
            return (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftControl)
                    || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightControl))
                && (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
                    || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift));
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
