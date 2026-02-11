namespace OniAccess.Util
{
    public static class Log
    {
        private const string Prefix = "[OniAccess]";

        public static void Debug(string msg)
        {
            UnityEngine.Debug.Log($"{Prefix} [DEBUG] {msg}");
        }

        public static void Info(string msg)
        {
            UnityEngine.Debug.Log($"{Prefix} {msg}");
        }

        public static void Warn(string msg)
        {
            UnityEngine.Debug.LogWarning($"{Prefix} {msg}");
        }

        public static void Error(string msg)
        {
            UnityEngine.Debug.LogError($"{Prefix} {msg}");
        }
    }
}
