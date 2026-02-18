using HarmonyLib;
using OniAccess.Handlers.Tiles;
using UnityEngine;

namespace OniAccess.Patches {
	/// <summary>
	/// Harmony patches on HoverTextDrawer that feed the tooltip capture buffer.
	/// All patches guard on skin.drawWidgets (hover tooltip is visible).
	///
	/// Draw pass lifecycle:
	///   BeginDrawing -> [BeginShadowBar -> DrawText/DrawIcon/NewLine]* -> EndDrawing
	/// TooltipCapture mirrors this:
	///   BeginFrame -> [BeginBlock -> AppendText/AppendIcon/AppendNewLine]* -> EndFrame
	///
	/// Text segments within a line are concatenated directly. NewLine creates
	/// line boundaries (comma-separated). Blocks are comma-separated.
	/// </summary>

	[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginDrawing))]
	internal static class HoverTextDrawer_BeginDrawing_Patch {
		private static void Postfix(HoverTextDrawer __instance) {
			if (!__instance.skin.drawWidgets) return;
			TooltipCapture.BeginFrame();
		}
	}

	[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginShadowBar))]
	internal static class HoverTextDrawer_BeginShadowBar_Patch {
		private static void Postfix(HoverTextDrawer __instance) {
			if (!__instance.skin.drawWidgets) return;
			TooltipCapture.BeginBlock();
		}
	}

	[HarmonyPatch(typeof(HoverTextDrawer), "DrawText",
		new[] { typeof(string), typeof(TextStyleSetting), typeof(Color), typeof(bool) })]
	internal static class HoverTextDrawer_DrawText_Patch {
		private static void Postfix(HoverTextDrawer __instance, string text) {
			if (!__instance.skin.drawWidgets) return;
			TooltipCapture.AppendText(text);
		}
	}

	[HarmonyPatch(typeof(HoverTextDrawer), "DrawIcon",
		new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) })]
	internal static class HoverTextDrawer_DrawIcon_Patch {
		private static void Postfix(HoverTextDrawer __instance, Sprite icon) {
			if (!__instance.skin.drawWidgets) return;
			TooltipCapture.AppendIcon(icon);
		}
	}

	[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.NewLine))]
	internal static class HoverTextDrawer_NewLine_Patch {
		private static void Postfix(HoverTextDrawer __instance) {
			if (!__instance.skin.drawWidgets) return;
			TooltipCapture.AppendNewLine();
		}
	}

	[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.EndDrawing))]
	internal static class HoverTextDrawer_EndDrawing_Patch {
		private static void Postfix(HoverTextDrawer __instance) {
			if (!__instance.skin.drawWidgets) return;
			TooltipCapture.EndFrame();
		}
	}
}
