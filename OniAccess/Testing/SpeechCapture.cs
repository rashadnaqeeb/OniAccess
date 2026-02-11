using System.Collections.Generic;
using System.Linq;
using OniAccess.Speech;

namespace OniAccess.Testing
{
    /// <summary>
    /// Speech capture framework for automated testing.
    /// Hooks into SpeechEngine.OnSpeechOutput callback to record all speech output
    /// during a capture session, then provides assertion helpers for test verification.
    ///
    /// Per META-02: "Speech capture testing framework for automated regression testing."
    ///
    /// Usage:
    ///   SpeechCapture.Start();
    ///   // ... trigger actions that produce speech ...
    ///   var captured = SpeechCapture.Stop();
    ///   // Assert against captured list
    ///
    /// The capture buffer records the final text that would be sent to Tolk
    /// (after TextFilter processing, since OnSpeechOutput fires in SpeechEngine.Say).
    /// </summary>
    public static class SpeechCapture
    {
        private static bool _capturing;
        private static readonly List<string> _captured = new List<string>();

        /// <summary>
        /// Start capturing speech output. Clears any previous captures.
        /// Hooks into SpeechEngine.OnSpeechOutput callback.
        /// </summary>
        public static void Start()
        {
            _captured.Clear();
            _capturing = true;
            SpeechEngine.OnSpeechOutput += OnCapture;
        }

        /// <summary>
        /// Stop capturing and return a copy of all captured speech text.
        /// Unhooks from SpeechEngine.OnSpeechOutput.
        /// </summary>
        /// <returns>Copy of the captured speech strings in order.</returns>
        public static List<string> Stop()
        {
            _capturing = false;
            SpeechEngine.OnSpeechOutput -= OnCapture;
            return new List<string>(_captured);
        }

        /// <summary>
        /// Callback that records speech output from SpeechEngine.
        /// </summary>
        private static void OnCapture(string text)
        {
            if (_capturing)
                _captured.Add(text);
        }

        /// <summary>
        /// Check if any captured speech contains the given substring.
        /// </summary>
        public static bool ContainsText(string substring)
        {
            return _captured.Any(s => s.Contains(substring));
        }

        /// <summary>
        /// Check if the last captured speech exactly matches the expected text.
        /// </summary>
        public static bool LastSpeechWas(string expected)
        {
            return _captured.Count > 0 && _captured[_captured.Count - 1] == expected;
        }

        /// <summary>
        /// The number of speech outputs captured so far.
        /// </summary>
        public static int Count => _captured.Count;

        /// <summary>
        /// The most recent captured speech text, or null if nothing captured.
        /// </summary>
        public static string LastSpeech => _captured.Count > 0 ? _captured[_captured.Count - 1] : null;
    }
}
