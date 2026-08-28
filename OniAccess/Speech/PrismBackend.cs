using System;
using System.Runtime.InteropServices;
using System.Text;
using OniAccess.Util;

namespace OniAccess.Speech {
	public class PrismBackend: ISpeechBackend {
		[StructLayout(LayoutKind.Sequential)]
		private struct PrismConfig {
			public byte version;
		}

		const int PRISM_OK = 0;
		const int PRISM_ERROR_NOT_SPEAKING = 10;
		const int PRISM_ERROR_ALREADY_INITIALIZED = 15;

		/// <summary>Registry ids from prism.h. Zero asks Prism for whichever backend it ranks best.</summary>
		public const ulong BACKEND_BEST = 0;
		public const ulong BACKEND_SAPI = 0x1D6DF72422CEEE66;
		public const ulong BACKEND_AV_SPEECH = 0x28E3429577805C24;
		public const ulong BACKEND_SPEECH_DISPATCHER = 0xE3D6F895D949EBFE;

		// PrismBackendFeature bits
		const ulong FEATURE_SET_VOLUME = 1UL << 10;
		const ulong FEATURE_SET_RATE = 1UL << 12;
		const ulong FEATURE_GET_RATE = 1UL << 13;
		const ulong FEATURE_COUNT_VOICES = 1UL << 17;
		const ulong FEATURE_GET_VOICE_NAME = 1UL << 18;
		const ulong FEATURE_SET_VOICE = 1UL << 21;
		const ulong FEATURES_VOICE_CONTROL = FEATURE_SET_VOLUME | FEATURE_SET_RATE | FEATURE_GET_RATE
			| FEATURE_COUNT_VOICES | FEATURE_GET_VOICE_NAME | FEATURE_SET_VOICE;

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern PrismConfig prism_config_init();

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr prism_init(ref PrismConfig cfg);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern void prism_shutdown(IntPtr ctx);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr prism_registry_acquire_best(IntPtr ctx);

		// Returns a cached or freshly created instance that is not necessarily
		// initialized; prism_backend_initialize must follow.
		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr prism_registry_acquire(IntPtr ctx, ulong id);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_initialize(IntPtr backend);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr prism_backend_name(IntPtr backend);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern ulong prism_backend_get_features(IntPtr backend);

		// Prism requires UTF-8 and rejects anything else with PRISM_ERROR_INVALID_UTF8.
		// .NET has no UTF-8 CharSet on Framework, so the text is converted to a
		// null-terminated UTF-8 byte[] by the caller and marshaled as a raw pointer.
		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_speak(IntPtr backend, byte[] text, bool interrupt);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_stop(IntPtr backend);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern void prism_backend_free(IntPtr backend);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr prism_error_string(int error);

		// Volume and rate are normalized to [0.0, 1.0]; 0.5 is the backend's default rate.
		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_set_volume(IntPtr backend, float volume);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_set_rate(IntPtr backend, float rate);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_get_rate(IntPtr backend, out float rate);

		// Voice ids are zero-based indices into the backend's current voice list.
		// size_t marshals as UIntPtr.
		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_count_voices(IntPtr backend, out UIntPtr count);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_get_voice_name(IntPtr backend, UIntPtr voiceId, out IntPtr name);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_get_voice_language(IntPtr backend, UIntPtr voiceId, out IntPtr language);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_set_voice(IntPtr backend, UIntPtr voiceId);

		[DllImport("prism", CallingConvention = CallingConvention.Cdecl)]
		private static extern int prism_backend_get_voice(IntPtr backend, out UIntPtr voiceId);

		private readonly ulong _requested;
		private IntPtr _context = IntPtr.Zero;
		private IntPtr _backend = IntPtr.Zero;
		private bool _initialized = false;
		private bool _available = false;

		public bool IsInitialized => _initialized;
		public bool IsAvailable => _available;

		/// <summary>Create a backend that asks Prism for its best-ranked output.</summary>
		public PrismBackend() : this(BACKEND_BEST) { }

		/// <summary>
		/// Create a backend bound to a specific Prism registry id (a BACKEND_* constant).
		/// When that backend cannot be acquired or initialized, Initialize falls back to
		/// Prism's best-ranked one and logs the fallback.
		/// </summary>
		public PrismBackend(ulong backendId) {
			_requested = backendId;
		}

		/// <summary>Prism's name for the running backend, such as "AVSpeech" or "NVDA". Null when unavailable.</summary>
		public string Name {
			get {
				if (!_available) return null;
				try {
					return PtrToUtf8(prism_backend_name(_backend));
				} catch (Exception ex) {
					Log.Warn($"Prism backend name error: {ex}");
					return null;
				}
			}
		}

