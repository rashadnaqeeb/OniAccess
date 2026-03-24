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

			// Pre-load the platform-specific Prism native library so that
			// SpeechEngine's DllImport("prism") resolves correctly.
			// Mono's DllImport doesn't reliably search subdirectories,
			// so we load from an explicit path.
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
