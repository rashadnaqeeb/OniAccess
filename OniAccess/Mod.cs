using System;
using System.IO;
using System.Runtime.InteropServices;
using HarmonyLib;
using OniAccess.Speech;
using OniAccess.Util;

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

            // All speech goes through the pipeline -- never call SpeechEngine.Say directly
            SpeechPipeline.SpeakInterrupt(
                string.Format(STRINGS.ONIACCESS.SPEECH.MOD_LOADED, Version));

            Localization.RegisterForTranslation(typeof(STRINGS.ONIACCESS));

            Log.Info($"Oni-Access version {Version} loaded");
        }
    }
}
