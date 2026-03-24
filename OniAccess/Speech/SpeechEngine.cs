using System;
using System.Runtime.InteropServices;
using OniAccess.Util;

namespace OniAccess.Speech {
	/// <summary>
	/// Prism P/Invoke wrapper providing cross-platform screen reader speech output.
	/// Say() passes text directly to Prism without filtering;
	/// filtering is handled by TextFilter via SpeechPipeline.
	/// </summary>
	public static class SpeechEngine {
		// PrismConfig layout matches the Windows native struct (largest variant).
		// On Linux/macOS the native struct is just { byte version } but passing
		// a pointer to the larger struct is safe — prism_init only reads what
		// its platform sizeof dictates.
		[StructLayout(LayoutKind.Sequential)]
		private struct PrismConfig {
			public byte version;
			public IntPtr hwnd;
		}

		const int PRISM_OK = 0;
		const int PRISM_ERROR_NOT_SPEAKING = 10;

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr prism_init(ref PrismConfig cfg);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern void prism_shutdown(IntPtr ctx);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr prism_registry_acquire_best(IntPtr ctx);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr prism_backend_name(IntPtr backend);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int prism_backend_speak(IntPtr backend, string text, bool interrupt);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_stop(IntPtr backend);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern void prism_backend_free(IntPtr backend);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr prism_error_string(int error);

		private static IntPtr _context = IntPtr.Zero;
		private static IntPtr _backend = IntPtr.Zero;
		private static bool _initialized = false;
		private static bool _available = false;

		/// <summary>
		/// Whether the speech engine has been initialized (regardless of availability).
		/// </summary>
		public static bool IsInitialized => _initialized;

		/// <summary>
		/// Whether a screen reader or TTS backend is available for speech output.
		/// </summary>
		public static bool IsAvailable => _available;

		/// <summary>
		/// Initialize Prism. Must be called after Mod.OnLoad pre-loads
		/// the platform-specific Prism native library.
		/// </summary>
		public static bool Initialize() {
			if (_initialized) return _available;

			try {
				var config = new PrismConfig { version = 1, hwnd = IntPtr.Zero };
				_context = prism_init(ref config);
				if (_context == IntPtr.Zero) {
					Log.Error("prism_init returned null");
					_initialized = true;
					_available = false;
					return false;
				}

				_backend = prism_registry_acquire_best(_context);
				_available = _backend != IntPtr.Zero;
				_initialized = true;

				if (_available) {
					IntPtr namePtr = prism_backend_name(_backend);
					string name = namePtr != IntPtr.Zero
						? Marshal.PtrToStringAnsi(namePtr)
						: "unknown";
					Log.Info($"Speech initialized with: {name}");
				} else {
					Log.Warn("No speech backend available");
				}

				return _available;
			} catch (DllNotFoundException ex) {
				Log.Error($"Prism native library not found: {ex}");
				_initialized = true;
				_available = false;
				return false;
			} catch (Exception ex) {
				Log.Error($"Speech init failed: {ex}");
				_initialized = true;
				_available = false;
				return false;
			}
		}

		/// <summary>
		/// Shutdown Prism and release resources.
		/// </summary>
		public static void Shutdown() {
			if (!_initialized) return;

			try {
				if (_backend != IntPtr.Zero) {
					prism_backend_free(_backend);
					_backend = IntPtr.Zero;
				}
				if (_context != IntPtr.Zero) {
					prism_shutdown(_context);
					_context = IntPtr.Zero;
				}
				Log.Info("Speech shutdown");
			} catch (Exception ex) {
				Log.Warn($"Speech shutdown error: {ex}");
			} finally {
				_initialized = false;
				_available = false;
			}
		}

		/// <summary>
		/// Speak the given text through the screen reader.
		/// Text is passed directly to Prism without filtering.
		/// </summary>
		/// <param name="text">Text to speak</param>
		/// <param name="interrupt">If true, interrupts any current speech</param>
		internal static void Say(string text, bool interrupt = true) {
			if (!_available || string.IsNullOrEmpty(text)) return;

			try {
				int err = prism_backend_speak(_backend, text, interrupt);
				if (err != PRISM_OK) {
					IntPtr msgPtr = prism_error_string(err);
					string msg = msgPtr != IntPtr.Zero
						? Marshal.PtrToStringAnsi(msgPtr)
						: $"error code {err}";
					Log.Warn($"Speech error: {msg}");
				}
			} catch (Exception ex) {
				Log.Warn($"Speech error: {ex}");
			}
		}

		/// <summary>
		/// Stop any current speech output.
		/// </summary>
		public static void Stop() {
			if (!_available) return;

			try {
				int err = prism_backend_stop(_backend);
				if (err != PRISM_OK && err != PRISM_ERROR_NOT_SPEAKING) {
					IntPtr msgPtr = prism_error_string(err);
					string msg = msgPtr != IntPtr.Zero
						? Marshal.PtrToStringAnsi(msgPtr)
						: $"error code {err}";
					Log.Warn($"Speech stop error: {msg}");
				}
			} catch (Exception ex) {
				Log.Warn($"Speech stop error: {ex}");
			}
		}
	}
}
