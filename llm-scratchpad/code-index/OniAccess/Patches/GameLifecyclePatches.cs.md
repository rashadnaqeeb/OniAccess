// Harmony patches for one-time game lifecycle events.
// Screen lifecycle patches are in ScreenLifecyclePatches.cs.
//
// Game.OnDestroy is intentionally NOT patched: it fires on scene transitions (load save),
// not only on app quit. A previous patch called SpeechEngine.Shutdown() there which killed
// speech after loading a save. Tolk cleanup is handled by OS process exit instead.

// Postfix on InputInit.Awake. Registers ModInputRouter in ONI's input handler tree at priority 50.
// Uses AccessTools because InputInit is an internal type.
// Idempotent: checks ModInputRouter.Instance before registering.
// Also warns if the game build watermark differs from the last tested version.
[HarmonyPatch] (uses TargetMethod with AccessTools.TypeByName("InputInit"))
internal static class InputInit_Awake_Patch (line 23)
  private static System.Reflection.MethodBase TargetMethod() (line 24)
  private static void Postfix() (line 29)
