using HarmonyLib;
using OniAccess.Input;
using OniAccess.Toggle;
using OniAccess.Util;

namespace OniAccess.Patches
{
    /// <summary>
    /// Harmony patches for integrating the mod's input architecture into ONI's systems.
    ///
    /// InputInit_Awake_Patch: Registers ModInputRouter in ONI's KInputHandler tree at
    /// priority 50 (above PlayerController at 20, KScreenManager at 10, CameraController at 1).
    ///
    /// KScreen_Activate_Patch: Fires context detection when screens open.
    /// KScreen_Deactivate_Patch: Fires context detection when screens close (Prefix because
    /// Deactivate calls PopScreen then Destroy).
    /// </summary>

    /// <summary>
    /// Register ModInputRouter in ONI's input handler tree when the input system initializes.
    /// Idempotent: checks if already registered before adding.
    /// Note: InputInit is internal, so we use TargetMethod with AccessTools for type resolution.
    /// </summary>
    [HarmonyPatch]
    internal static class InputInit_Awake_Patch
    {
        private static System.Reflection.MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("InputInit");
            return AccessTools.Method(type, "Awake");
        }

        private static void Postfix()
        {
            // Idempotent: don't register twice
            if (ModInputRouter.Instance != null) return;

            var router = new ModInputRouter();

            // Follow the same pattern as InputInit.Awake uses for KScreenManager/DebugHandler
            if (KInputManager.currentController != null)
            {
                KInputHandler.Add(KInputManager.currentController, router, 50);
            }
            else
            {
                var inputManager = Global.GetInputManager();
                KInputHandler.Add(inputManager.GetDefaultController(), router, 50);
            }

            Log.Info("ModInputRouter registered at priority 50");
        }
    }

    /// <summary>
    /// Detect screen activations for context-aware handler switching.
    /// Postfix: fires after KScreen.Activate completes (screen is now on the stack).
    /// </summary>
    [HarmonyPatch(typeof(KScreen), nameof(KScreen.Activate))]
    internal static class KScreen_Activate_Patch
    {
        private static void Postfix(KScreen __instance)
        {
            if (!VanillaMode.IsEnabled) return;
            ContextDetector.OnScreenActivated(__instance);
        }
    }

    /// <summary>
    /// Detect screen deactivations for context-aware handler switching.
    /// Prefix: fires BEFORE KScreen.Deactivate because Deactivate calls PopScreen then Destroy.
    /// We need the screen instance before it's destroyed.
    /// </summary>
    [HarmonyPatch(typeof(KScreen), nameof(KScreen.Deactivate))]
    internal static class KScreen_Deactivate_Patch
    {
        private static void Prefix(KScreen __instance)
        {
            if (!VanillaMode.IsEnabled) return;
            ContextDetector.OnScreenDeactivating(__instance);
        }
    }
}
