using HarmonyLib;

using OniAccess.Handlers;
using OniAccess.Handlers.Screens;

namespace OniAccess.Patches {
	/// <summary>
	/// Manages the handler stack for secondary side screens (e.g.,
	/// SelectedRecipeQueueScreen opened from ComplexFabricatorSideScreen).
	///
	/// SetSecondarySideScreen postfix: pushes a handler for the new screen,
	/// or replaces the existing one when cycling between recipes.
	///
	/// ClearSecondarySideScreen prefix: pops the handler when the game
	/// closes the secondary screen. Suppressed during recipe cycling
	/// (the postfix handles replacement) and during DetailsScreen teardown
	/// (the normal deactivation path handles cleanup).
	/// </summary>
	internal static class SecondarySideScreenPatches {
		internal static bool SuppressClearPop;
	}

	[HarmonyPatch(typeof(DetailsScreen), nameof(DetailsScreen.SetSecondarySideScreen))]
	internal static class DetailsScreen_SetSecondarySideScreen_Patch {
		private static void Postfix(KScreen __result) {
			if (!ModToggle.IsEnabled) return;
			if (__result == null) {
				Util.Log.Warn("SetSecondarySideScreen: __result is null");
			} else if (__result is SelectedRecipeQueueScreen recipeScreen) {
				if (HandlerStack.ActiveHandler is RecipeQueueHandler) {
					HandlerStack.Replace(new RecipeQueueHandler(recipeScreen));
				} else {
					HandlerStack.Push(new RecipeQueueHandler(recipeScreen));
				}
			} else if (__result is OwnablesSecondSideScreen ownablesScreen) {
				HandlerStack.Push(new OwnablesSecondHandler(ownablesScreen));
			} else {
				Util.Log.Debug(
					$"SetSecondarySideScreen: unhandled type {__result.GetType().Name}");
			}
		}
	}

	[HarmonyPatch(typeof(DetailsScreen), nameof(DetailsScreen.ClearSecondarySideScreen))]
	internal static class DetailsScreen_ClearSecondarySideScreen_Patch {
		private static void Prefix() {
			if (!ModToggle.IsEnabled) return;
			if (SecondarySideScreenPatches.SuppressClearPop) return;
			if (!(HandlerStack.ActiveHandler is BaseScreenHandler)) return;
			var ds = DetailsScreen.Instance;
			if (ds == null || !ds.gameObject.activeInHierarchy) return;
			if (HandlerStack.ActiveHandler is RecipeQueueHandler) {
				Util.Log.Debug("ClearSecondarySideScreen: popping RecipeQueueHandler");
				HandlerStack.Pop();
			} else if (HandlerStack.ActiveHandler is OwnablesSecondHandler) {
				Util.Log.Debug("ClearSecondarySideScreen: popping OwnablesSecondHandler");
				HandlerStack.Pop();
			}
		}
	}
}
