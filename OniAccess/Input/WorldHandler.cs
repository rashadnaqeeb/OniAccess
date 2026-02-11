namespace OniAccess.Input
{
    /// <summary>
    /// Default handler for world view. Selective claim (CapturesAllInput = false).
    /// Active when no menu/overlay is open. Per locked decision: when mod is on,
    /// there is always an active handler -- WorldHandler is that baseline.
    ///
    /// Phase 2: only handles F12 for help.
    /// Phase 4 adds arrow keys for cursor movement.
    /// </summary>
    public class WorldHandler : IAccessHandler
    {
        public string DisplayName => STRINGS.ONIACCESS.HANDLERS.WORLD_VIEW;
        public bool CapturesAllInput => false;

        public System.Collections.Generic.IReadOnlyList<HelpEntry> HelpEntries { get; }
            = new System.Collections.Generic.List<HelpEntry>
            {
                new HelpEntry("F12", STRINGS.ONIACCESS.HOTKEYS.CONTEXT_HELP),
                new HelpEntry("Ctrl+Shift+F12", STRINGS.ONIACCESS.HOTKEYS.TOGGLE_MOD),
            }.AsReadOnly();

        public bool HandleKeyDown(KButtonEvent e) => false;
        public bool HandleKeyUp(KButtonEvent e) => false;

        public bool HandleUnboundKey(UnityEngine.KeyCode keyCode)
        {
            if (keyCode == UnityEngine.KeyCode.F12)
            {
                // Push HelpHandler with THIS handler's help entries
                HandlerStack.Push(new HelpHandler(HelpEntries));
                return true;
            }
            return false;
        }

        public void OnActivate()
        {
            Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
        }

        public void OnDeactivate() { }
    }
}
