using System;
using System.Runtime.InteropServices;

namespace OniAccess.Speech {
	/// <summary>
	/// The Objective-C runtime calls the macOS speech code shares
	/// (<see cref="MacSpokenContent"/>, <see cref="MacSystemVoiceBackend"/>,
	/// <see cref="MacSpeechStream"/>): class and selector lookup, symbol lookup,
	/// the objc_msgSend shapes in use, and NSString conversion. Results are
	/// pointers, BOOLs, or one double, all of which plain objc_msgSend returns
	/// correctly on x86_64 (the game under Rosetta) and arm64 alike;
	/// objc_msgSend_fpret only matters for long double, and no struct is
	/// returned anywhere. Only called on macOS. Callers that create autoreleased
	/// objects push a pool of their own around the work rather than trusting
	/// the Unity main thread's pool to drain each frame.
	/// </summary>
	public static class ObjC {
		const string Lib = "/usr/lib/libobjc.A.dylib";
		const int RTLD_NOW = 2;

		[DllImport("/usr/lib/libSystem.B.dylib")]
		private static extern IntPtr dlopen(string path, int mode);

		[DllImport("/usr/lib/libSystem.B.dylib")]
		private static extern IntPtr dlsym(IntPtr handle, string symbol);

		private static readonly IntPtr RTLD_DEFAULT = new IntPtr(-2);

		/// <summary>The address of a C symbol in any loaded image, such as a block class; Zero if none.</summary>
		public static IntPtr Symbol(string name) => dlsym(RTLD_DEFAULT, name);

		[DllImport(Lib, EntryPoint = "objc_getClass")]
		public static extern IntPtr Class(string name);

		[DllImport(Lib, EntryPoint = "sel_registerName")]
		public static extern IntPtr Sel(string name);

		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		public static extern IntPtr Send(IntPtr receiver, IntPtr selector);

		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		public static extern IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr arg);

		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		public static extern IntPtr Send(IntPtr receiver, IntPtr selector, byte[] utf8);

		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		public static extern IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		public static extern IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, IntPtr arg3);

		/// <summary>
		/// A message taking one float, such as the setter of a float property.
		/// Named apart from <see cref="Send(IntPtr, IntPtr, IntPtr)"/> because an
		/// int literal converts implicitly to both float and IntPtr. The value
		/// travels in a floating-point register, which objc_msgSend leaves alone.
		/// </summary>
		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		public static extern IntPtr SendFloat(IntPtr receiver, IntPtr selector, float arg);

		/// <summary>
		/// A message taking a double then an unsigned int, such as
		/// initStandardFormatWithSampleRate:channels:. The double rides a
		/// floating-point register and the int an integer one, so their order in
		/// the selector does not affect placement.
		/// </summary>
		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		public static extern IntPtr SendDoubleUInt(IntPtr receiver, IntPtr selector, double arg1, uint arg2);

		/// <summary>A message returning a double, such as AVAudioFormat.sampleRate.</summary>
		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		public static extern double SendReturningDouble(IntPtr receiver, IntPtr selector);

		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SendBool(IntPtr receiver, IntPtr selector);

		[DllImport(Lib, EntryPoint = "objc_msgSend")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SendBool(IntPtr receiver, IntPtr selector, IntPtr arg);

		[DllImport(Lib, EntryPoint = "objc_autoreleasePoolPush")]
		public static extern IntPtr AutoreleasePoolPush();

		[DllImport(Lib, EntryPoint = "objc_autoreleasePoolPop")]
		public static extern void AutoreleasePoolPop(IntPtr pool);

		private static readonly IntPtr SelRetain = Sel("retain");
		private static readonly IntPtr SelRelease = Sel("release");
		private static readonly IntPtr SelCount = Sel("count");
		private static readonly IntPtr SelIsKindOfClass = Sel("isKindOfClass:");
		private static readonly IntPtr SelStringWithUtf8 = Sel("stringWithUTF8String:");
		private static readonly IntPtr SelUtf8String = Sel("UTF8String");
		private static readonly IntPtr SelDescription = Sel("description");
		private static readonly IntPtr ClassNSString = Class("NSString");

		/// <summary>Take a +1 reference on <paramref name="obj"/> (nil is left alone) and hand it back.</summary>
		public static IntPtr Retain(IntPtr obj) {
			if (obj != IntPtr.Zero) Send(obj, SelRetain);
			return obj;
		}

		/// <summary>Give up a +1 reference; nil is left alone.</summary>
		public static void Release(IntPtr obj) {
			if (obj != IntPtr.Zero) Send(obj, SelRelease);
		}

		public static bool IsKindOfClass(IntPtr obj, IntPtr cls) => SendBool(obj, SelIsKindOfClass, cls);

		/// <summary>The count of an NSArray or other collection (nil counts as 0). NSUInteger arrives whole in the integer return register.</summary>
		public static long Count(IntPtr collection) => (long)Send(collection, SelCount);

		/// <summary>An autoreleased NSString holding <paramref name="text"/>, or nil for null.</summary>
		public static IntPtr NSString(string text) =>
			text == null ? IntPtr.Zero : Send(ClassNSString, SelStringWithUtf8, PrismBackend.ToUtf8(text));

		/// <summary>The UTF-8 contents of an NSString, or of any other object's description; null for nil.</summary>
		public static string ToManagedString(IntPtr obj) {
			if (obj == IntPtr.Zero) return null;
			if (!IsKindOfClass(obj, ClassNSString))
				obj = Send(obj, SelDescription);
			return PrismBackend.PtrToUtf8(Send(obj, SelUtf8String));
		}

		private static bool _loaded;

		/// <summary>Make the speech classes resolvable: the game links AppKit already; AVFoundation may not be resident yet.</summary>
		public static void LoadSpeechFrameworks() {
			if (_loaded) return;
			dlopen("/System/Library/Frameworks/AppKit.framework/AppKit", RTLD_NOW);
			dlopen("/System/Library/Frameworks/AVFoundation.framework/AVFoundation", RTLD_NOW);
			_loaded = true;
		}
	}
}
