using System;
using System.Runtime.InteropServices;
using OniAccess.Util;

namespace OniAccess.Speech {
	public class TolkBackend : ISpeechBackend {
		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void Tolk_Load();

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void Tolk_Unload();

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool Tolk_HasSpeech();

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void Tolk_TrySAPI(bool trySAPI);

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern IntPtr Tolk_DetectScreenReader();

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern bool Tolk_Output(string text, bool interrupt);

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool Tolk_Silence();

		private bool _initialized = false;
		private bool _available = false;

		public bool IsInitialized => _initialized;
		public bool IsAvailable => _available;

		public bool Initialize() {
			if (_initialized) return _available;

			try {
				Tolk_Load();
				Tolk_TrySAPI(true);
				_available = Tolk_HasSpeech();
				_initialized = true;

				if (_available) {
					IntPtr namePtr = Tolk_DetectScreenReader();
					string name = namePtr != IntPtr.Zero
						? Marshal.PtrToStringUni(namePtr)
						: "unknown";
					Log.Info($"Tolk backend initialized with: {name}");
				} else {
					Log.Warn("Tolk found no speech output");
				}

				return _available;
			} catch (DllNotFoundException ex) {
				Log.Error($"Tolk.dll not found: {ex}");
				_initialized = true;
				_available = false;
				return false;
			} catch (Exception ex) {
				Log.Error($"Tolk init failed: {ex}");
				_initialized = true;
				_available = false;
				return false;
			}
		}

		public void Shutdown() {
			if (!_initialized) return;

			try {
				Tolk_Unload();
				Log.Info("Tolk backend shutdown");
			} catch (Exception ex) {
				Log.Warn($"Tolk shutdown error: {ex}");
			} finally {
				_initialized = false;
				_available = false;
			}
		}

		public void Say(string text, bool interrupt) {
			if (!_available || string.IsNullOrEmpty(text)) return;

			try {
				if (!Tolk_Output(text, interrupt))
					Log.Warn("Tolk_Output returned false");
			} catch (Exception ex) {
				Log.Warn($"Tolk speech error: {ex}");
			}
		}

		public void Stop() {
			if (!_available) return;

			try {
				Tolk_Silence();
			} catch (Exception ex) {
				Log.Warn($"Tolk stop error: {ex}");
			}
		}
	}
}
