// Harmony patches on HoverTextDrawer that feed the tooltip capture buffer (TooltipCapture).
// All patches guard on skin.drawWidgets (tooltip is actually visible).
//
// Draw pass lifecycle mirrors TooltipCapture's API:
//   HoverTextDrawer:  BeginDrawing -> [BeginShadowBar -> DrawText/DrawIcon/NewLine]* -> EndDrawing
//   TooltipCapture:   BeginFrame   -> [BeginBlock     -> AppendText/AppendIcon/NewLine]* -> EndFrame
//
// Text segments within a line are concatenated directly.
// NewLine creates line boundaries (comma-separated). Blocks are comma-separated.

[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginDrawing))]
internal static class HoverTextDrawer_BeginDrawing_Patch (line 20)
  private static void Postfix(HoverTextDrawer __instance) (line 21)  // -> TooltipCapture.BeginFrame()

[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginShadowBar))]
internal static class HoverTextDrawer_BeginShadowBar_Patch (line 28)
  private static void Postfix(HoverTextDrawer __instance) (line 29)  // -> TooltipCapture.BeginBlock()

[HarmonyPatch(typeof(HoverTextDrawer), "DrawText", new[] { typeof(string), typeof(TextStyleSetting), typeof(Color), typeof(bool) })]
internal static class HoverTextDrawer_DrawText_Patch (line 36)
  private static void Postfix(HoverTextDrawer __instance, string text) (line 38)  // -> TooltipCapture.AppendText(text)

[HarmonyPatch(typeof(HoverTextDrawer), "DrawIcon", new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) })]
internal static class HoverTextDrawer_DrawIcon_Patch (line 44)
  private static void Postfix(HoverTextDrawer __instance, Sprite icon) (line 46)  // -> TooltipCapture.AppendIcon(icon)

[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.NewLine))]
internal static class HoverTextDrawer_NewLine_Patch (line 53)
  private static void Postfix(HoverTextDrawer __instance) (line 55)  // -> TooltipCapture.AppendNewLine()

[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.EndDrawing))]
internal static class HoverTextDrawer_EndDrawing_Patch (line 61)
  private static void Postfix(HoverTextDrawer __instance) (line 63)  // -> TooltipCapture.EndFrame()
