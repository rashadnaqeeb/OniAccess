namespace OniAccess.Input {
	/// <summary>
	/// Static class that receives screen lifecycle events from Harmony patches and
	/// determines which handler to activate on the HandlerStack.
	///
	/// Uses a type-safe registry mapping KScreen types to handler factories.
	/// When a registered screen activates, creates and pushes a handler.
	/// When a screen deactivates, pops the handler if it matches the top of stack.
	/// Unregistered screens are silently ignored (structural UI, not interactive menus).
	///
	/// Called from InputArchPatches (KScreen.Activate postfix, KScreen.Deactivate prefix).
	/// </summary>
	public static class ContextDetector {
		/// <summary>
		/// Registry mapping KScreen types to handler factory functions.
		/// Populated during mod initialization by concrete handler registration.
		/// </summary>
		private static readonly System.Collections.Generic.Dictionary<System.Type, System.Func<KScreen, IAccessHandler>> _registry
			= new System.Collections.Generic.Dictionary<System.Type, System.Func<KScreen, IAccessHandler>>();

		/// <summary>
		/// Screen types whose lifecycle is managed via Show patches instead of
		/// KScreen.Activate/Deactivate. These screens call Show(false) inside OnActivate
		/// during prefab init, so the generic Activate/Deactivate patches must skip them
		/// to avoid pushing zombie handlers on startup.
		/// </summary>
		private static readonly System.Collections.Generic.HashSet<System.Type> _showPatchedTypes
			= new System.Collections.Generic.HashSet<System.Type>();

		/// <summary>
		/// Returns true if the given type uses Show-patch lifecycle and should be
		/// skipped by generic KScreen.Activate/Deactivate patches.
		/// </summary>
		public static bool IsShowPatched(System.Type screenType) {
			return _showPatchedTypes.Contains(screenType);
		}

		/// <summary>
		/// Register a screen type to handler factory mapping.
		/// Generic overload for compile-time type safety.
		/// </summary>
		/// <typeparam name="TScreen">The KScreen subclass to register.</typeparam>
		/// <param name="factory">Factory function that creates a handler for the screen.</param>
		public static void Register<TScreen>(System.Func<KScreen, IAccessHandler> factory) where TScreen : KScreen {
			_registry[typeof(TScreen)] = factory;
			Util.Log.Debug($"ContextDetector.Register: {typeof(TScreen).Name}");
		}

		/// <summary>
		/// Register a screen type to handler factory mapping.
		/// Non-generic overload for runtime-resolved types (e.g., AccessTools.TypeByName).
		/// </summary>
		/// <param name="screenType">The screen type to register.</param>
		/// <param name="factory">Factory function that creates a handler for the screen.</param>
		public static void Register(System.Type screenType, System.Func<KScreen, IAccessHandler> factory) {
			if (screenType == null) {
				Util.Log.Warn("ContextDetector.Register called with null screenType");
				return;
			}
			_registry[screenType] = factory;
			Util.Log.Debug($"ContextDetector.Register: {screenType.Name}");
		}

		/// <summary>
		/// Called from Harmony postfix on KScreen.Activate.
		/// Looks up the screen type in the registry. If found, creates and pushes a handler.
		/// Unregistered screens are silently ignored (structural UI like FrontEndBackground).
		/// </summary>
		public static void OnScreenActivated(KScreen screen) {
			if (screen == null) return;

			var screenType = screen.GetType();
			if (!_registry.TryGetValue(screenType, out var factory)) {
				Util.Log.Debug($"Screen activated (no handler): {screenType.Name}");
				return;
			}

			// Guard: don't push a duplicate handler for the same screen instance
			var active = HandlerStack.ActiveHandler;
			if (active is ScreenHandler sh && sh.Screen == screen) {
				Util.Log.Debug($"Screen activated (already handled): {screenType.Name}");
				return;
			}

			var handler = factory(screen);
			HandlerStack.Push(handler);
			Util.Log.Debug($"Screen activated: {screenType.Name} -> pushed handler");
		}

		/// <summary>
		/// Called from Harmony prefix on KScreen.Deactivate.
		/// Pops the handler only if the active handler is a ScreenHandler whose Screen
		/// property matches the deactivating screen. This prevents popping the wrong handler
		/// when structural screens deactivate.
		/// </summary>
		public static void OnScreenDeactivating(KScreen screen) {
			if (screen == null) return;

			var active = HandlerStack.ActiveHandler;

			// Match ScreenHandler subclasses (BaseMenuHandler, etc.) by Screen property
			if (active is ScreenHandler screenHandler && screenHandler.Screen == screen) {
				HandlerStack.Pop();
				Util.Log.Debug($"Screen deactivating: {screen.GetType().Name} -> popped handler");
				return;
			}

			// Match non-ScreenHandler handlers that track their screen (e.g., WorldGenHandler)
			if (active is Handlers.WorldGenHandler worldGenHandler && worldGenHandler.Screen == screen) {
				HandlerStack.Pop();
				Util.Log.Debug($"Screen deactivating: {screen.GetType().Name} -> popped WorldGenHandler");
				return;
			}

			Util.Log.Debug($"Screen deactivating (no matching handler): {screen.GetType().Name}");
		}

		/// <summary>
		/// Register all menu screen handlers for Phase 3 basic screens.
		/// Called during mod initialization (Mod.OnLoad).
		/// Future plans (03-03, 03-04) will add more registrations.
		/// </summary>
		public static void RegisterMenuHandlers() {
			// MainMenu (direct KScreen subclass, NOT KButtonMenu)
			Register<MainMenu>(screen => new Handlers.MainMenuHandler(screen));

			// PauseScreen (KModalButtonMenu)
			Register<PauseScreen>(screen => new Handlers.PauseMenuHandler(screen));

			// ConfirmDialogScreen (KModalScreen)
			Register<ConfirmDialogScreen>(screen => new Handlers.ConfirmDialogHandler(screen));

			// OptionsMenuScreen (KModalButtonMenu -- top-level options menu)
			Register<OptionsMenuScreen>(screen => new Handlers.OptionsMenuHandler(screen));

			// Options sub-screens may not have compile-time types available.
			// Use AccessTools.TypeByName for runtime resolution and the non-generic Register overload.
			var audioType = HarmonyLib.AccessTools.TypeByName("AudioOptionsScreen");
			Register(audioType, screen => new Handlers.OptionsMenuHandler(screen));

			var graphicsType = HarmonyLib.AccessTools.TypeByName("GraphicsOptionsScreen");
			Register(graphicsType, screen => new Handlers.OptionsMenuHandler(screen));

			var gameOptionsType = HarmonyLib.AccessTools.TypeByName("GameOptionsScreen");
			Register(gameOptionsType, screen => new Handlers.OptionsMenuHandler(screen));

			var inputBindingsType = HarmonyLib.AccessTools.TypeByName("InputBindingsScreen");
			Register(inputBindingsType, screen => new Handlers.KeyBindingsHandler(screen));

			var metricsType = HarmonyLib.AccessTools.TypeByName("MetricsOptionsScreen");
			Register(metricsType, screen => new Handlers.OptionsMenuHandler(screen));

			var feedbackType = HarmonyLib.AccessTools.TypeByName("FeedbackScreen");
			Register(feedbackType, screen => new Handlers.OptionsMenuHandler(screen));

			var creditsType = HarmonyLib.AccessTools.TypeByName("CreditsScreen");
			Register(creditsType, screen => new Handlers.OptionsMenuHandler(screen));

			// RetiredColonyInfoScreen (KModalScreen -- colony summary, MENU-09)
			var retiredColonyType = HarmonyLib.AccessTools.TypeByName("RetiredColonyInfoScreen");
			Register(retiredColonyType, screen => new Handlers.ColonySummaryHandler(screen));

			// ModeSelectScreen (Survival vs No Sweat -- first screen after New Game)
			var modeSelectType = HarmonyLib.AccessTools.TypeByName("ModeSelectScreen");
			Register(modeSelectType, screen => new Handlers.ColonySetupHandler(screen));

			// ClusterCategorySelectionScreen (game mode select -- Survival/No Sweat/Custom)
			var clusterCategoryType = HarmonyLib.AccessTools.TypeByName("ClusterCategorySelectionScreen");
			Register(clusterCategoryType, screen => new Handlers.ColonySetupHandler(screen));

			// ColonyDestinationSelectScreen (asteroid selection, settings, seed)
			Register<ColonyDestinationSelectScreen>(screen => new Handlers.ColonySetupHandler(screen));

			// WorldGenScreen (world generation progress -- no widgets, just progress polling)
			var worldGenType = HarmonyLib.AccessTools.TypeByName("WorldGenScreen");
			Register(worldGenType, screen => new Handlers.WorldGenHandler(screen));

			// MinionSelectScreen (CharacterSelectionController -> NewGameFlowScreen)
			// Handles both initial colony start and Printing Pod duplicant selection
			Register<MinionSelectScreen>(screen => new Handlers.DuplicantSelectHandler(screen));

			// LoadScreen (KModalScreen -- save/load with two-level colony/save navigation)
			Register<LoadScreen>(screen => new Handlers.SaveLoadHandler(screen));

			// ModsScreen (KModalScreen -- mod management from main menu)
			Register<ModsScreen>(screen => new Handlers.ModsHandler(screen));

			// LanguageOptionsScreen (KModalScreen -- language/translation selection from options)
			var langOptionsType = HarmonyLib.AccessTools.TypeByName("LanguageOptionsScreen");
			Register(langOptionsType, screen => new Handlers.TranslationHandler(screen));

			// InfoDialogScreen (KModalScreen -- used by DLC toggle, mod warnings, etc.)
			var infoDialogType = HarmonyLib.AccessTools.TypeByName("InfoDialogScreen");
			Register(infoDialogType, screen => new Handlers.ConfirmDialogHandler(screen));

			// LockerMenuScreen (KModalScreen -- Supply Closet hub from main menu)
			// Show patch pushes/pops via ContextDetector since OnActivate calls Show(false)
			Register<LockerMenuScreen>(screen => new Handlers.LockerMenuHandler(screen));
			_showPatchedTypes.Add(typeof(LockerMenuScreen));

			// KleiItemDropScreen (KModalScreen -- cosmetic item claim/reveal)
			// Show patch pushes/pops via ContextDetector since OnActivate calls Show(false)
			Register<KleiItemDropScreen>(screen => new Handlers.KleiItemDropHandler(screen));
			_showPatchedTypes.Add(typeof(KleiItemDropScreen));

			Util.Log.Debug("ContextDetector.RegisterMenuHandlers: Phase 3 handlers registered");
		}

		/// <summary>
		/// Detect current game state and activate the appropriate handler.
		/// Called when mod is toggled ON to determine what handler should be active.
		///
		/// Checks KScreenManager screen stack for any open registered screens.
		/// If found, creates and pushes the handler. Otherwise, pushes WorldHandler as baseline.
		/// screenStack is private, so we use Harmony Traverse to access it.
		/// </summary>
		public static void DetectAndActivate() {
			if (HandlerStack.Count > 0) return;
			Util.Log.Debug("ContextDetector.DetectAndActivate called");

			// Check KScreenManager for open registered screens
			// screenStack is a private field -- access via Harmony Traverse
			if (KScreenManager.Instance != null) {
				try {
					var screenStack = HarmonyLib.Traverse.Create(KScreenManager.Instance)
						.Field<System.Collections.Generic.List<KScreen>>("screenStack").Value;

					if (screenStack != null) {
						// Walk from top of screen stack looking for registered screens
						for (int i = screenStack.Count - 1; i >= 0; i--) {
							var entry = screenStack[i];
							if (entry == null) continue;

							var screenType = entry.GetType();
							if (_registry.TryGetValue(screenType, out var factory)) {
								// Found a registered screen -- push WorldHandler first as baseline,
								// then push the screen handler on top
								HandlerStack.Push(new WorldHandler());
								var handler = factory(entry);
								HandlerStack.Push(handler);
								Util.Log.Debug($"DetectAndActivate: found {screenType.Name}, pushed handler");
								return;
							}
						}
					}
				} catch (System.Exception ex) {
					Util.Log.Warn($"DetectAndActivate: failed to read screenStack: {ex.Message}");
				}
			}

			// Baseline: WorldHandler so input handling works after toggle cycle
			HandlerStack.Push(new WorldHandler());
		}
	}
}
