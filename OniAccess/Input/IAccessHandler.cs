namespace OniAccess.Input
{
    /// <summary>
    /// Interface for mod input handlers in the handler stack.
    ///
    /// Each game context (world view, menus, build mode, help list) has a handler
    /// that implements this interface. The active handler receives key events from
    /// ModInputRouter and unbound key events from KeyPoller.
    ///
    /// Per locked decisions:
    /// - Selective claim by default: handler uses e.TryConsume to claim specific keys
    /// - Full capture for menus: CapturesAllInput=true blocks all keyboard input
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
        /// Whether this handler captures ALL keyboard input (true for menus).
        /// When true, ModInputRouter consumes all keyboard KButtonEvents after the handler
        /// processes them, EXCEPT mouse actions (MouseLeft, MouseRight, MouseMiddle,
        /// ShiftMouseLeft, ZoomIn, ZoomOut).
        /// When false, only keys the handler explicitly consumes via e.TryConsume are blocked.
        /// </summary>
        bool CapturesAllInput { get; }

        /// <summary>
        /// Help entries for F12 navigable help list. Each handler owns its help text.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<HelpEntry> HelpEntries { get; }

        /// <summary>
        /// Process a key down event from ONI's KButtonEvent system.
        /// Call e.TryConsume(Action.X) to claim a game-mapped key.
        /// Set e.Consumed = true to claim an unmapped key.
        /// Return true if the event was consumed.
        /// </summary>
        bool HandleKeyDown(KButtonEvent e);

        /// <summary>
        /// Process a key up event. Same consumption rules as HandleKeyDown.
        /// </summary>
        bool HandleKeyUp(KButtonEvent e);

        /// <summary>
        /// Called by KeyPoller for keys that have no KButtonEvent (F12, arrows).
        /// keyCode is the UnityEngine.KeyCode that was pressed.
        /// Return true if handled.
        /// </summary>
        bool HandleUnboundKey(UnityEngine.KeyCode keyCode);

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
