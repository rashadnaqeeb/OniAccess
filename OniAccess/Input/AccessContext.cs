namespace OniAccess.Input
{
    /// <summary>
    /// Game context for context-sensitive hotkey registration.
    /// Determines which hotkey bindings are active based on the current game state.
    ///
    /// Hotkeys are registered with a context, and only fire when that context
    /// (or a more permissive one) is active.
    ///
    /// Per locked decision: "Context-sensitive: same key can do different things
    /// in different game states (arrows navigate world vs. menu)."
    /// </summary>
    public enum AccessContext
    {
        /// <summary>Active even when mod is off (toggle hotkey only).</summary>
        Always,

        /// <summary>Active whenever mod is on, regardless of game state.</summary>
        Global,

        /// <summary>Main gameplay grid view.</summary>
        WorldView,

        /// <summary>Any KScreen/overlay is active.</summary>
        MenuOpen,

        /// <summary>Build tool selected.</summary>
        BuildMode,

        // Future phases will add more contexts as needed
    }
}
