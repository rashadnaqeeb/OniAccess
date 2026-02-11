using System.Collections.Generic;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Testing
{
    /// <summary>
    /// Tests for the input dispatch system: handler lifecycle, key routing,
    /// and game passthrough. Calls HandlerStack.DispatchUnboundKey -- the same
    /// method KeyPoller.Update() calls -- so these test real production code.
    /// </summary>
    public static class InputSystemTests
    {
        /// <summary>
        /// Configurable test handler. Tracks which keys it received
        /// and whether to consume them.
        /// </summary>
        private class TestHandler : IAccessHandler
        {
            public string DisplayName { get; }
            public bool CapturesAllInput { get; }
            public IReadOnlyList<HelpEntry> HelpEntries { get; }
                = new List<HelpEntry>().AsReadOnly();

            public List<UnityEngine.KeyCode> ReceivedKeys { get; }
                = new List<UnityEngine.KeyCode>();
            public bool ConsumeUnboundKeys { get; set; }
            public int ActivateCount { get; private set; }

            public TestHandler(string name, bool capturesAll = false)
            {
                DisplayName = name;
                CapturesAllInput = capturesAll;
            }

            public bool HandleKeyDown(KButtonEvent e) => false;
            public bool HandleKeyUp(KButtonEvent e) => false;

            public bool HandleUnboundKey(UnityEngine.KeyCode keyCode)
            {
                ReceivedKeys.Add(keyCode);
                return ConsumeUnboundKeys;
            }

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

            // Key dispatch ordering
            results.Add(TopHandlerGetsKeyFirst());
            results.Add(ConsumedKeyStopsWalk());
            results.Add(KeyFallsThroughWhenNotCaptured());
            results.Add(CapturesAllInputBlocksFallthrough());

            // Passthrough to game
            results.Add(UnconsumedKeyReachesGame());
            results.Add(EmptyStackPassesThrough());

            // Real handler integration
            results.Add(WorldHandlerF12PushesHelp());
            results.Add(HelpHandlerBlocksKeysFromWorld());

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

        // --- Key dispatch ordering ---

        private static (string, bool, string) TopHandlerGetsKeyFirst()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var top = new TestHandler("Top") { ConsumeUnboundKeys = true };
            HandlerStack.Push(bottom);
            HandlerStack.Push(top);

            HandlerStack.DispatchUnboundKey(UnityEngine.KeyCode.LeftArrow);

            bool ok = top.ReceivedKeys.Count == 1 && bottom.ReceivedKeys.Count == 0;
            return AssertTrue("TopHandlerGetsKeyFirst", ok,
                ok ? "top consumed, bottom never saw key"
                   : $"top={top.ReceivedKeys.Count}, bottom={bottom.ReceivedKeys.Count}");
        }

        private static (string, bool, string) ConsumedKeyStopsWalk()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var middle = new TestHandler("Middle") { ConsumeUnboundKeys = true };
            var top = new TestHandler("Top");
            HandlerStack.Push(bottom);
            HandlerStack.Push(middle);
            HandlerStack.Push(top);

            HandlerStack.DispatchUnboundKey(UnityEngine.KeyCode.DownArrow);

            // top doesn't consume -> falls to middle -> middle consumes -> bottom never sees it
            bool ok = top.ReceivedKeys.Count == 1
                   && middle.ReceivedKeys.Count == 1
                   && bottom.ReceivedKeys.Count == 0;
            return AssertTrue("ConsumedKeyStopsWalk", ok,
                ok ? "walk stopped at consuming handler"
                   : $"top={top.ReceivedKeys.Count}, mid={middle.ReceivedKeys.Count}, bot={bottom.ReceivedKeys.Count}");
        }

        private static (string, bool, string) KeyFallsThroughWhenNotCaptured()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var top = new TestHandler("Top"); // CapturesAllInput=false, doesn't consume
            HandlerStack.Push(bottom);
            HandlerStack.Push(top);

            HandlerStack.DispatchUnboundKey(UnityEngine.KeyCode.LeftArrow);

            bool ok = top.ReceivedKeys.Count == 1 && bottom.ReceivedKeys.Count == 1;
            return AssertTrue("KeyFallsThroughWhenNotCaptured", ok,
                ok ? "both handlers saw the key"
                   : $"top={top.ReceivedKeys.Count}, bottom={bottom.ReceivedKeys.Count}");
        }

        private static (string, bool, string) CapturesAllInputBlocksFallthrough()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            // CapturesAllInput=true, but doesn't consume -- should still block
            var top = new TestHandler("Top", capturesAll: true);
            HandlerStack.Push(bottom);
            HandlerStack.Push(top);

            HandlerStack.DispatchUnboundKey(UnityEngine.KeyCode.LeftArrow);

            bool ok = top.ReceivedKeys.Count == 1 && bottom.ReceivedKeys.Count == 0;
            return AssertTrue("CapturesAllInputBlocksFallthrough", ok,
                ok ? "CapturesAllInput blocked fallthrough to bottom"
                   : $"top={top.ReceivedKeys.Count}, bottom={bottom.ReceivedKeys.Count}");
        }

        // --- Passthrough to game ---

        private static (string, bool, string) UnconsumedKeyReachesGame()
        {
            Reset();
            // CapturesAllInput=false, doesn't consume -- key should pass through
            var handler = new TestHandler("World");
            HandlerStack.Push(handler);

            bool consumed = HandlerStack.DispatchUnboundKey(UnityEngine.KeyCode.LeftArrow);

            bool ok = !consumed && handler.ReceivedKeys.Count == 1;
            return AssertTrue("UnconsumedKeyReachesGame", ok,
                ok ? "key passed through to game"
                   : $"consumed={consumed}, handler saw {handler.ReceivedKeys.Count} keys");
        }

        private static (string, bool, string) EmptyStackPassesThrough()
        {
            Reset();
            bool consumed = HandlerStack.DispatchUnboundKey(UnityEngine.KeyCode.F12);

            bool ok = !consumed;
            return AssertTrue("EmptyStackPassesThrough", ok,
                ok ? "empty stack passes through to game"
                   : "key was consumed with no handlers");
        }

        // --- Real handler integration ---

        private static (string, bool, string) WorldHandlerF12PushesHelp()
        {
            Reset();
            HandlerStack.Push(new WorldHandler());
            int before = HandlerStack.Count;

            HandlerStack.DispatchUnboundKey(UnityEngine.KeyCode.F12);

            bool ok = HandlerStack.Count == before + 1
                   && HandlerStack.ActiveHandler is HelpHandler;
            return AssertTrue("WorldHandlerF12PushesHelp", ok,
                ok ? "HelpHandler pushed onto stack"
                   : $"count {before}->{HandlerStack.Count}, active={HandlerStack.ActiveHandler?.GetType().Name ?? "null"}");
        }

        private static (string, bool, string) HelpHandlerBlocksKeysFromWorld()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            HandlerStack.Push(bottom);

            // HelpHandler has CapturesAllInput=true
            var help = new HelpHandler(new List<HelpEntry>
            {
                new HelpEntry("Test", "test entry")
            }.AsReadOnly());
            HandlerStack.Push(help);

            HandlerStack.DispatchUnboundKey(UnityEngine.KeyCode.LeftArrow);

            bool ok = bottom.ReceivedKeys.Count == 0;
            return AssertTrue("HelpHandlerBlocksKeysFromWorld", ok,
                ok ? "HelpHandler blocked key from reaching bottom"
                   : $"bottom saw {bottom.ReceivedKeys.Count} keys");
        }
    }
}
