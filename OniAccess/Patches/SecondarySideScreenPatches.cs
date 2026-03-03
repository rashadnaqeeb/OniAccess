using HarmonyLib;

using OniAccess.Handlers;
using OniAccess.Handlers.Screens;

namespace OniAccess.Patches {
	[HarmonyPatch(typeof(DetailsScreen), nameof(DetailsScreen.SetSecondarySideScreen))]
	internal static class DetailsScreen_SetSecondarySideScreen_Patch {
		private static void Postfix(KScreen __result) {
			if (!ModToggle.IsEnabled) return;
			if (__result is SelectedRecipeQueueScreen recipeScreen) {
				HandlerStack.Push(new RecipeQueueHandler(recipeScreen));
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
			if (HandlerStack.ActiveHandler is RecipeQueueHandler)
				HandlerStack.Pop();
		}
	}
}
