using OniAccess.Speech;

namespace OniAccess.Toggle
{
    /// <summary>
    /// Mod on/off toggle with clean state management.
    ///
    /// Per locked decisions:
    /// - Toggle off: "Oni-Access off" -- speech stops, only toggle hotkey remains active
    /// - Toggle on: "Oni-Access on" -- brief confirmation, no state readout
    /// - Only the toggle hotkey remains active when mod is off (AccessContext.Always)
    /// - No separate mute function
    ///
    /// When the mod is toggled off:
    /// 1. A confirmation message ("Oni-Access off") is spoken
    /// 2. SpeechPipeline is disabled (all subsequent speech calls return immediately)
    /// 3. Only AccessContext.Always bindings fire (the toggle hotkey itself)
    /// 4. Game returns to normal behavior with no errors
    /// </summary>
    public static class VanillaMode
    {
        /// <summary>
        /// Whether the mod is currently enabled. Starts ON.
        /// When false, SpeechPipeline rejects all speech and HotkeyRegistry
        /// only fires Always-context bindings.
        /// </summary>
        public static bool IsEnabled { get; private set; } = true;

        /// <summary>
        /// Toggle the mod on or off.
        /// Speaks confirmation via SpeechPipeline before changing state.
        /// </summary>
        public static void Toggle()
        {
            IsEnabled = !IsEnabled;
            SpeechPipeline.SetEnabled(IsEnabled);

            if (IsEnabled)
            {
                SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.SPEECH.MOD_ON);
            }
            else
            {
                // Must temporarily re-enable pipeline to speak the "off" message,
                // then disable again after the message is dispatched to Tolk
                SpeechPipeline.SetEnabled(true);
                SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.SPEECH.MOD_OFF);
                SpeechPipeline.SetEnabled(false);
            }
        }
    }
}
