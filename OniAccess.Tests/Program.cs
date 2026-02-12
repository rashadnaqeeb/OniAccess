using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            // Resolve game assemblies at runtime from ONI_MANAGED
            var managed = Environment.GetEnvironmentVariable("ONI_MANAGED")
                ?? @"C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed";

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                var name = new AssemblyName(e.Name).Name;
                var path = Path.Combine(managed, name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };

            return RunTests();
        }

        static int RunTests()
        {
            var results = new List<(string name, bool passed, string detail)>();

            // --- HandlerStack (9 existing + 4 new edge cases) ---
            results.Add(ActiveHandlerIsTop());
            results.Add(CapturesAllInputReadable());
            results.Add(PopExposesLowerHandler());
            results.Add(PushCallsOnActivate());
            results.Add(PopCallsOnDeactivate());
            results.Add(PopReactivatesExposedHandler());
            results.Add(ReplaceSwapsHandlers());
            results.Add(ClearEmptiesStack());
            results.Add(DeactivateAllCallsOnDeactivate());
            results.Add(ReplaceOnEmptyStack());
            results.Add(PopOnEmptyStack());
            results.Add(PushNullIgnored());
            results.Add(ReplaceNullIgnored());
            results.Add(RapidPushPopSequence());
            results.Add(ExceptionInOnActivateDoesNotCorruptStack());
            results.Add(ExceptionInOnDeactivateDoesNotCorruptStack());

            HandlerStack.Clear();

            // --- TypeAheadSearch (12 + 1 new) ---
            results.Add(SearchWordStartMatch());
            results.Add(SearchMultiWordMatch());
            results.Add(SearchCaseInsensitive());
            results.Add(SearchNullLabelsSkipped());
            results.Add(SearchMultiCharNarrowing());
            results.Add(SearchRepeatLetterCycles());
            results.Add(SearchBackspace());
            results.Add(SearchClearResetsState());
            results.Add(SearchNavigateWraps());
            results.Add(SearchJumpFirstLast());
            results.Add(SearchNoMatch());
            results.Add(SearchEmptyAfterClear());
            results.Add(SearchBufferTimeoutResets());

            // --- TextFilter (12 ported + 3 new edge cases) ---
            TextFilter.RegisterSprite("warning", "warning:");
            TextFilter.RegisterSprite("logic_signal_green", "green signal");
            TextFilter.RegisterSprite("logic_signal_red", "red signal");

            results.Add(TextFilterStripsBold());
            results.Add(TextFilterStripsColor());
            results.Add(TextFilterStripsNested());
            results.Add(TextFilterConvertsSpriteToText());
            results.Add(TextFilterStripsUnregisteredSprite());
            results.Add(TextFilterStripsTmpBracketSprites());
            results.Add(TextFilterHandlesLinkTags());
            results.Add(TextFilterNormalizesWhitespace());
            results.Add(TextFilterHandlesEmptyAndNull());
            results.Add(TextFilterPreservesPlainText());
            results.Add(TextFilterStripsHotkey());
            results.Add(TextFilterStripsSizeStyle());
            results.Add(TextFilterUnclosedTag());
            results.Add(TextFilterMismatchedTags());
            results.Add(TextFilterSpriteNameCaseInsensitive());

            int passed = 0, failed = 0;
            foreach (var (name, ok, detail) in results)
            {
                if (ok)
                {
                    Console.WriteLine($"  PASS  {name}");
                    passed++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  FAIL  {name}: {detail}");
                    Console.ResetColor();
                    failed++;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"{passed} passed, {failed} failed, {results.Count} total");

            return failed > 0 ? 1 : 0;
        }

        // --- Test handler ---

        private class TestHandler : IAccessHandler
        {
            public string DisplayName { get; }
            public bool CapturesAllInput { get; }
            public IReadOnlyList<HelpEntry> HelpEntries { get; }
                = new List<HelpEntry>().AsReadOnly();

            public int ActivateCount { get; private set; }
            public int DeactivateCount { get; private set; }

            public TestHandler(string name, bool capturesAll = false)
            {
                DisplayName = name;
                CapturesAllInput = capturesAll;
            }

            public void Tick() { }
            public bool HandleKeyDown(KButtonEvent e) => false;
            public void OnActivate() => ActivateCount++;
            public void OnDeactivate() => DeactivateCount++;
        }

        private class ThrowingHandler : IAccessHandler
        {
            public string DisplayName => "Thrower";
            public bool CapturesAllInput => false;
            public IReadOnlyList<HelpEntry> HelpEntries { get; }
                = new List<HelpEntry>().AsReadOnly();

            public bool ThrowOnActivate { get; set; }
            public bool ThrowOnDeactivate { get; set; }

            public void Tick() { }
            public bool HandleKeyDown(KButtonEvent e) => false;

            public void OnActivate()
            {
                if (ThrowOnActivate)
                    throw new InvalidOperationException("OnActivate exploded");
            }

            public void OnDeactivate()
            {
                if (ThrowOnDeactivate)
                    throw new InvalidOperationException("OnDeactivate exploded");
            }
        }

        // --- Helpers ---

        private static void Reset() => HandlerStack.Clear();

        private static (string, bool, string) Assert(string name, bool ok, string detail)
            => (name, ok, ok ? "OK" : detail);

        // --- Mock data for TypeAheadSearch ---

        private static readonly string[] SearchItems =
            { "Apple", "Apricot", "Banana", "Blue Cheese", "Cherry", null, "" };

        private static string NameByIndex(int i) =>
            i >= 0 && i < SearchItems.Length ? SearchItems[i] : null;

        // ========================================
        // HandlerStack tests (existing)
        // ========================================

        private static (string, bool, string) ActiveHandlerIsTop()
        {
            Reset();
            var first = new TestHandler("First");
            var second = new TestHandler("Second");
            HandlerStack.Push(first);
            HandlerStack.Push(second);

            bool ok = HandlerStack.ActiveHandler == second;
            return Assert("ActiveHandlerIsTop", ok,
                $"expected Second, got {HandlerStack.ActiveHandler?.DisplayName ?? "null"}");
        }

        private static (string, bool, string) CapturesAllInputReadable()
        {
            Reset();
            var handler = new TestHandler("Modal", capturesAll: true);
            HandlerStack.Push(handler);

            bool ok = HandlerStack.ActiveHandler.CapturesAllInput;
            return Assert("CapturesAllInputReadable", ok, "CapturesAllInput was false");
        }

        private static (string, bool, string) PopExposesLowerHandler()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var top = new TestHandler("Top");
            HandlerStack.Push(bottom);
            HandlerStack.Push(top);
            HandlerStack.Pop();

            bool ok = HandlerStack.ActiveHandler == bottom && HandlerStack.Count == 1;
            return Assert("PopExposesLowerHandler", ok,
                $"ActiveHandler={HandlerStack.ActiveHandler?.DisplayName ?? "null"}, count={HandlerStack.Count}");
        }

        private static (string, bool, string) PushCallsOnActivate()
        {
            Reset();
            var handler = new TestHandler("Test");
            HandlerStack.Push(handler);

            bool ok = handler.ActivateCount == 1;
            return Assert("PushCallsOnActivate", ok,
                $"ActivateCount={handler.ActivateCount}");
        }

        private static (string, bool, string) PopCallsOnDeactivate()
        {
            Reset();
            var handler = new TestHandler("Test");
            HandlerStack.Push(handler);
            HandlerStack.Pop();

            bool ok = handler.DeactivateCount == 1;
            return Assert("PopCallsOnDeactivate", ok,
                $"DeactivateCount={handler.DeactivateCount}");
        }

        private static (string, bool, string) PopReactivatesExposedHandler()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var top = new TestHandler("Top");
            HandlerStack.Push(bottom); // ActivateCount = 1
            HandlerStack.Push(top);
            HandlerStack.Pop(); // should reactivate bottom

            bool ok = bottom.ActivateCount == 2;
            return Assert("PopReactivatesExposedHandler", ok,
                $"ActivateCount={bottom.ActivateCount}, expected 2");
        }

        private static (string, bool, string) ReplaceSwapsHandlers()
        {
            Reset();
            var first = new TestHandler("First");
            var second = new TestHandler("Second");
            HandlerStack.Push(first);
            HandlerStack.Replace(second);

            bool ok = HandlerStack.ActiveHandler == second
                   && HandlerStack.Count == 1
                   && first.DeactivateCount == 1
                   && second.ActivateCount == 1;
            return Assert("ReplaceSwapsHandlers", ok,
                $"Active={HandlerStack.ActiveHandler?.DisplayName ?? "null"}, " +
                $"count={HandlerStack.Count}, " +
                $"first.Deactivate={first.DeactivateCount}, " +
                $"second.Activate={second.ActivateCount}");
        }

        private static (string, bool, string) ClearEmptiesStack()
        {
            Reset();
            HandlerStack.Push(new TestHandler("A"));
            HandlerStack.Push(new TestHandler("B"));
            HandlerStack.Clear();

            bool ok = HandlerStack.Count == 0 && HandlerStack.ActiveHandler == null;
            return Assert("ClearEmptiesStack", ok,
                $"count={HandlerStack.Count}");
        }

        private static (string, bool, string) DeactivateAllCallsOnDeactivate()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var top = new TestHandler("Top");
            HandlerStack.Push(bottom);
            HandlerStack.Push(top);
            HandlerStack.DeactivateAll();

            // Only the active (top) handler gets OnDeactivate
            bool ok = top.DeactivateCount == 1
                   && bottom.DeactivateCount == 0
                   && HandlerStack.Count == 0;
            return Assert("DeactivateAllCallsOnDeactivate", ok,
                $"top.Deactivate={top.DeactivateCount}, " +
                $"bottom.Deactivate={bottom.DeactivateCount}, " +
                $"count={HandlerStack.Count}");
        }

        // ========================================
        // HandlerStack edge cases (new)
        // ========================================

        private static (string, bool, string) ReplaceOnEmptyStack()
        {
            Reset();
            var handler = new TestHandler("Replacement");
            HandlerStack.Replace(handler);

            bool ok = HandlerStack.Count == 1 && handler.ActivateCount == 1;
            return Assert("ReplaceOnEmptyStack", ok,
                $"count={HandlerStack.Count}, ActivateCount={handler.ActivateCount}");
        }

        private static (string, bool, string) PopOnEmptyStack()
        {
            Reset();
            try
            {
                HandlerStack.Pop();
            }
            catch (Exception ex)
            {
                return Assert("PopOnEmptyStack", false, $"threw {ex.GetType().Name}");
            }

            bool ok = HandlerStack.Count == 0;
            return Assert("PopOnEmptyStack", ok, $"count={HandlerStack.Count}");
        }

        private static (string, bool, string) PushNullIgnored()
        {
            Reset();
            HandlerStack.Push(new TestHandler("Base"));
            int before = HandlerStack.Count;

            try
            {
                HandlerStack.Push(null);
            }
            catch (Exception ex)
            {
                return Assert("PushNullIgnored", false, $"threw {ex.GetType().Name}");
            }

            bool ok = HandlerStack.Count == before;
            return Assert("PushNullIgnored", ok,
                $"count changed from {before} to {HandlerStack.Count}");
        }

        private static (string, bool, string) ReplaceNullIgnored()
        {
            Reset();
            var existing = new TestHandler("Existing");
            HandlerStack.Push(existing);
            int before = HandlerStack.Count;

            try
            {
                HandlerStack.Replace(null);
            }
            catch (Exception ex)
            {
                return Assert("ReplaceNullIgnored", false, $"threw {ex.GetType().Name}");
            }

            bool ok = HandlerStack.Count == before
                   && HandlerStack.ActiveHandler == existing;
            return Assert("ReplaceNullIgnored", ok,
                $"count={HandlerStack.Count}, active={HandlerStack.ActiveHandler?.DisplayName ?? "null"}");
        }

        private static (string, bool, string) RapidPushPopSequence()
        {
            Reset();
            var a = new TestHandler("A");
            var b = new TestHandler("B");
            var c = new TestHandler("C");

            HandlerStack.Push(a);
            HandlerStack.Push(b);
            HandlerStack.Pop();
            HandlerStack.Push(c);
            HandlerStack.Pop();
            HandlerStack.Pop();

            // a: activated on push(1), reactivated after pop(b)(2), reactivated after pop(c)(3)
            // a: deactivated on final pop(1)
            bool ok = HandlerStack.Count == 0
                   && a.ActivateCount == 3
                   && a.DeactivateCount == 1
                   && b.DeactivateCount == 1
                   && c.DeactivateCount == 1;
            return Assert("RapidPushPopSequence", ok,
                $"count={HandlerStack.Count}, a.Act={a.ActivateCount}, a.Deact={a.DeactivateCount}, " +
                $"b.Deact={b.DeactivateCount}, c.Deact={c.DeactivateCount}");
        }

        private static (string, bool, string) ExceptionInOnActivateDoesNotCorruptStack()
        {
            Reset();
            var good = new TestHandler("Good");
            HandlerStack.Push(good);

            var thrower = new ThrowingHandler { ThrowOnActivate = true };
            bool threw = false;
            try
            {
                HandlerStack.Push(thrower);
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }

            // Push adds to stack before calling OnActivate, so thrower is on the stack
            // even though OnActivate threw. Verify the stack isn't in a broken state:
            // the thrower was added (count=2) and the exception propagated.
            bool ok = threw && HandlerStack.Count == 2;
            return Assert("ExceptionInOnActivateDoesNotCorruptStack", ok,
                $"threw={threw}, count={HandlerStack.Count}");
        }

        private static (string, bool, string) ExceptionInOnDeactivateDoesNotCorruptStack()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var thrower = new ThrowingHandler { ThrowOnDeactivate = true };
            HandlerStack.Push(bottom);
            HandlerStack.Push(thrower);

            bool threw = false;
            try
            {
                HandlerStack.Pop();
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }

            // Pop removes from stack before calling OnDeactivate, so thrower is gone
            // even though OnDeactivate threw. Bottom should be reactivated... but the
            // exception fires before reactivation. Verify removal happened.
            bool ok = threw && HandlerStack.Count == 1
                   && HandlerStack.ActiveHandler == bottom;
            return Assert("ExceptionInOnDeactivateDoesNotCorruptStack", ok,
                $"threw={threw}, count={HandlerStack.Count}, " +
                $"active={HandlerStack.ActiveHandler?.DisplayName ?? "null"}");
        }

        // ========================================
        // TypeAheadSearch tests (new)
        // ========================================

        private static (string, bool, string) SearchWordStartMatch()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('a');
            search.Search(SearchItems.Length, NameByIndex);

            bool ok = search.ResultCount == 2 && search.SelectedOriginalIndex == 0;
            return Assert("SearchWordStartMatch", ok,
                $"ResultCount={search.ResultCount}, SelectedOriginalIndex={search.SelectedOriginalIndex}");
        }

        private static (string, bool, string) SearchMultiWordMatch()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('c');
            search.Search(SearchItems.Length, NameByIndex);

            // 'c' matches Blue Cheese(3) via "Cheese" word-start + Cherry(4)
            bool ok = search.ResultCount == 2;
            return Assert("SearchMultiWordMatch", ok,
                $"ResultCount={search.ResultCount}");
        }

        private static (string, bool, string) SearchCaseInsensitive()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('b');
            search.Search(SearchItems.Length, NameByIndex);

            // 'b' matches Banana(2), Blue Cheese(3)
            bool ok = search.ResultCount == 2;
            return Assert("SearchCaseInsensitive", ok,
                $"ResultCount={search.ResultCount}");
        }

        private static (string, bool, string) SearchNullLabelsSkipped()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('a');
            search.Search(SearchItems.Length, NameByIndex);

            // null at index 5 and "" at index 6 are skipped
            bool ok = search.ResultCount == 2 && search.SelectedOriginalIndex == 0;
            return Assert("SearchNullLabelsSkipped", ok,
                $"ResultCount={search.ResultCount}, SelectedOriginalIndex={search.SelectedOriginalIndex}");
        }

        private static (string, bool, string) SearchMultiCharNarrowing()
        {
            var search = new TypeAheadSearch(() => 0f);

            search.AddChar('a');
            search.Search(SearchItems.Length, NameByIndex);
            int afterA = search.ResultCount;

            search.AddChar('p');
            search.Search(SearchItems.Length, NameByIndex);
            int afterAp = search.ResultCount;

            search.AddChar('r');
            search.Search(SearchItems.Length, NameByIndex);
            int afterApr = search.ResultCount;

            bool ok = afterA == 2 && afterAp == 2 && afterApr == 1
                   && search.SelectedOriginalIndex == 1; // Apricot
            return Assert("SearchMultiCharNarrowing", ok,
                $"a={afterA}, ap={afterAp}, apr={afterApr}, idx={search.SelectedOriginalIndex}");
        }

        private static (string, bool, string) SearchRepeatLetterCycles()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('b');
            search.Search(SearchItems.Length, NameByIndex);
            int firstIdx = search.SelectedOriginalIndex; // Banana(2)

            search.AddChar('b'); // buffer="bb", triggers cycle
            search.Search(SearchItems.Length, NameByIndex);
            int secondIdx = search.SelectedOriginalIndex; // Blue Cheese(3)

            bool ok = firstIdx == 2 && secondIdx == 3;
            return Assert("SearchRepeatLetterCycles", ok,
                $"first={firstIdx}, second={secondIdx}");
        }

        private static (string, bool, string) SearchBackspace()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('a');
            search.AddChar('p');
            search.Search(SearchItems.Length, NameByIndex);
            int afterAp = search.ResultCount;

            search.RemoveChar(); // buffer="a"
            search.Search(SearchItems.Length, NameByIndex);
            int afterBackspace = search.ResultCount;

            bool ok = afterAp == 2 && afterBackspace == 2
                   && search.Buffer == "a";
            return Assert("SearchBackspace", ok,
                $"ap={afterAp}, after backspace={afterBackspace}, buffer=\"{search.Buffer}\"");
        }

        private static (string, bool, string) SearchClearResetsState()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('a');
            search.Search(SearchItems.Length, NameByIndex);
            search.Clear();

            bool ok = !search.IsSearchActive
                   && search.ResultCount == 0
                   && !search.HasBuffer;
            return Assert("SearchClearResetsState", ok,
                $"IsSearchActive={search.IsSearchActive}, ResultCount={search.ResultCount}, HasBuffer={search.HasBuffer}");
        }

        private static (string, bool, string) SearchNavigateWraps()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('a');
            search.Search(SearchItems.Length, NameByIndex);
            // 2 results: Apple(0), Apricot(1). Cursor at 0.

            search.NavigateResults(1); // cursor -> 1
            search.NavigateResults(1); // cursor -> 0 (wrap)

            bool ok = search.SelectedOriginalIndex == 0; // back to Apple
            return Assert("SearchNavigateWraps", ok,
                $"SelectedOriginalIndex={search.SelectedOriginalIndex}");
        }

        private static (string, bool, string) SearchJumpFirstLast()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('a');
            search.Search(SearchItems.Length, NameByIndex);
            // 2 results: Apple(0), Apricot(1)

            search.JumpToLastResult();
            int lastIdx = search.SelectedOriginalIndex; // Apricot(1)

            search.JumpToFirstResult();
            int firstIdx = search.SelectedOriginalIndex; // Apple(0)

            bool ok = lastIdx == 1 && firstIdx == 0;
            return Assert("SearchJumpFirstLast", ok,
                $"last={lastIdx}, first={firstIdx}");
        }

        private static (string, bool, string) SearchNoMatch()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('z');
            search.Search(SearchItems.Length, NameByIndex);

            bool ok = search.ResultCount == 0 && search.IsSearchActive;
            return Assert("SearchNoMatch", ok,
                $"ResultCount={search.ResultCount}, IsSearchActive={search.IsSearchActive}");
        }

        private static (string, bool, string) SearchEmptyAfterClear()
        {
            var search = new TypeAheadSearch(() => 0f);
            search.AddChar('b');
            search.Search(SearchItems.Length, NameByIndex);
            search.Clear();

            bool ok = !search.IsSearchActive
                   && search.ResultCount == 0
                   && !search.HasBuffer
                   && search.SelectedOriginalIndex == -1;
            return Assert("SearchEmptyAfterClear", ok,
                $"IsSearchActive={search.IsSearchActive}, ResultCount={search.ResultCount}, " +
                $"HasBuffer={search.HasBuffer}, SelectedOriginalIndex={search.SelectedOriginalIndex}");
        }

        private static (string, bool, string) SearchBufferTimeoutResets()
        {
            float fakeTime = 0f;
            var search = new TypeAheadSearch(() => fakeTime);

            search.AddChar('a');
            search.AddChar('p');
            // Buffer is "ap"

            // Advance past the 1.5s timeout
            fakeTime = 2f;
            search.AddChar('b');
            // Timeout elapsed, buffer should have been cleared before adding 'b'
            // So buffer is now just "b", not "apb"

            bool ok = search.Buffer == "b";
            return Assert("SearchBufferTimeoutResets", ok,
                $"buffer=\"{search.Buffer}\", expected \"b\"");
        }

        // ========================================
        // TextFilter tests (ported + new edge cases)
        // ========================================

        private static (string, bool, string) TextFilterStripsBold()
        {
            string result = TextFilter.FilterForSpeech("<b>Warning</b>");
            bool ok = result == "Warning";
            return Assert("TextFilterStripsBold", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterStripsColor()
        {
            string result = TextFilter.FilterForSpeech("<color=#FF0000>Hot</color>");
            bool ok = result == "Hot";
            return Assert("TextFilterStripsColor", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterStripsNested()
        {
            string result = TextFilter.FilterForSpeech("<b><color=red>Alert</color></b>");
            bool ok = result == "Alert";
            return Assert("TextFilterStripsNested", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterConvertsSpriteToText()
        {
            string result = TextFilter.FilterForSpeech("<sprite name=warning>Pipe broken");
            bool ok = result == "warning: Pipe broken";
            return Assert("TextFilterConvertsSpriteToText", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterStripsUnregisteredSprite()
        {
            string result = TextFilter.FilterForSpeech("<sprite name=decorative_icon>text");
            bool ok = result == "text";
            return Assert("TextFilterStripsUnregisteredSprite", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterStripsTmpBracketSprites()
        {
            string result = TextFilter.FilterForSpeech("[icon_name] some text");
            bool ok = result == "some text";
            return Assert("TextFilterStripsTmpBracketSprites", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterHandlesLinkTags()
        {
            string result = TextFilter.FilterForSpeech("<link=\"LINK_ID\">Click here</link>");
            bool ok = result == "Click here";
            return Assert("TextFilterHandlesLinkTags", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterNormalizesWhitespace()
        {
            string result = TextFilter.FilterForSpeech("word1   word2\n\nword3");
            bool ok = result == "word1 word2 word3";
            return Assert("TextFilterNormalizesWhitespace", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterHandlesEmptyAndNull()
        {
            string nullResult = TextFilter.FilterForSpeech(null);
            string emptyResult = TextFilter.FilterForSpeech("");

            bool ok = nullResult == "" && emptyResult == "";
            return Assert("TextFilterHandlesEmptyAndNull", ok,
                $"null->\"{nullResult}\", empty->\"{emptyResult}\"");
        }

        private static (string, bool, string) TextFilterPreservesPlainText()
        {
            string input = "Copper Ore, 200 kg, 25\u00B0C";
            string result = TextFilter.FilterForSpeech(input);
            bool ok = result == input;
            return Assert("TextFilterPreservesPlainText", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterStripsHotkey()
        {
            string result = TextFilter.FilterForSpeech("Build a Ladder {Hotkey}");
            bool ok = result == "Build a Ladder";
            return Assert("TextFilterStripsHotkey", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterStripsSizeStyle()
        {
            string result = TextFilter.FilterForSpeech("<size=10><style=\"KKeyword\">text</style></size>");
            bool ok = result == "text";
            return Assert("TextFilterStripsSizeStyle", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterUnclosedTag()
        {
            string result = TextFilter.FilterForSpeech("<b>Bold text");
            bool ok = result == "Bold text";
            return Assert("TextFilterUnclosedTag", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterMismatchedTags()
        {
            string result = TextFilter.FilterForSpeech("<b>text</color>");
            bool ok = result == "text";
            return Assert("TextFilterMismatchedTags", ok, $"got \"{result}\"");
        }

        private static (string, bool, string) TextFilterSpriteNameCaseInsensitive()
        {
            string result = TextFilter.FilterForSpeech("<sprite name=WARNING>text");
            bool ok = result == "warning: text";
            return Assert("TextFilterSpriteNameCaseInsensitive", ok, $"got \"{result}\"");
        }
    }
}
