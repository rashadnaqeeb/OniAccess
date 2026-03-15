using System;
using System.IO;
using System.Runtime.InteropServices;
using HarmonyLib;
using OniAccess.Handlers;
using OniAccess.Handlers.Tiles.Sections;
using OniAccess.Audio;
using OniAccess.Input;
using OniAccess.Speech;
using OniAccess.Util;
using OniAccess.Widgets;
using UnityEngine;

namespace OniAccess {
	public sealed class Mod: KMod.UserMod2 {
		public static Mod Instance { get; private set; }
		public static string ModDir { get; private set; }
		public static string Version { get; private set; }

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool SetDllDirectory(string lpPathName);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		public override void OnLoad(Harmony harmony) {
			Instance = this;
			ModDir = Path.GetDirectoryName(typeof(Mod).Assembly.Location);
			Version = typeof(Mod).Assembly.GetName().Version.ToString();

			// Switch logging from Console (test default) to Unity's Debug.Log
			LogUnityBackend.Install();
			ConfigManager.Load(ModDir);

			// Native DLLs live in a "native" subfolder so ONI's mod loader
			// doesn't try to load them as .NET assemblies. Pre-load each with
			// a full path because: (1) Mono's DllImport ignores SetDllDirectory,
			// and (2) Tolk's internal LoadLibrary for the screen reader drivers
			// needs them already in process since SetDllDirectory can be reset
			// by Harmony patching or other mods.
			string nativeDir = Path.Combine(ModDir, "native");
			foreach (var dll in new[] { "nvdaControllerClient64.dll", "SAAPI64.dll" }) {
				string path = Path.Combine(nativeDir, dll);
				if (LoadLibrary(path) == IntPtr.Zero) {
					Log.Warn($"Failed to pre-load {dll} from: {path}");
				}
			}

			// If tolk_override.dll exists in native/, use it instead of the
			// bundled Tolk.dll. P/Invoke resolves by module base name, so the
			// override is copied to a temp directory as "Tolk.dll".
			string overridePath = Path.Combine(nativeDir, "tolk_override.dll");
			string tolkLoadPath;
			if (File.Exists(overridePath)) {
				string tempDir = Path.Combine(Path.GetTempPath(), "OniAccess");
				Directory.CreateDirectory(tempDir);
				tolkLoadPath = Path.Combine(tempDir, "Tolk.dll");
				File.Copy(overridePath, tolkLoadPath, true);
				Log.Info("Loading tolk_override.dll");
			} else {
				tolkLoadPath = Path.Combine(nativeDir, "Tolk.dll");
			}
			if (LoadLibrary(tolkLoadPath) == IntPtr.Zero) {
				Log.Warn($"Failed to pre-load Tolk from: {tolkLoadPath}");
			}

			base.OnLoad(harmony);

			SpeechEngine.Initialize();
			TextFilter.InitializeDefaults();
			StatusFilter.Initialize();

			// Create persistent KeyPoller MonoBehaviour for unbound key detection
			// (Shift+/, arrows -- keys ONI doesn't generate KButtonEvents for)
			var go = new GameObject("OniAccess_Input");
			UnityEngine.Object.DontDestroyOnLoad(go);
			go.AddComponent<KeyPoller>();

			var audioGo = new GameObject("OniAccess_Audio");
			UnityEngine.Object.DontDestroyOnLoad(audioGo);
			audioGo.AddComponent<EarconScheduler>();
			audioGo.AddComponent<Sonifier>();
			audioGo.AddComponent<ShapeEarconPlayer>();
			new SonifierController();

			// Register screen-to-handler mappings for ContextDetector
			ContextDetector.RegisterMenuHandlers();
			SideScreenOverrides.RegisterAll();

			// Push BaselineHandler so the stack is never empty
			// ModInputRouter will be registered later via Harmony patch on InputInit.Awake
			HandlerStack.Push(new BaselineHandler());

			ModUtil.RegisterForTranslation(typeof(STRINGS.ONIACCESS));

			Log.Info($"Oni-Access version {Version} loaded");
		}
	}
}
