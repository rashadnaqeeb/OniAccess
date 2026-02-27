// Harmony patches for KScreen lifecycle events (activate, deactivate, show/hide).
// These feed screen transitions into ContextDetector for handler push/pop.
//
// Each screen type requires a different patch point depending on how it manages its lifecycle:
// - Standard screens: KScreen.Activate (Postfix) / KScreen.Deactivate (Prefix)
// - ShowOptimizedKScreen subclasses (TableScreen): TableScreen.OnShow
// - KModalScreen subclasses (ResearchScreen): ResearchScreen.Show (NOT OnShow -- OnActivate
//   calls OnShow(true) during prefab init)
// - Screens whose OnActivate calls Show(false) during init (LockerMenuScreen, KleiItemDropScreen,
//   RetiredColonyInfoScreen): patch Show(bool)
// - Screens that override OnShow (PauseScreen, VideoScreen, SkillsScreen, ScheduleScreen):
//   patch OnShow directly
// - DetailsScreen: OnShow for push; OnCmpDisable for pop (catches both Show(false) and SetActive(false))
// - MinionSelectScreen: OnSpawn (does not call base.OnSpawn so KScreen.Activate never fires)

// Postfix on KScreen.Activate. Skips screens managed via Show patches (IsShowPatched guard)
// to avoid pushing a zombie handler during prefab init.
[HarmonyPatch(typeof(KScreen), nameof(KScreen.Activate))]
internal static class KScreen_Activate_Patch (line 21)
  private static void Postfix(KScreen __instance) (line 22)

// Prefix on KScreen.Deactivate. Fires BEFORE Deactivate because it calls PopScreen then Destroy;
// we need the screen instance before it's gone. Skips Show-patched screens.
[HarmonyPatch(typeof(KScreen), nameof(KScreen.Deactivate))]
internal static class KScreen_Deactivate_Patch (line 37)
  private static void Prefix(KScreen __instance) (line 38)

// Prefix on KModalButtonMenu.Unhide. Guards against NullReferenceException when a child screen's
// Close event fires after the parent is already destroyed (panelRoot is null).
[HarmonyPatch(typeof(KModalButtonMenu), "Unhide")]
internal static class KModalButtonMenu_Unhide_Patch (line 51)
  private static bool Prefix(KModalButtonMenu __instance) (line 53)

// Postfix on LockerMenuScreen.Show(bool). LockerMenuScreen.OnActivate calls Show(false) during
// prefab init, so Activate/Deactivate hooks don't fire for user-visible show/hide.
[HarmonyPatch(typeof(LockerMenuScreen), nameof(LockerMenuScreen.Show))]
internal static class LockerMenuScreen_Show_Patch (line 73)
  private static void Postfix(LockerMenuScreen __instance, bool show) (line 74)

// Postfix on KleiItemDropScreen.Show(bool). Same init pattern as LockerMenuScreen.
[HarmonyPatch(typeof(KleiItemDropScreen), nameof(KleiItemDropScreen.Show))]
internal static class KleiItemDropScreen_Show_Patch (line 88)
  private static void Postfix(KleiItemDropScreen __instance, bool show) (line 89)

// Postfix on PauseScreen.OnShow. PauseScreen overrides OnShow (not Show).
[HarmonyPatch(typeof(PauseScreen), "OnShow")]
internal static class PauseScreen_Show_Patch (line 105)
  private static void Postfix(PauseScreen __instance, bool show) (line 106)

// Postfix on VideoScreen.OnShow. VideoScreen overrides OnShow (not Show).
[HarmonyPatch(typeof(VideoScreen), "OnShow")]
internal static class VideoScreen_OnShow_Patch (line 121)
  private static void Postfix(VideoScreen __instance, bool show) (line 122)

// Postfix on TableScreen.OnShow. TableScreen subclasses (JobsTableScreen, ConsumablesTableScreen)
// extend ShowOptimizedKScreen, which hides via canvas alpha without calling Deactivate.
// Show is declared on ShowOptimizedKScreen; patching TableScreen.OnShow catches all subclasses.
[HarmonyPatch(typeof(TableScreen), "OnShow")]
internal static class TableScreen_OnShow_Patch (line 140)
  private static void Postfix(TableScreen __instance, bool show) (line 141)

// Postfix on DetailsScreen.OnShow. Only pushes handler on show=true.
// DetailsScreen.OnPrefabInit calls Show(false) during init so Activate/Deactivate don't fire.
[HarmonyPatch(typeof(DetailsScreen), "OnShow")]
internal static class DetailsScreen_OnShow_Patch (line 158)
  private static void Postfix(DetailsScreen __instance, bool show) (line 159)

// Postfix on DetailsScreen.OnCmpDisable. Handles pop for both Show(false) and SetActive(false).
// RootMenu.CloseSubMenus() hides DetailsScreen via SetActive(false), bypassing Show(false),
// so the OnShow patch alone would miss it. OnCmpDisable catches both hiding paths.
[HarmonyPatch(typeof(DetailsScreen), "OnCmpDisable")]
internal static class DetailsScreen_OnCmpDisable_Patch (line 175)
  private static void Postfix(DetailsScreen __instance) (line 176)

// Postfix on MinionSelectScreen.OnSpawn. MinionSelectScreen does not call base.OnSpawn(),
// so KScreen.Activate() is never called and KScreen_Activate_Patch never fires.
[HarmonyPatch(typeof(MinionSelectScreen), "OnSpawn")]
internal static class MinionSelectScreen_OnSpawn_Patch (line 188)
  private static void Postfix(MinionSelectScreen __instance) (line 189)

// Postfix on ResearchScreen.Show(bool). Extends KModalScreen whose OnActivate calls OnShow(true)
// during prefab init -- patching Show avoids firing on that init path.
[HarmonyPatch(typeof(ResearchScreen), nameof(ResearchScreen.Show))]
internal static class ResearchScreen_Show_Patch (line 203)
  private static void Postfix(ResearchScreen __instance, bool show) (line 204)

// Postfix on SkillsScreen.OnShow. SkillsScreen does not override Show, only OnShow.
[HarmonyPatch(typeof(SkillsScreen), "OnShow")]
internal static class SkillsScreen_OnShow_Patch (line 219)
  private static void Postfix(SkillsScreen __instance, bool show) (line 220)

// Postfix on ScheduleScreen.OnShow. Same pattern as SkillsScreen.
[HarmonyPatch(typeof(ScheduleScreen), "OnShow")]
internal static class ScheduleScreen_OnShow_Patch (line 234)
  private static void Postfix(ScheduleScreen __instance, bool show) (line 235)

// Postfix on RetiredColonyInfoScreen.Show(bool). Reuses its instance via Show(true) on
// subsequent opens, so KScreen.Activate never fires again. Duplicate guard in
// OnScreenActivated prevents double-push when both Activate and Show(true) fire on first open.
// Note: __instance typed as KScreen (base) because the patch parameter matches the base type.
[HarmonyPatch(typeof(RetiredColonyInfoScreen), nameof(RetiredColonyInfoScreen.Show))]
internal static class RetiredColonyInfoScreen_Show_Patch (line 251)
  private static void Postfix(KScreen __instance, bool show) (line 252)
