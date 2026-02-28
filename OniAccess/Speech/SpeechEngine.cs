using System;
using System.Runtime.InteropServices;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Speech {
	/// <summary>
	/// Tolk P/Invoke wrapper providing screen reader speech output.
	/// Say() passes text directly to Tolk without filtering;
	/// filtering is handled by TextFilter via SpeechPipeline.
	/// </summary>
	public static class SpeechEngine {
		// Tolk P/Invoke declarations
		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void Tolk_Load();

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void Tolk_Unload();

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern bool Tolk_Output(string str, bool interrupt);

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool Tolk_TrySAPI(bool trySAPI);

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool Tolk_HasSpeech();

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern IntPtr Tolk_DetectScreenReader();

		[DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool Tolk_Silence();

		// State tracking
		private static bool _initialized = false;
		private static bool _available = false;

		/// <summary>
		/// Whether the speech engine has been initialized (regardless of availability).
		/// </summary>
		public static bool IsInitialized => _initialized;

		/// <summary>
		/// Whether a screen reader or SAPI is available for speech output.
		/// </summary>
		public static bool IsAvailable => _available;

		/// <summary>
		/// Initialize Tolk. Must be called after Mod.OnLoad pre-loads
		/// Tolk.dll via LoadLibrary and sets SetDllDirectory for the
		/// screen reader driver DLLs.
		/// </summary>
		public static bool Initialize() {
			if (_initialized) return _available;

			try {
				Tolk_Load();
				Tolk_TrySAPI(true); // Enable SAPI fallback for users without screen readers

				_available = Tolk_HasSpeech();
				_initialized = true;

				// Log which screen reader was detected
				IntPtr readerPtr = Tolk_DetectScreenReader();
				string reader = readerPtr != IntPtr.Zero
					? Marshal.PtrToStringUni(readerPtr)
					: "SAPI (fallback)";
				Log.Info($"Speech initialized with: {reader}");

				return _available;
			} catch (DllNotFoundException ex) {
				Log.Error($"Tolk.dll not found: {ex}");
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
		/// Shutdown Tolk and release resources.
		/// </summary>
		public static void Shutdown() {
			if (!_initialized) return;

			try {
				Tolk_Unload();
				Log.Info("Speech shutdown");
			} catch (Exception ex) {
				Log.Error($"Speech shutdown error: {ex}");
			} finally {
				_initialized = false;
				_available = false;
			}
		}

		/// <summary>
		/// Speak the given text through the screen reader.
		/// Text is passed directly to Tolk without filtering.
		/// </summary>
		/// <param name="text">Text to speak</param>
		/// <param name="interrupt">If true, interrupts any current speech</param>
		internal static void Say(string text, bool interrupt = true) {
			if (!_available || string.IsNullOrEmpty(text)) return;

			try {
				Tolk_Output(text, interrupt);
			} catch (Exception ex) {
				Log.Error($"Speech error: {ex}");
			}
		}

		/// <summary>
		/// Stop any current speech output using Tolk_Silence.
		/// </summary>
		public static void Stop() {
			if (!_available) return;

			try {
				Tolk_Silence();
			} catch (Exception ex) {
				Log.Error($"Speech stop error: {ex}");
			}
		}
	}
}
