using System;
using System.IO;
using System.Runtime.InteropServices;
using HarmonyLib;
using OniAccess.Handlers;
using OniAccess.Handlers.Tiles.Sections;
using OniAccess.Input;
using OniAccess.Speech;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess {
	public sealed class Mod: KMod.UserMod2 {
		public static Mod Instance { get; private set; }
		public static string ModDir { get; private set; }
		public static string Version { get; private set; }

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetDllDirectory(string lpPathName);

		public override void OnLoad(Harmony harmony) {
			Instance = this;
			ModDir = Path.GetDirectoryName(typeof(Mod).Assembly.Location);
			Version = typeof(Mod).Assembly.GetName().Version.ToString();

			// Switch logging from Console (test default) to Unity's Debug.Log
			LogUnityBackend.Install();
			ConfigManager.Load(ModDir);

			// Set DLL search path for Tolk native libraries before any Tolk calls
			string tolkDir = Path.Combine(ModDir, "tolk", "dist");
			if (!SetDllDirectory(tolkDir)) {
				Log.Error($"Failed to set DLL directory to: {tolkDir}");
			}

			base.OnLoad(harmony);

			SpeechEngine.Initialize();
			TextFilter.InitializeDefaults();
			StatusFilter.Initialize();

			// Create persistent KeyPoller MonoBehaviour for unbound key detection
			// (F12, arrows -- keys ONI doesn't generate KButtonEvents for)
			var go = new GameObject("OniAccess_Input");
			UnityEngine.Object.DontDestroyOnLoad(go);
			go.AddComponent<KeyPoller>();

			// Register screen-to-handler mappings for ContextDetector
			ContextDetector.RegisterMenuHandlers();

			// Push BaselineHandler so the stack is never empty
			// ModInputRouter will be registered later via Harmony patch on InputInit.Awake
			HandlerStack.Push(new BaselineHandler());

			// Startup announcement
			SpeechPipeline.SpeakInterrupt(
				string.Format(STRINGS.ONIACCESS.SPEECH.MOD_LOADED, Version));

			Localization.RegisterForTranslation(typeof(STRINGS.ONIACCESS));

			Log.Info($"Oni-Access version {Version} loaded");
		}
	}
}
