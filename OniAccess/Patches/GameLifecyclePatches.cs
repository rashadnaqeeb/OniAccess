using HarmonyLib;
using OniAccess.Input;
using OniAccess.Util;

namespace OniAccess.Patches {
	/// <summary>
	/// Harmony patches for one-time game lifecycle events.
	/// Screen lifecycle patches (activate, deactivate, show/hide) are in
	/// ScreenLifecyclePatches.cs.
	///
	/// Game.OnDestroy is intentionally NOT patched. It fires during scene
	/// transitions (loading a save), not just on application quit. A previous
	/// patch called SpeechEngine.Shutdown() here, which killed all speech
	/// after loading a save. Tolk cleanup is handled by OS process exit.
	/// </summary>

	/// <summary>
	/// Register ModInputRouter in ONI's input handler tree when the input system initializes.
	/// Idempotent: checks if already registered before adding.
	/// Note: InputInit is internal, so we use TargetMethod with AccessTools for type resolution.
	/// </summary>
	[HarmonyPatch]
	internal static class InputInit_Awake_Patch {
		private static System.Reflection.MethodBase TargetMethod() {
			var type = AccessTools.TypeByName("InputInit");
			return AccessTools.Method(type, "Awake");
		}

		private static void Postfix() {
			// Idempotent: don't register twice
			if (ModInputRouter.Instance != null) return;

			try {
				var router = new ModInputRouter();

				// Follow the same pattern as InputInit.Awake uses for KScreenManager/DebugHandler
				if (KInputManager.currentController != null) {
					KInputHandler.Add(KInputManager.currentController, router, 50);
				} else {
					var inputManager = Global.GetInputManager();
					KInputHandler.Add(inputManager.GetDefaultController(), router, 50);
				}

				Log.Info("ModInputRouter registered at priority 50");

				var buildText = BuildWatermark.GetBuildText();
				if (!buildText.StartsWith("U57-707956"))
					Log.Warn($"Game build '{buildText}' is newer than last tested 'U57-707956'. Field names may have changed.");
			} catch (System.Exception ex) {
				Log.Error($"Failed to register ModInputRouter: {ex}");
			}
		}
	}
}
