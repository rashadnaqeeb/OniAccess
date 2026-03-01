using HarmonyLib;
using OniAccess.Handlers;
using OniAccess.Handlers.Build;

namespace OniAccess.Patches {
	[HarmonyPatch(typeof(PlanScreen), nameof(PlanScreen.OpenCategoryByName))]
	internal static class PlanScreen_OpenCategoryByName_Patch {
		private static bool Prefix(string category) {
			if (!ModToggle.IsEnabled) return true;
			if (BuildMenuData._selectBuildingInProgress) return true;
			HandlerStack.Push(new ActionMenuHandler((HashedString)category));
			return false;
		}
	}
}
