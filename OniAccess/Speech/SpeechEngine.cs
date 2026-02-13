using System;
using System.Runtime.InteropServices;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Speech {
	/// <summary>
	/// Tolk P/Invoke wrapper providing screen reader speech output.
	/// Evolved from Speech.cs -- preserves all working Tolk signatures
	/// and initialization logic with proper lifecycle management.
	///
	/// Text filtering is NOT handled here (deferred to TextFilter.cs in Plan 02).
	/// Say() passes text directly to Tolk without filtering.
	/// </summary>
	public static class SpeechEngine {
		// Tolk P/Invoke declarations (preserved from Speech.cs)
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

		// State tracking (preserved from Speech.cs)
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
		/// Initialize Tolk. Must be called after SetDllDirectory points
		/// to the Tolk DLL location.
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
				Log.Error($"Tolk.dll not found: {ex.Message}");
				_initialized = true;
				_available = false;
				return false;
			} catch (Exception ex) {
				Log.Error($"Speech init failed: {ex.Message}");
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
				Log.Error($"Speech shutdown error: {ex.Message}");
			} finally {
				_initialized = false;
				_available = false;
			}
		}

		/// <summary>
		/// Speak the given text through the screen reader.
		/// Text is passed directly to Tolk without filtering.
		/// Plan 02 will insert the TextFilter pipeline before the Tolk call.
		/// </summary>
		/// <param name="text">Text to speak</param>
		/// <param name="interrupt">If true, interrupts any current speech</param>
		internal static void Say(string text, bool interrupt = true) {
			if (!_available || string.IsNullOrEmpty(text)) return;

			try {
				Tolk_Output(text, interrupt);
			} catch (Exception ex) {
				Log.Error($"Speech error: {ex.Message}");
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
				Log.Error($"Speech stop error: {ex.Message}");
			}
		}
	}
}
