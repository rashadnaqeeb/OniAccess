using UnityEngine;
using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Input
{
    /// <summary>
    /// MonoBehaviour that bridges Unity's input system to the HotkeyRegistry.
    /// Attached to a persistent GameObject created during mod initialization.
    ///
    /// Phase 1 approach: Use Input.GetKeyDown in Update() for simplicity.
    /// This avoids the complexity of hooking into ONI's KInputHandler priority system.
    /// Future phases can migrate to proper KInputHandler integration when context-sensitive
    /// key handling needs to interact with ONI's own input routing.
    ///
    /// Checks Input.GetKeyDown for registered hotkey keys, determines active modifiers,
    /// determines current AccessContext, and calls HotkeyRegistry.TryHandle.
    /// </summary>
    public class InputInterceptor : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance for external access.
        /// </summary>
        public static InputInterceptor Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            // Check F12 key (used for both toggle with Ctrl+Shift and help without modifiers)
            if (Input.GetKeyDown(KeyCode.F12))
            {
                HotkeyModifier modifiers = GetActiveModifiers();
                AccessContext context = DetermineContext();

                if (HotkeyRegistry.TryHandle(KeyCode.F12, modifiers, context))
                {
                    // Handled by registry -- event consumed
                    return;
                }
            }

            // Future phases will add more key checks here as hotkeys are registered.
            // A more dynamic approach (iterating registered keys) can be added when needed.
        }

        /// <summary>
        /// Determine the currently active modifier keys.
        /// </summary>
        private static HotkeyModifier GetActiveModifiers()
        {
            HotkeyModifier modifiers = HotkeyModifier.None;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                modifiers |= HotkeyModifier.Ctrl;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                modifiers |= HotkeyModifier.Shift;

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                modifiers |= HotkeyModifier.Alt;

            return modifiers;
        }

        /// <summary>
        /// Determine the current game context for context-sensitive hotkey matching.
        /// Phase 1 implementation is minimal -- expanded in later phases.
        /// </summary>
        private static AccessContext DetermineContext()
        {
            // Check if game is fully loaded first
            try
            {
                if (Game.Instance == null)
                    return AccessContext.Global;
            }
            catch
            {
                // Game class may not be available (e.g., main menu)
                return AccessContext.Global;
            }

            // Future phases will make this more sophisticated:
            // - Check ManagementMenu.Instance for open screens -> MenuOpen
            // - Check active tools for BuildMode
            // - Default to WorldView when in gameplay

            return AccessContext.WorldView;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
