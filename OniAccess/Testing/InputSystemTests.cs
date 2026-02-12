using System.Collections.Generic;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Testing
{
    /// <summary>
    /// Tests for the input system: handler lifecycle, stack contracts,
    /// and gate behavior. Tests stack operations and handler properties
    /// using real production code.
    /// </summary>
    public static class InputSystemTests
    {
        /// <summary>
        /// Configurable test handler. Tracks activation count and
        /// implements the new unified interface.
        /// </summary>
        private class TestHandler : IAccessHandler
        {
            public string DisplayName { get; }
            public bool CapturesAllInput { get; }
            public IReadOnlyList<HelpEntry> HelpEntries { get; }
                = new List<HelpEntry>().AsReadOnly();

            public int ActivateCount { get; private set; }

            public TestHandler(string name, bool capturesAll = false)
            {
                DisplayName = name;
                CapturesAllInput = capturesAll;
            }

            public void Tick() { }
            public bool HandleKeyDown(KButtonEvent e) => false;
            public void OnActivate() => ActivateCount++;
            public void OnDeactivate() { }
        }

        public static List<(string name, bool passed, string detail)> RunAll()
        {
            // Save speech pipeline state so we can restore after tests
            bool speechWasActive = SpeechPipeline.IsActive;
            SpeechPipeline.SetEnabled(true);

            var results = new List<(string name, bool passed, string detail)>();

            // Handler lifecycle + speech
            results.Add(PushSpeaksDisplayName());
            results.Add(PopReannouncesExposedHandler());

            // Stack contract tests
            results.Add(ActiveHandlerIsTop());
            results.Add(CapturesAllInputReadable());
            results.Add(PopExposesLowerHandler());

            // Restore state: clear test handlers
            HandlerStack.Clear();
            SpeechPipeline.SetEnabled(speechWasActive);

            return results;
        }

        private static (string, bool, string) AssertTrue(
            string testName, bool condition, string detail)
        {
            return (testName, condition,
                condition ? $"OK: {detail}" : $"FAIL: {detail}");
        }

        private static void Reset()
        {
            HandlerStack.Clear();
        }

        // --- Handler lifecycle + speech ---

        private static (string, bool, string) PushSpeaksDisplayName()
        {
            Reset();
            if (!SpeechEngine.IsAvailable)
                return ("PushSpeaksDisplayName", true, "SKIP: no speech engine");

            SpeechCapture.Start();
            HandlerStack.Push(new WorldHandler());
            var captured = SpeechCapture.Stop();

            bool ok = captured.Count > 0
                   && captured[0] == STRINGS.ONIACCESS.HANDLERS.WORLD_VIEW;
            return AssertTrue("PushSpeaksDisplayName", ok,
                ok ? $"spoke \"{captured[0]}\""
                   : $"expected \"{STRINGS.ONIACCESS.HANDLERS.WORLD_VIEW}\", got {(captured.Count > 0 ? $"\"{captured[0]}\"" : "nothing")}");
        }

        private static (string, bool, string) PopReannouncesExposedHandler()
        {
            Reset();
            if (!SpeechEngine.IsAvailable)
                return ("PopReannouncesExposedHandler", true, "SKIP: no speech engine");

            HandlerStack.Push(new WorldHandler());
            var modal = new TestHandler("Modal", capturesAll: true);
            HandlerStack.Push(modal);

            SpeechCapture.Start();
            HandlerStack.Pop(); // exposes WorldHandler, should re-speak
            var captured = SpeechCapture.Stop();

            bool ok = captured.Count > 0
                   && captured[0] == STRINGS.ONIACCESS.HANDLERS.WORLD_VIEW;
            return AssertTrue("PopReannouncesExposedHandler", ok,
                ok ? $"re-spoke \"{captured[0]}\""
                   : $"expected \"{STRINGS.ONIACCESS.HANDLERS.WORLD_VIEW}\", got {(captured.Count > 0 ? $"\"{captured[0]}\"" : "nothing")}");
        }

        // --- Stack contract tests ---

        private static (string, bool, string) ActiveHandlerIsTop()
        {
            Reset();
            var first = new TestHandler("First");
            var second = new TestHandler("Second");
            HandlerStack.Push(first);
            HandlerStack.Push(second);

            bool ok = HandlerStack.ActiveHandler == second;
            return AssertTrue("ActiveHandlerIsTop", ok,
                ok ? "ActiveHandler is the second (top) handler"
                   : $"ActiveHandler is {HandlerStack.ActiveHandler?.DisplayName ?? "null"}");
        }

        private static (string, bool, string) CapturesAllInputReadable()
        {
            Reset();
            var handler = new TestHandler("Modal", capturesAll: true);
            HandlerStack.Push(handler);

            bool ok = HandlerStack.ActiveHandler.CapturesAllInput;
            return AssertTrue("CapturesAllInputReadable", ok,
                ok ? "CapturesAllInput=true is readable from ActiveHandler"
                   : "CapturesAllInput was false");
        }

        private static (string, bool, string) PopExposesLowerHandler()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var top = new TestHandler("Top");
            HandlerStack.Push(bottom);
            HandlerStack.Push(top);

            HandlerStack.Pop();

            bool ok = HandlerStack.ActiveHandler == bottom
                   && HandlerStack.Count == 1;
            return AssertTrue("PopExposesLowerHandler", ok,
                ok ? "Pop exposed bottom handler"
                   : $"ActiveHandler={HandlerStack.ActiveHandler?.DisplayName ?? "null"}, count={HandlerStack.Count}");
        }
    }
}
