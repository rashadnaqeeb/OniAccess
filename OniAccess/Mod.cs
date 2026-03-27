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
		public static string DataDir { get; private set; }
		public static string Version { get; private set; }

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("libdl", EntryPoint = "dlopen")]
		private static extern IntPtr DlOpen(string path, int flags);

		const int RTLD_NOW = 2;

		public override void OnLoad(Harmony harmony) {
			Instance = this;
			ModDir = Path.GetDirectoryName(typeof(Mod).Assembly.Location);
			DataDir = Path.Combine(global::Util.RootFolder(), "mods", "OniAccess");
			Version = typeof(Mod).Assembly.GetName().Version.ToString();

			// Switch logging from Console (test default) to Unity's Debug.Log
			LogUnityBackend.Install();
			ConfigManager.Load(DataDir);

			// Check for Tolk override on Windows before loading Prism
			string tolkOverridePath = Path.Combine(DataDir, "tolk_override.dll");
			bool useTolk = Application.platform == RuntimePlatform.WindowsPlayer
				&& File.Exists(tolkOverridePath);

			if (useTolk) {
				Log.Info("Using Tolk override backend");
				string tempDir = Path.Combine(Path.GetTempPath(), "OniAccess");
				Directory.CreateDirectory(tempDir);

				// Copy all DLLs from DataDir to temp dir (companion DLLs + Tolk)
				foreach (string dll in Directory.GetFiles(DataDir, "*.dll")) {
					string fileName = Path.GetFileName(dll);
					string destName = fileName.Equals("tolk_override.dll", StringComparison.OrdinalIgnoreCase)
						? "Tolk.dll"
						: fileName;
					File.Copy(dll, Path.Combine(tempDir, destName), true);
				}

				// Pre-load companion DLLs, then Tolk itself
				foreach (string dll in Directory.GetFiles(tempDir, "*.dll")) {
					if (Path.GetFileName(dll).Equals("Tolk.dll", StringComparison.OrdinalIgnoreCase))
						continue;
					LoadLibrary(dll);
				}
				if (LoadLibrary(Path.Combine(tempDir, "Tolk.dll")) == IntPtr.Zero)
					Log.Warn("Failed to pre-load Tolk.dll from temp dir");
			} else {
				// Pre-load the platform-specific Prism native library so that
				// PrismBackend's DllImport("prism") resolves correctly.
				string platform;
				string libName;
				switch (Application.platform) {
					case RuntimePlatform.WindowsPlayer:
						platform = "win-x64";
						libName = "prism.dll";
						break;
					case RuntimePlatform.LinuxPlayer:
						platform = "linux-x64";
						libName = "libprism.so";
						break;
					case RuntimePlatform.OSXPlayer:
						platform = "osx";
						libName = "libprism.dylib";
						break;
					default:
						Log.Error($"Unsupported platform: {Application.platform}");
						return;
				}
				string libPath = Path.Combine(ModDir, "native", platform, libName);
				if (Application.platform == RuntimePlatform.WindowsPlayer) {
					if (LoadLibrary(libPath) == IntPtr.Zero)
						Log.Warn($"Failed to pre-load Prism from: {libPath}");
				} else {
					if (DlOpen(libPath, RTLD_NOW) == IntPtr.Zero)
						Log.Warn($"Failed to pre-load Prism from: {libPath}");
				}
			}

			base.OnLoad(harmony);

			ISpeechBackend speechBackend = useTolk
				? (ISpeechBackend)new TolkBackend()
				: new PrismBackend();
			SpeechEngine.Initialize(speechBackend);
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
			audioGo.AddComponent<FollowMovementEarcon>();
			audioGo.AddComponent<ScannerDirectionEarcon>();
			new SonifierController();
			new FootstepPlayer();

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
