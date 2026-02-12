namespace OniAccess.Input
{
    /// <summary>
    /// Interface for mod input handlers in the handler stack.
    ///
    /// Each handler detects its own keys via UnityEngine.Input.GetKeyDown() in Tick(),
    /// called once per frame by KeyPoller. HandleKeyDown exists only for Escape
    /// interception -- Escape is a game Action that must be consumed atomically via
    /// e.TryConsume to prevent the game from also processing it.
    ///
    /// ModInputRouter acts as a gate: it asks the active handler to handle Escape,
    /// then blocks all other non-passthrough keys for CapturesAllInput handlers.
    /// No stack walking, no dispatch.
    ///
    /// Per locked decisions:
    /// - Mode announcements interrupt current speech (OnActivate speaks DisplayName)
    /// - Name first, vary early: "Build menu" not "Menu, build"
    /// </summary>
    public interface IAccessHandler
    {
        /// <summary>
        /// Display name spoken on activation (e.g., "World view", "Build menu").
        /// Per locked decision: name first, vary early ("Build menu" not "Menu, build").
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Whether this handler blocks non-passthrough keys from reaching the game.
        /// When true, ModInputRouter consumes all keyboard events except mouse and Escape.
        /// When false, unconsumed events pass through to the game.
        /// </summary>
        bool CapturesAllInput { get; }

        /// <summary>
        /// Help entries for F12 navigable help list. Each handler owns its help text.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<HelpEntry> HelpEntries { get; }

        /// <summary>
        /// Called once per frame by KeyPoller. All key detection and mod logic
        /// happens here via UnityEngine.Input.GetKeyDown().
        /// </summary>
        void Tick();

        /// <summary>
        /// Process a key down event from ONI's KButtonEvent system.
        /// Used only for Escape interception: call e.TryConsume(Action.Escape)
        /// to atomically claim Escape before ModInputRouter's gate lets it through.
        /// Return true if the event was consumed.
        /// </summary>
        bool HandleKeyDown(KButtonEvent e);

        /// <summary>
        /// Called when this handler becomes the active handler on the stack.
        /// Per locked decision: speak DisplayName here (interrupts current speech).
        /// </summary>
        void OnActivate();

        /// <summary>
        /// Called when this handler is removed from the active position.
        /// </summary>
        void OnDeactivate();
    }
}