		/// <summary>
		/// True when the running backend lets the mod choose the voice, rate, and volume.
		/// Screen reader backends do not; system TTS backends (AVSpeech, SAPI) do.
		/// </summary>
		public bool SupportsVoiceControl {
			get {
				if (!_available) return false;
				try {
					return (prism_backend_get_features(_backend) & FEATURES_VOICE_CONTROL) == FEATURES_VOICE_CONTROL;
				} catch (Exception ex) {
					Log.Warn($"Prism feature query error: {ex}");
					return false;
				}
			}
		}

		public bool Initialize() {
			if (_initialized) return _available;

			try {
				var config = prism_config_init();
				_context = prism_init(ref config);
				if (_context == IntPtr.Zero) {
					Log.Error("prism_init returned null");
					_initialized = true;
					_available = false;
					return false;
				}

				if (_requested != BACKEND_BEST) {
					_backend = AcquireRequested();
					if (_backend == IntPtr.Zero)
						Log.Warn($"Prism backend {_requested:X} unavailable, falling back to the best available");
				}
				if (_backend == IntPtr.Zero)
					_backend = prism_registry_acquire_best(_context);
				_available = _backend != IntPtr.Zero;
				_initialized = true;

				if (_available)
					Log.Info($"Prism backend initialized with: {Name ?? "unknown"}");
				else
					Log.Warn("No Prism speech backend available");

				return _available;
			} catch (DllNotFoundException ex) {
				Log.Error($"Prism native library not found: {ex}");
				_initialized = true;
				_available = false;
				return false;
			} catch (Exception ex) {
				Log.Error($"Prism init failed: {ex}");
				_initialized = true;
				_available = false;
				return false;
			}
		}

		/// <summary>
		/// Acquire and initialize the requested backend. Returns IntPtr.Zero when Prism
		/// has no such backend or it fails to initialize (the instance is freed).
		/// </summary>
		private IntPtr AcquireRequested() {
			IntPtr backend = prism_registry_acquire(_context, _requested);
			if (backend == IntPtr.Zero) return IntPtr.Zero;
			int err = prism_backend_initialize(backend);
			if (err == PRISM_OK || err == PRISM_ERROR_ALREADY_INITIALIZED) return backend;
			Log.Warn($"Prism backend {_requested:X} failed to initialize: {ErrorText(err)}");
			prism_backend_free(backend);
			return IntPtr.Zero;
		}

		public void Shutdown() {
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
				Log.Info("Prism backend shutdown");
			} catch (Exception ex) {
				Log.Warn($"Prism shutdown error: {ex}");
			} finally {
				_initialized = false;
				_available = false;
			}
		}

		/// <summary>
		/// Encodes text as the null-terminated UTF-8 bytes Prism expects. Prism reads
		/// to the first null and validates the bytes as UTF-8, so the terminator is
		/// mandatory and the encoding must be UTF-8 (not the system ANSI code page).
		/// </summary>
		internal static byte[] ToUtf8(string text) {
			int count = Encoding.UTF8.GetByteCount(text);
			var bytes = new byte[count + 1]; // trailing slot stays 0 = null terminator
			Encoding.UTF8.GetBytes(text, 0, text.Length, bytes, 0);
			return bytes;
		}

		/// <summary>
		/// Decodes a null-terminated UTF-8 string Prism owns (backend and voice names).
		/// Marshal.PtrToStringAnsi would run the bytes through the system code page
		/// and mangle any non-ASCII voice name. Null pointer decodes to null.
		/// </summary>
		internal static string PtrToUtf8(IntPtr ptr) {
			if (ptr == IntPtr.Zero) return null;
			int length = 0;
			while (Marshal.ReadByte(ptr, length) != 0) length++;
			var bytes = new byte[length];
			Marshal.Copy(ptr, bytes, 0, length);
			return Encoding.UTF8.GetString(bytes);
		}

		private static string ErrorText(int err) {
			IntPtr msgPtr = prism_error_string(err);
			return msgPtr != IntPtr.Zero
				? Marshal.PtrToStringAnsi(msgPtr)
				: $"error code {err}";
		}

		public void Say(string text, bool interrupt) {
			if (!_available || string.IsNullOrEmpty(text)) return;

			try {
				int err = prism_backend_speak(_backend, ToUtf8(text), interrupt);
				if (err != PRISM_OK)
					Log.Warn($"Prism speech error: {ErrorText(err)}");
			} catch (Exception ex) {
				Log.Warn($"Prism speech error: {ex}");
			}
		}

