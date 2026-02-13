using System;

namespace OniAccess.Util {
	/// <summary>
	/// Logging with swappable backend. Defaults to Console so tests can run
	/// without Unity. Mod.OnLoad() installs the Unity backend for in-game use.
	/// </summary>
	public static class Log {
		private const string Prefix = "[OniAccess]";

		internal static Action<string> LogFn = msg => Console.WriteLine(msg);
		internal static Action<string> WarnFn = msg => Console.WriteLine(msg);
		internal static Action<string> ErrorFn = msg => Console.Error.WriteLine(msg);

		public static void Debug(string msg) => LogFn($"{Prefix} [DEBUG] {msg}");
		public static void Info(string msg) => LogFn($"{Prefix} {msg}");
		public static void Warn(string msg) => WarnFn($"{Prefix} {msg}");
		public static void Error(string msg) => ErrorFn($"{Prefix} {msg}");
	}
}
