namespace OniAccess.Input
{
    /// <summary>
    /// Static class that receives screen lifecycle events from Harmony patches and
    /// determines which handler to activate on the HandlerStack.
    ///
    /// Phase 2 implements a minimal skeleton -- logging screen activations/deactivations
    /// for debugging. Phase 3 will add specific screen-to-handler mappings.
    ///
    /// Called from InputArchPatches (KScreen.Activate postfix, KScreen.Deactivate prefix).
    /// </summary>
    public static class ContextDetector
    {
        /// <summary>
        /// Called from Harmony postfix on KScreen.Activate.
        /// Phase 2: log the activation for debugging.
        /// Phase 3 will map specific screens to handlers and push/replace on HandlerStack.
        /// </summary>
        public static void OnScreenActivated(KScreen screen)
        {
            if (screen == null) return;
            Util.Log.Debug($"Screen activated: {screen.GetType().Name}");
            // Phase 3 will add: determine handler from screen type, push/replace on HandlerStack
        }

        /// <summary>
        /// Called from Harmony prefix on KScreen.Deactivate.
        /// Phase 2: log the deactivation.
        /// Phase 3 will pop/switch handlers based on which screen is closing.
        /// </summary>
        public static void OnScreenDeactivating(KScreen screen)
        {
            if (screen == null) return;
            Util.Log.Debug($"Screen deactivating: {screen.GetType().Name}");
            // Phase 3 will add: pop handler if this screen's handler is on top of stack
        }

        /// <summary>
        /// Detect current game state and activate the appropriate handler.
        /// Called when mod is toggled ON to determine what handler should be active.
        ///
        /// Phase 2: logs the call for debugging. No handlers are pushed because
        /// concrete handler implementations (WorldHandler, etc.) are Phase 3.
        /// Phase 3 will examine KScreenManager.Instance.screenStack for open screens
        /// and push the appropriate handler.
        /// </summary>
        public static void DetectAndActivate()
        {
            Util.Log.Debug("ContextDetector.DetectAndActivate called");
            // Phase 3 will add:
            // 1. Check if Game.Instance exists (might be in main menu)
            // 2. Check KScreenManager screen stack for open screens
            // 3. Push appropriate handler (WorldHandler as default, or menu handler if screen is open)
        }
    }
}