		public void Stop() {
			if (!_available) return;

			try {
				int err = prism_backend_stop(_backend);
				if (err != PRISM_OK && err != PRISM_ERROR_NOT_SPEAKING)
					Log.Warn($"Prism stop error: {ErrorText(err)}");
			} catch (Exception ex) {
				Log.Warn($"Prism stop error: {ex}");
			}
		}

		// ========================================
		// VOICE CONTROL (system TTS backends only)
		// ========================================

		/// <summary>Number of voices the backend offers, or 0 when unavailable or unsupported.</summary>
		public int VoiceCount {
			get {
				if (!_available) return 0;
				try {
					int err = prism_backend_count_voices(_backend, out UIntPtr count);
					if (err != PRISM_OK) {
						Log.Warn($"Prism voice count error: {ErrorText(err)}");
						return 0;
					}
					return (int)count.ToUInt64();
				} catch (Exception ex) {
					Log.Warn($"Prism voice count error: {ex}");
					return 0;
				}
			}
		}

		/// <summary>Human-readable name of voice <paramref name="index"/>, or null on failure.</summary>
		public string GetVoiceName(int index) {
			if (!_available) return null;
			try {
				int err = prism_backend_get_voice_name(_backend, (UIntPtr)index, out IntPtr name);
				if (err != PRISM_OK) {
					Log.Warn($"Prism voice name error for voice {index}: {ErrorText(err)}");
					return null;
				}
				return PtrToUtf8(name);
			} catch (Exception ex) {
				Log.Warn($"Prism voice name error: {ex}");
				return null;
			}
		}

		/// <summary>Language tag of voice <paramref name="index"/> (typically BCP 47, e.g. "en-US"), or null on failure.</summary>
		public string GetVoiceLanguage(int index) {
			if (!_available) return null;
			try {
				int err = prism_backend_get_voice_language(_backend, (UIntPtr)index, out IntPtr language);
				if (err != PRISM_OK) {
					Log.Warn($"Prism voice language error for voice {index}: {ErrorText(err)}");
					return null;
				}
				return PtrToUtf8(language);
			} catch (Exception ex) {
				Log.Warn($"Prism voice language error: {ex}");
				return null;
			}
		}

		/// <summary>Index of the voice in use, or -1 on failure.</summary>
		public int CurrentVoice {
			get {
				if (!_available) return -1;
				try {
					int err = prism_backend_get_voice(_backend, out UIntPtr id);
					if (err != PRISM_OK) {
						Log.Warn($"Prism current voice error: {ErrorText(err)}");
						return -1;
					}
					return (int)id.ToUInt64();
				} catch (Exception ex) {
					Log.Warn($"Prism current voice error: {ex}");
					return -1;
				}
			}
		}

		public bool SetVoice(int index) {
			if (!_available) return false;
			try {
				int err = prism_backend_set_voice(_backend, (UIntPtr)index);
				if (err != PRISM_OK) {
					Log.Warn($"Prism set voice {index} error: {ErrorText(err)}");
					return false;
				}
				return true;
			} catch (Exception ex) {
				Log.Warn($"Prism set voice error: {ex}");
				return false;
			}
		}

		/// <param name="rate">Normalized rate in [0, 1]; 0.5 is the voice's default speed.</param>
		public bool SetRate(float rate) {
			if (!_available) return false;
			try {
				int err = prism_backend_set_rate(_backend, rate);
				if (err != PRISM_OK) {
					Log.Warn($"Prism set rate {rate} error: {ErrorText(err)}");
					return false;
				}
				return true;
			} catch (Exception ex) {
				Log.Warn($"Prism set rate error: {ex}");
				return false;
			}
		}

		/// <summary>The rate in use, normalized to [0, 1], or -1 on failure.</summary>
		public float GetRate() {
			if (!_available) return -1f;
			try {
				int err = prism_backend_get_rate(_backend, out float rate);
				if (err != PRISM_OK) {
					Log.Warn($"Prism get rate error: {ErrorText(err)}");
					return -1f;
				}
				return rate;
			} catch (Exception ex) {
				Log.Warn($"Prism get rate error: {ex}");
				return -1f;
			}
		}

		/// <param name="volume">Normalized volume in [0, 1].</param>
		public bool SetVolume(float volume) {
			if (!_available) return false;
			try {
				int err = prism_backend_set_volume(_backend, volume);
				if (err != PRISM_OK) {
					Log.Warn($"Prism set volume {volume} error: {ErrorText(err)}");
					return false;
				}
				return true;
			} catch (Exception ex) {
				Log.Warn($"Prism set volume error: {ex}");
				return false;
			}
		}
	}
}
