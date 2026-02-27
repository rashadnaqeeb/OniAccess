# ContextDetector.cs

## File-level comment
Static class that receives screen lifecycle events from Harmony patches and
determines which handler to activate on the HandlerStack.

Uses a type-safe registry mapping KScreen types to handler factories. When a
registered screen activates, creates and pushes a handler. When a screen
deactivates, pops the handler if it matches the top of stack. Unregistered
screens are silently ignored.

Called from `ScreenLifecyclePatches` (KScreen.Activate postfix, KScreen.Deactivate prefix).

---

```
static class ContextDetector (line 15)

  // Private fields
  private static readonly Dictionary<Type, Func<KScreen, IAccessHandler>> _registry (line 20)
  // Maps KScreen types to handler factories. Populated during mod init.

  private static readonly HashSet<Type> _showPatchedTypes (line 29)
  // Screen types whose lifecycle is managed via Show patches rather than
  // KScreen.Activate/Deactivate. Generic Activate/Deactivate patches skip
  // these to prevent zombie handlers from OnPrefabInit calls.

  // Methods
  static bool IsShowPatched(System.Type screenType)       (line 36)
  // Returns true if the type uses Show-patch lifecycle.

  static void Register<TScreen>(Func<KScreen, IAccessHandler> factory) (line 46)
  // Generic overload. Registers factory keyed by TScreen compile-time type.

  static void Register(System.Type screenType, Func<KScreen, IAccessHandler> factory) (line 57)
  // Non-generic overload for runtime-resolved types (AccessTools.TypeByName).
  // Logs a warning and returns if screenType is null.

  static void OnScreenActivated(KScreen screen)           (line 71)
  // Called from Harmony postfix on KScreen.Activate.
  // Looks up screen type in registry; if found, removes stale handlers,
  // guards against duplicate pushes for the same screen instance, then
  // creates and pushes a new handler via factory.
  // Unregistered types are silently ignored.

  static void OnScreenDeactivating(KScreen screen)        (line 105)
  // Called from Harmony prefix/postfix on screen deactivation.
  // Fast path: if handler is on top, calls HandlerStack.Pop.
  // Slow path: searches stack for a buried handler and calls RemoveByScreen.
  // No-ops if no matching handler is found.

  static void RegisterMenuHandlers()                      (line 130)
  // Registers all screen handler factories. Called during Mod.OnLoad.
  // Covers: MainMenu, PauseScreen, ConfirmDialogScreen, OptionsMenuScreen,
  // AudioOptionsScreen, GraphicsOptionsScreen, GameOptionsScreen,
  // InputBindingsScreen, MetricsOptionsScreen, FeedbackScreen, CreditsScreen,
  // RetiredColonyInfoScreen, ModeSelectScreen, ClusterCategorySelectionScreen,
  // ColonyDestinationSelectScreen, WorldGenScreen, MinionSelectScreen,
  // LoadScreen, SaveScreen, FileNameDialog, ModsScreen, LanguageOptionsScreen,
  // InfoDialogScreen, CustomizableDialogScreen, GameOverScreen, VictoryScreen,
  // PatchNotesScreen, LockerMenuScreen, KleiItemDropScreen, WattsonMessage,
  // StoryMessageScreen, VideoScreen, Hud (TileCursorHandler),
  // DetailsScreen, JobsTableScreen, VitalsTableScreen, ConsumablesTableScreen,
  // ResearchScreen, SkillsScreen, ScheduleScreen.
  // Runtime-resolved types (not available at compile time) use
  // AccessTools.TypeByName + the non-generic Register overload.

  static void DetectAndActivate()                         (line 300)
  // Scans the live Unity scene for registered, active KScreens when the mod
  // is toggled ON mid-session. Sorts matches by KScreenManager.screenStack
  // position (layering order), then pushes a BaselineHandler followed by
  // handlers for each matched screen. Falls back to BaselineHandler alone
  // if nothing matches. No-ops if HandlerStack already has entries.
```
