using System;
using System.IO;
using System.Runtime.InteropServices;
using HarmonyLib;
using OniAccess.Input;
using OniAccess.Speech;
using OniAccess.Toggle;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess
{
    public sealed class Mod : KMod.UserMod2
    {
        public static Mod Instance { get; private set; }
        public static string ModDir { get; private set; }
        public static string Version { get; private set; }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            ModDir = Path.GetDirectoryName(typeof(Mod).Assembly.Location);
            Version = typeof(Mod).Assembly.GetName().Version.ToString();

            // Set DLL search path for Tolk native libraries before any Tolk calls
            string tolkDir = Path.Combine(ModDir, "tolk", "dist");
            if (!SetDllDirectory(tolkDir))
            {
                Log.Error($"Failed to set DLL directory to: {tolkDir}");
            }

            base.OnLoad(harmony);

            SpeechEngine.Initialize();
            TextFilter.InitializeDefaults();

            // Register Phase 1 hotkeys before startup speech
            RegisterHotkeys();

            // Create persistent InputInterceptor MonoBehaviour for keyboard handling
            var go = new GameObject("OniAccess_Input");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<InputInterceptor>();

            // All speech goes through the pipeline -- never call SpeechEngine.Say directly
            SpeechPipeline.SpeakInterrupt(
                string.Format(STRINGS.ONIACCESS.SPEECH.MOD_LOADED, Version));

            Localization.RegisterForTranslation(typeof(STRINGS.ONIACCESS));

            Log.Info($"Oni-Access version {Version} loaded");
        }

        /// <summary>
        /// Register Phase 1 hotkeys: toggle (Ctrl+Shift+F12) and context help (F12).
        /// Additional hotkeys will be registered by later phases.
        /// </summary>
        private static void RegisterHotkeys()
        {
            // Toggle: Ctrl+Shift+F12, Always context (active even when mod is off)
            // OriginalFunction: null (F12 is not used by ONI, Ctrl+Shift+F12 even less)
            HotkeyRegistry.Register(new HotkeyBinding(
                KeyCode.F12,
                HotkeyModifier.CtrlShift,
                AccessContext.Always,
                STRINGS.ONIACCESS.HOTKEYS.TOGGLE_MOD,
                VanillaMode.Toggle,
                originalFunction: null
            ));

            // Context Help: F12 (no modifiers), Global context (active when mod is on)
            // OriginalFunction: null (F12 not bound in ONI)
            HotkeyRegistry.Register(new HotkeyBinding(
                KeyCode.F12,
                HotkeyModifier.None,
                AccessContext.Global,
                STRINGS.ONIACCESS.HOTKEYS.CONTEXT_HELP,
                SpeakContextHelp,
                originalFunction: null
            ));

            Log.Info("Phase 1 hotkeys registered: Ctrl+Shift+F12 (toggle), F12 (help)");
        }

        /// <summary>
        /// Handler for the context help hotkey (F12).
        /// Speaks the help text listing available commands for the current context.
        /// </summary>
        private static void SpeakContextHelp()
        {
            // Determine current context for help text
            AccessContext context;
            try
            {
                context = Game.Instance != null ? AccessContext.WorldView : AccessContext.Global;
            }
            catch
            {
                context = AccessContext.Global;
            }

            string helpText = HotkeyRegistry.GetHelpText(context);
            SpeechPipeline.SpeakInterrupt(helpText);
        }
    }
}
