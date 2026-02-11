namespace OniAccess.Input
{
    /// <summary>
    /// Interface for mod input handlers in the handler stack.
    ///
    /// Each game context (world view, menus, build mode, help list) has a handler
    /// that implements this interface. Input dispatch walks the stack top-to-bottom:
    /// each handler gets a chance to consume the event. Unconsumed events fall
    /// through to the next handler unless CapturesAllInput=true blocks fallthrough.
    ///
    /// Per locked decisions:
    /// - Selective claim by default: handler uses e.TryConsume to claim specific keys
    /// - Full capture for modals: CapturesAllInput=true blocks all keyboard fallthrough
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
        /// Whether this handler blocks fallthrough to handlers below it.
        /// When true, unconsumed keyboard events stop here (not passed to lower handlers
        /// or the game), EXCEPT mouse actions (MouseLeft, MouseRight, MouseMiddle,
        /// ShiftMouseLeft, ZoomIn, ZoomOut). Use for modal contexts like help lists.
        /// When false, unconsumed events fall through to the next handler down the stack.
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
