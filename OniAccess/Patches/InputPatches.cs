using HarmonyLib;
using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Patches
{
    /// <summary>
    /// Harmony patches for input and lifecycle events.
    ///
    /// Phase 1: With the MonoBehaviour approach for input (InputInterceptor),
    /// we don't need input Harmony patches. This file contains the lifecycle
    /// patch to ensure Tolk is properly unloaded when the game shuts down.
    ///
    /// Future phases may add input-related patches here if the MonoBehaviour
    /// approach proves insufficient for complex input routing.
    /// </summary>

    /// <summary>
    /// Ensures SpeechEngine.Shutdown is called when the game is destroyed,
    /// properly unloading Tolk and releasing native resources.
    /// </summary>
    [HarmonyPatch(typeof(Game), "OnDestroy")]
    internal static class Game_OnDestroy_Patch
    {
        private static void Postfix()
        {
            Log.Info("Game shutting down, cleaning up speech engine");
            SpeechEngine.Shutdown();
        }
    }
}
