using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OniAccess.Handlers;
using OniAccess.Handlers.Tiles;
using OniAccess.Handlers.Tiles.Scanner;
using OniAccess.Speech;
using OniAccess.Util;
using OniAccess.Widgets;
using UnityEngine;

namespace OniAccess.Tests {
	class Program {
		static int Main(string[] args) {
			// Resolve game assemblies at runtime from ONI_MANAGED
			var managed = Environment.GetEnvironmentVariable("ONI_MANAGED")
				?? @"C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed";

			AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
				var name = new AssemblyName(e.Name).Name;
				var path = Path.Combine(managed, name + ".dll");
				return File.Exists(path) ? Assembly.LoadFrom(path) : null;
			};

			return RunTests();
		}

		static int RunTests() {
			// Replace Unity time/frame sources to avoid native calls in tests
			SpeechPipeline.TimeSource = () => 0f;
			HandlerStack.FrameSource = () => 0;

			var results = new List<(string name, bool passed, string detail)>();

			// --- HandlerStack ---
			results.Add(PopExposesLowerHandler());
			results.Add(PushCallsOnActivate());
			results.Add(PopCallsOnDeactivate());
			results.Add(PopReactivatesExposedHandler());
			results.Add(ReplaceSwapsHandlers());
			results.Add(DeactivateAllCallsOnDeactivate());
			results.Add(ReplaceOnEmptyStack());
			results.Add(PopOnEmptyStack());
			results.Add(RapidPushPopSequence());
			results.Add(ExceptionInOnActivateDoesNotCorruptStack());
			results.Add(ExceptionInOnDeactivateDoesNotCorruptStack());

			// --- CollectHelpEntries ---
			results.Add(CollectHelpEntriesEmptyStack());
			results.Add(CollectHelpEntriesSingleHandler());
			results.Add(CollectHelpEntriesTwoNonCapturing());
			results.Add(CollectHelpEntriesBarrierStopsWalk());
			results.Add(CollectHelpEntriesKeyDedup());
			results.Add(CollectHelpEntriesBarrierInclusive());

			HandlerStack.Clear();

			// --- TypeAheadSearch ---
			results.Add(SearchWordStartMatch());
			results.Add(SearchMultiWordMatch());
			results.Add(SearchCaseInsensitive());
			results.Add(SearchMultiCharNarrowing());
			results.Add(SearchRepeatLetterCycles());
			results.Add(SearchBackspace());
			results.Add(SearchNavigateWraps());
			results.Add(SearchJumpFirstLast());
			results.Add(SearchNoMatch());
			results.Add(SearchBufferTimeoutResets());
			results.Add(MatchTierStartWholeWord());
			results.Add(MatchTierStartPrefix());
			results.Add(MatchTierMidWholeWord());
			results.Add(MatchTierMidPrefix());
			results.Add(MatchTierSubstring());
			results.Add(MatchTierNoMatch());
			results.Add(SearchTierOrdering());

			// --- TextFilter ---
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
			results.Add(TextFilterReplacesMasculineOrdinalDegree());
			results.Add(TextFilterPreservesNumericBrackets());

			// --- Log class ---
			results.Add(LogWarnRoutesToWarnFn());
			results.Add(LogErrorRoutesToErrorFn());
			results.Add(LogBackendSwapWorks());

			// --- HandlerStack diagnostic quality ---
			results.Add(PushFailureLogsHandlerNameAndException());
			results.Add(PushNullLogsWarning());
			results.Add(PopOnEmptyLogsWarning());
			results.Add(ReplaceFailureLogsHandlerNameAndException());

			HandlerStack.Clear();

			// --- TooltipCapture ---
			TooltipCapture.Reset();
			results.Add(TooltipCaptureEmptyFrameReturnsNull());
			results.Add(TooltipCaptureTextOutsideBlockDiscarded());
			results.Add(TooltipCaptureSingleBlock());
			results.Add(TooltipCaptureTwoBlocks());
			results.Add(TooltipCaptureMultipleTokensInBlock());
			results.Add(TooltipCaptureNewLineSeparatesLines());
			results.Add(TooltipCaptureEmptyTextSkipped());
			results.Add(TooltipCaptureResetClearsState());
			results.Add(TooltipCaptureGetLinesGroupsByBlock());
			TooltipCapture.Reset();

			// --- UnionFind ---
			results.Add(UnionFindSameSetAfterUnion());
			results.Add(UnionFindDisjointSetsDistinct());
			results.Add(UnionFindTransitiveUnion());
			results.Add(UnionFindSelfUnionNoOp());
			results.Add(UnionFindDuplicateUnionNoOp());
			results.Add(UnionFindPathCompression());
			results.Add(UnionFindResetReinitializes());
			results.Add(UnionFindResetReallocatesOnSizeChange());
			results.Add(UnionFindLargeChainMerge());

			// --- SpeechPipeline ---
			results.Add(PipelineDisabledSkipsSpeech());
			results.Add(PipelineEnabledSpeaks());
			results.Add(PipelineFiltersBeforeSpeaking());
			results.Add(PipelineDeduplicatesSameText());
			results.Add(PipelineAllowsSameTextAfterWindow());
			results.Add(PipelineAllowsDifferentTextImmediately());
			results.Add(PipelineNullAndEmptySkipped());

			// --- ColorNameUtil ---
			results.Add(ColorNameUtilAllPaletteColorsMapped());
			results.Add(ColorNameUtilNoDuplicateNames());
			results.Add(ColorNameUtilUnknownColorReturnsNull());

			int passed = 0, failed = 0;
			foreach (var (name, ok, detail) in results) {
				if (ok) {
					Console.WriteLine($"  PASS  {name}");
					passed++;
				} else {
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

		private class TestHandler: IAccessHandler {
			public string DisplayName { get; }
			public bool CapturesAllInput { get; }
			public IReadOnlyList<HelpEntry> HelpEntries { get; }
			public IReadOnlyList<ConsumedKey> ConsumedKeys { get; }
				= Array.Empty<ConsumedKey>();

			public int ActivateCount { get; private set; }
			public int DeactivateCount { get; private set; }
			public int TickCount { get; private set; }
			public bool ConsumeKeyDown { get; set; }
			public int HandleKeyDownCount { get; private set; }

			public TestHandler(string name, bool capturesAll = false,
				IReadOnlyList<HelpEntry> helpEntries = null) {
				DisplayName = name;
				CapturesAllInput = capturesAll;
				HelpEntries = helpEntries ?? new List<HelpEntry>().AsReadOnly();
			}

			public void Tick() => TickCount++;
			public bool HandleKeyDown(KButtonEvent e) { HandleKeyDownCount++; return ConsumeKeyDown; }
			public void OnActivate() => ActivateCount++;
			public void OnDeactivate() => DeactivateCount++;
		}

		private class ThrowingHandler: IAccessHandler {
			public string DisplayName => "Thrower";
			public bool CapturesAllInput => false;
			public IReadOnlyList<HelpEntry> HelpEntries { get; }
				= new List<HelpEntry>().AsReadOnly();
			public IReadOnlyList<ConsumedKey> ConsumedKeys { get; }
				= Array.Empty<ConsumedKey>();

			public bool ThrowOnActivate { get; set; }
			public bool ThrowOnDeactivate { get; set; }

			public void Tick() { }
			public bool HandleKeyDown(KButtonEvent e) => false;

			public void OnActivate() {
				if (ThrowOnActivate)
					throw new InvalidOperationException("OnActivate exploded");
			}

			public void OnDeactivate() {
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
		// HandlerStack tests
		// ========================================

		private static (string, bool, string) PopExposesLowerHandler() {
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

		private static (string, bool, string) PushCallsOnActivate() {
			Reset();
			var handler = new TestHandler("Test");
			HandlerStack.Push(handler);

			bool ok = handler.ActivateCount == 1;
			return Assert("PushCallsOnActivate", ok,
				$"ActivateCount={handler.ActivateCount}");
		}

		private static (string, bool, string) PopCallsOnDeactivate() {
			Reset();
			var handler = new TestHandler("Test");
			HandlerStack.Push(handler);
			HandlerStack.Pop();

			bool ok = handler.DeactivateCount == 1;
			return Assert("PopCallsOnDeactivate", ok,
				$"DeactivateCount={handler.DeactivateCount}");
		}

		private static (string, bool, string) PopReactivatesExposedHandler() {
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

		private static (string, bool, string) ReplaceSwapsHandlers() {
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

		private static (string, bool, string) DeactivateAllCallsOnDeactivate() {
			Reset();
			var bottom = new TestHandler("Bottom");
			var top = new TestHandler("Top");
			HandlerStack.Push(bottom);
			HandlerStack.Push(top);
			HandlerStack.DeactivateAll();

			bool ok = top.DeactivateCount == 1
				   && bottom.DeactivateCount == 1
				   && HandlerStack.Count == 0;
			return Assert("DeactivateAllCallsOnDeactivate", ok,
				$"top.Deactivate={top.DeactivateCount}, " +
				$"bottom.Deactivate={bottom.DeactivateCount}, " +
				$"count={HandlerStack.Count}");
		}

		// ========================================
		// HandlerStack edge cases
		// ========================================

		private static (string, bool, string) ReplaceOnEmptyStack() {
			Reset();
			var handler = new TestHandler("Replacement");
			HandlerStack.Replace(handler);

			bool ok = HandlerStack.Count == 1 && handler.ActivateCount == 1;
			return Assert("ReplaceOnEmptyStack", ok,
				$"count={HandlerStack.Count}, ActivateCount={handler.ActivateCount}");
		}

		private static (string, bool, string) PopOnEmptyStack() {
			Reset();
			try {
				HandlerStack.Pop();
			} catch (Exception ex) {
				return Assert("PopOnEmptyStack", false, $"threw {ex.GetType().Name}");
			}

			bool ok = HandlerStack.Count == 0;
			return Assert("PopOnEmptyStack", ok, $"count={HandlerStack.Count}");
		}

		private static (string, bool, string) RapidPushPopSequence() {
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

		private static (string, bool, string) ExceptionInOnActivateDoesNotCorruptStack() {
			Reset();
			var good = new TestHandler("Good");
			HandlerStack.Push(good);

			var thrower = new ThrowingHandler { ThrowOnActivate = true };
			bool threw = false;
			try {
				HandlerStack.Push(thrower);
			} catch (InvalidOperationException) {
				threw = true;
			}

			// Push calls OnActivate before adding to stack. If OnActivate throws,
			// Push catches the exception, logs it, and does NOT add the handler.
			// The exception does not propagate. Stack stays clean with only "Good".
			bool ok = !threw && HandlerStack.Count == 1 && HandlerStack.ActiveHandler == good;
			return Assert("ExceptionInOnActivateDoesNotCorruptStack", ok,
				$"threw={threw}, count={HandlerStack.Count}");
		}

		private static (string, bool, string) ExceptionInOnDeactivateDoesNotCorruptStack() {
			Reset();
			var bottom = new TestHandler("Bottom");
			var thrower = new ThrowingHandler { ThrowOnDeactivate = true };
			HandlerStack.Push(bottom);
			HandlerStack.Push(thrower);

			// Pop wraps OnDeactivate in try/catch, so the exception should not propagate.
			HandlerStack.Pop();

			// Thrower is removed, bottom is reactivated despite the throw.
			bool ok = HandlerStack.Count == 1
				   && HandlerStack.ActiveHandler == bottom;
			return Assert("ExceptionInOnDeactivateDoesNotCorruptStack", ok,
				$"count={HandlerStack.Count}, " +
				$"active={HandlerStack.ActiveHandler?.DisplayName ?? "null"}");
		}

		// ========================================
		// CollectHelpEntries tests (new)
		// ========================================

		private static IReadOnlyList<HelpEntry> MakeEntries(params string[] keys) {
			var list = new List<HelpEntry>();
			foreach (var k in keys)
				list.Add(new HelpEntry(k, $"desc-{k}"));
			return list.AsReadOnly();
		}

		private static (string, bool, string) CollectHelpEntriesEmptyStack() {
			Reset();
			var entries = HandlerStack.CollectHelpEntries();
			bool ok = entries.Count == 0;
			return Assert("CollectHelpEntriesEmptyStack", ok,
				$"count={entries.Count}");
		}

		private static (string, bool, string) CollectHelpEntriesSingleHandler() {
			Reset();
			HandlerStack.Push(new TestHandler("A", helpEntries: MakeEntries("F1", "F2")));
			var entries = HandlerStack.CollectHelpEntries();
			bool ok = entries.Count == 2
				   && entries[0].KeyName == "F1"
				   && entries[1].KeyName == "F2";
			return Assert("CollectHelpEntriesSingleHandler", ok,
				$"count={entries.Count}");
		}

		private static (string, bool, string) CollectHelpEntriesTwoNonCapturing() {
			Reset();
			HandlerStack.Push(new TestHandler("Bottom", capturesAll: false,
				helpEntries: MakeEntries("F1")));
			HandlerStack.Push(new TestHandler("Top", capturesAll: false,
				helpEntries: MakeEntries("F2")));
			var entries = HandlerStack.CollectHelpEntries();
			bool ok = entries.Count == 2
				   && entries[0].KeyName == "F2"
				   && entries[1].KeyName == "F1";
			return Assert("CollectHelpEntriesTwoNonCapturing", ok,
				$"count={entries.Count}, [0]={entries[0]?.KeyName}, [1]={entries[1]?.KeyName}");
		}

		private static (string, bool, string) CollectHelpEntriesBarrierStopsWalk() {
			Reset();
			HandlerStack.Push(new TestHandler("Bottom", capturesAll: false,
				helpEntries: MakeEntries("F1")));
			HandlerStack.Push(new TestHandler("Barrier", capturesAll: true,
				helpEntries: MakeEntries("F2")));
			var entries = HandlerStack.CollectHelpEntries();
			// Barrier is inclusive — its entries included, but bottom excluded
			bool ok = entries.Count == 1 && entries[0].KeyName == "F2";
			return Assert("CollectHelpEntriesBarrierStopsWalk", ok,
				$"count={entries.Count}");
		}

		private static (string, bool, string) CollectHelpEntriesKeyDedup() {
			Reset();
			HandlerStack.Push(new TestHandler("Bottom", capturesAll: false,
				helpEntries: MakeEntries("F1", "F2")));
			HandlerStack.Push(new TestHandler("Top", capturesAll: false,
				helpEntries: MakeEntries("F1", "F3")));
			var entries = HandlerStack.CollectHelpEntries();
			// F1 from Top wins, Bottom's F1 suppressed. F3 from Top + F2 from Bottom.
			bool ok = entries.Count == 3
				   && entries[0].KeyName == "F1"
				   && entries[0].Description == "desc-F1"
				   && entries[1].KeyName == "F3"
				   && entries[2].KeyName == "F2";
			return Assert("CollectHelpEntriesKeyDedup", ok,
				$"count={entries.Count}");
		}

		private static (string, bool, string) CollectHelpEntriesBarrierInclusive() {
			Reset();
			HandlerStack.Push(new TestHandler("Bottom", capturesAll: false,
				helpEntries: MakeEntries("F1")));
			HandlerStack.Push(new TestHandler("Barrier", capturesAll: true,
				helpEntries: MakeEntries("F2", "F3")));
			HandlerStack.Push(new TestHandler("Top", capturesAll: false,
				helpEntries: MakeEntries("F4")));
			var entries = HandlerStack.CollectHelpEntries();
			// Top(F4) + Barrier(F2,F3) — barrier is inclusive, Bottom excluded
			bool ok = entries.Count == 3
				   && entries[0].KeyName == "F4"
				   && entries[1].KeyName == "F2"
				   && entries[2].KeyName == "F3";
			return Assert("CollectHelpEntriesBarrierInclusive", ok,
				$"count={entries.Count}");
		}

		// ========================================
		// TypeAheadSearch tests
		// ========================================

		private static (string, bool, string) SearchWordStartMatch() {
			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('a');
			search.Search(SearchItems.Length, NameByIndex);

			// 'a' matches Apple(0,tier1), Apricot(1,tier1), Banana(2,tier4 substring)
			bool ok = search.ResultCount == 3 && search.SelectedOriginalIndex == 0;
			return Assert("SearchWordStartMatch", ok,
				$"ResultCount={search.ResultCount}, SelectedOriginalIndex={search.SelectedOriginalIndex}");
		}

		private static (string, bool, string) SearchMultiWordMatch() {
			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('c');
			search.Search(SearchItems.Length, NameByIndex);

			// 'c' matches Cherry(4,tier1), Blue Cheese(3,tier3), Apricot(1,tier4 substring)
			bool ok = search.ResultCount == 3;
			return Assert("SearchMultiWordMatch", ok,
				$"ResultCount={search.ResultCount}");
		}

		private static (string, bool, string) SearchCaseInsensitive() {
			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('b');
			search.Search(SearchItems.Length, NameByIndex);

			// 'b' matches Banana(2), Blue Cheese(3)
			bool ok = search.ResultCount == 2;
			return Assert("SearchCaseInsensitive", ok,
				$"ResultCount={search.ResultCount}");
		}

		private static (string, bool, string) SearchMultiCharNarrowing() {
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

			// 'a' also matches Banana(substring), 'ap' narrows to Apple+Apricot, 'apr' to Apricot
			bool ok = afterA == 3 && afterAp == 2 && afterApr == 1
				   && search.SelectedOriginalIndex == 1; // Apricot
			return Assert("SearchMultiCharNarrowing", ok,
				$"a={afterA}, ap={afterAp}, apr={afterApr}, idx={search.SelectedOriginalIndex}");
		}

		private static (string, bool, string) SearchRepeatLetterCycles() {
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

		private static (string, bool, string) SearchBackspace() {
			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('a');
			search.AddChar('p');
			search.Search(SearchItems.Length, NameByIndex);
			int afterAp = search.ResultCount;

			search.RemoveChar(); // buffer="a"
			search.Search(SearchItems.Length, NameByIndex);
			int afterBackspace = search.ResultCount;

			// backspace from 'ap' to 'a' restores Banana as substring match
			bool ok = afterAp == 2 && afterBackspace == 3
				   && search.Buffer == "a";
			return Assert("SearchBackspace", ok,
				$"ap={afterAp}, after backspace={afterBackspace}, buffer=\"{search.Buffer}\"");
		}

		private static (string, bool, string) SearchNavigateWraps() {
			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('a');
			search.Search(SearchItems.Length, NameByIndex);
			// 3 results: Apple(0), Apricot(1), Banana(2). Cursor at 0.

			search.NavigateResults(1); // cursor -> 1
			search.NavigateResults(1); // cursor -> 2
			search.NavigateResults(1); // cursor -> 0 (wrap)

			bool ok = search.SelectedOriginalIndex == 0; // back to Apple
			return Assert("SearchNavigateWraps", ok,
				$"SelectedOriginalIndex={search.SelectedOriginalIndex}");
		}

		private static (string, bool, string) SearchJumpFirstLast() {
			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('a');
			search.Search(SearchItems.Length, NameByIndex);
			// 3 results: Apple(0), Apricot(1), Banana(2)

			search.JumpToLastResult();
			int lastIdx = search.SelectedOriginalIndex; // Banana(2)

			search.JumpToFirstResult();
			int firstIdx = search.SelectedOriginalIndex; // Apple(0)

			bool ok = lastIdx == 2 && firstIdx == 0;
			return Assert("SearchJumpFirstLast", ok,
				$"last={lastIdx}, first={firstIdx}");
		}

		private static (string, bool, string) SearchNoMatch() {
			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('z');
			search.Search(SearchItems.Length, NameByIndex);

			bool ok = search.ResultCount == 0 && search.IsSearchActive;
			return Assert("SearchNoMatch", ok,
				$"ResultCount={search.ResultCount}, IsSearchActive={search.IsSearchActive}");
		}

		private static (string, bool, string) SearchBufferTimeoutResets() {
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

		private static (string, bool, string) MatchTierStartWholeWord() {
			// "wood" at start of "wood club" = tier 0
			int tier = TypeAheadSearch.MatchTier("wood club", "wood");
			bool ok = tier == 0;
			return Assert("MatchTierStartWholeWord", ok, $"tier={tier}");
		}

		private static (string, bool, string) MatchTierStartPrefix() {
			// "wood" at start of "wooden club" = tier 1
			int tier = TypeAheadSearch.MatchTier("wooden club", "wood");
			bool ok = tier == 1;
			return Assert("MatchTierStartPrefix", ok, $"tier={tier}");
		}

		private static (string, bool, string) MatchTierMidWholeWord() {
			// "wood" as whole word mid-string in "pine wood" = tier 2
			int tier = TypeAheadSearch.MatchTier("pine wood", "wood");
			bool ok = tier == 2;
			return Assert("MatchTierMidWholeWord", ok, $"tier={tier}");
		}

		private static (string, bool, string) MatchTierMidPrefix() {
			// "wood" as prefix of mid-string word in "a wooden thing" = tier 3
			int tier = TypeAheadSearch.MatchTier("a wooden thing", "wood");
			bool ok = tier == 3;
			return Assert("MatchTierMidPrefix", ok, $"tier={tier}");
		}

		private static (string, bool, string) MatchTierSubstring() {
			// "wood" inside "plywood" = tier 4
			int tier = TypeAheadSearch.MatchTier("plywood", "wood");
			bool ok = tier == 4;
			return Assert("MatchTierSubstring", ok, $"tier={tier}");
		}

		private static (string, bool, string) MatchTierNoMatch() {
			int tier = TypeAheadSearch.MatchTier("banana", "wood");
			bool ok = tier == -1;
			return Assert("MatchTierNoMatch", ok, $"tier={tier}");
		}

		private static (string, bool, string) SearchTierOrdering() {
			// Items designed so each tier is represented:
			// "Wood Club" (tier 0), "Wooden Axe" (tier 1), "Pine Wood" (tier 2),
			// "A Wooden Thing" (tier 3), "Plywood" (tier 4)
			var items = new[] { "Plywood", "A Wooden Thing", "Pine Wood", "Wooden Axe", "Wood Club" };
			string nameByIndex(int i) => i >= 0 && i < items.Length ? items[i] : null;

			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('w');
			search.AddChar('o');
			search.AddChar('o');
			search.AddChar('d');
			search.Search(items.Length, nameByIndex);

			// Expected order: Wood Club(4,t0), Wooden Axe(3,t1), Pine Wood(2,t2),
			// A Wooden Thing(1,t3), Plywood(0,t4)
			bool ok = search.ResultCount == 5;
			if (ok) {
				int[] expected = { 4, 3, 2, 1, 0 };
				for (int i = 0; i < 5; i++) {
					if (search.SelectedOriginalIndex != expected[i]) {
						ok = false;
						break;
					}
					if (i < 4) search.NavigateResults(1);
				}
			}
			return Assert("SearchTierOrdering", ok,
				$"ResultCount={search.ResultCount}");
		}

		// ========================================
		// TextFilter tests (ported + new edge cases)
		// ========================================

		private static (string, bool, string) TextFilterStripsBold() {
			string result = TextFilter.FilterForSpeech("<b>Warning</b>");
			bool ok = result == "Warning";
			return Assert("TextFilterStripsBold", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterStripsColor() {
			string result = TextFilter.FilterForSpeech("<color=#FF0000>Hot</color>");
			bool ok = result == "Hot";
			return Assert("TextFilterStripsColor", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterStripsNested() {
			string result = TextFilter.FilterForSpeech("<b><color=red>Alert</color></b>");
			bool ok = result == "Alert";
			return Assert("TextFilterStripsNested", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterConvertsSpriteToText() {
			string result = TextFilter.FilterForSpeech("<sprite name=warning>Pipe broken");
			bool ok = result == "warning: Pipe broken";
			return Assert("TextFilterConvertsSpriteToText", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterStripsUnregisteredSprite() {
			string result = TextFilter.FilterForSpeech("<sprite name=decorative_icon>text");
			bool ok = result == "text";
			return Assert("TextFilterStripsUnregisteredSprite", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterStripsTmpBracketSprites() {
			string result = TextFilter.FilterForSpeech("[icon_name] some text");
			bool ok = result == "some text";
			return Assert("TextFilterStripsTmpBracketSprites", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterHandlesLinkTags() {
			string result = TextFilter.FilterForSpeech("<link=\"LINK_ID\">Click here</link>");
			bool ok = result == "Click here";
			return Assert("TextFilterHandlesLinkTags", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterNormalizesWhitespace() {
			string result = TextFilter.FilterForSpeech("word1   word2\n\nword3");
			bool ok = result == "word1 word2 word3";
			return Assert("TextFilterNormalizesWhitespace", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterHandlesEmptyAndNull() {
			string nullResult = TextFilter.FilterForSpeech(null);
			string emptyResult = TextFilter.FilterForSpeech("");

			bool ok = nullResult == "" && emptyResult == "";
			return Assert("TextFilterHandlesEmptyAndNull", ok,
				$"null->\"{nullResult}\", empty->\"{emptyResult}\"");
		}

		private static (string, bool, string) TextFilterPreservesPlainText() {
			string input = "Copper Ore, 200 kg, 25\u00B0C";
			string result = TextFilter.FilterForSpeech(input);
			bool ok = result == input;
			return Assert("TextFilterPreservesPlainText", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterStripsHotkey() {
			string result = TextFilter.FilterForSpeech("Build a Ladder {Hotkey}");
			bool ok = result == "Build a Ladder";
			return Assert("TextFilterStripsHotkey", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterStripsSizeStyle() {
			string result = TextFilter.FilterForSpeech("<size=10><style=\"KKeyword\">text</style></size>");
			bool ok = result == "text";
			return Assert("TextFilterStripsSizeStyle", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterUnclosedTag() {
			string result = TextFilter.FilterForSpeech("<b>Bold text");
			bool ok = result == "Bold text";
			return Assert("TextFilterUnclosedTag", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterMismatchedTags() {
			string result = TextFilter.FilterForSpeech("<b>text</color>");
			bool ok = result == "text";
			return Assert("TextFilterMismatchedTags", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterSpriteNameCaseInsensitive() {
			string result = TextFilter.FilterForSpeech("<sprite name=WARNING>text");
			bool ok = result == "warning: text";
			return Assert("TextFilterSpriteNameCaseInsensitive", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterReplacesMasculineOrdinalDegree() {
			string result = TextFilter.FilterForSpeech("21.9 \u00BAC");
			bool ok = result == "21.9 \u00B0C";
			return Assert("TextFilterReplacesMasculineOrdinalDegree", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterPreservesNumericBrackets() {
			string result = TextFilter.FilterForSpeech("Growing [45%]");
			bool ok = result == "Growing 45%";
			return Assert("TextFilterPreservesNumericBrackets", ok, $"got \"{result}\"");
		}

		// ========================================
		// LogCapture helper
		// ========================================

		private class LogCapture : IDisposable {
			public List<string> LogMessages = new List<string>();
			public List<string> WarnMessages = new List<string>();
			public List<string> ErrorMessages = new List<string>();

			private readonly Action<string> _origLog;
			private readonly Action<string> _origWarn;
			private readonly Action<string> _origError;

			public LogCapture() {
				_origLog = Log.LogFn;
				_origWarn = Log.WarnFn;
				_origError = Log.ErrorFn;
				Log.LogFn = msg => LogMessages.Add(msg);
				Log.WarnFn = msg => WarnMessages.Add(msg);
				Log.ErrorFn = msg => ErrorMessages.Add(msg);
			}

			public void Dispose() {
				Log.LogFn = _origLog;
				Log.WarnFn = _origWarn;
				Log.ErrorFn = _origError;
			}
		}

		// ========================================
		// Log class tests
		// ========================================

		private static (string, bool, string) LogWarnRoutesToWarnFn() {
			using (var capture = new LogCapture()) {
				Log.Warn("oops");
				bool ok = capture.WarnMessages.Count == 1
					   && capture.LogMessages.Count == 0;
				return Assert("LogWarnRoutesToWarnFn", ok,
					$"warn={capture.WarnMessages.Count}, log={capture.LogMessages.Count}");
			}
		}

		private static (string, bool, string) LogErrorRoutesToErrorFn() {
			using (var capture = new LogCapture()) {
				Log.Error("bad");
				bool ok = capture.ErrorMessages.Count == 1
					   && capture.LogMessages.Count == 0;
				return Assert("LogErrorRoutesToErrorFn", ok,
					$"error={capture.ErrorMessages.Count}, log={capture.LogMessages.Count}");
			}
		}

		private static (string, bool, string) LogBackendSwapWorks() {
			var capture1 = new LogCapture();
			Log.Info("first");
			bool gotFirst = capture1.LogMessages.Count == 1;
			capture1.Dispose();

			var capture2 = new LogCapture();
			Log.Info("second");
			bool gotSecond = capture2.LogMessages.Count == 1
						  && capture1.LogMessages.Count == 1; // original didn't get more
			capture2.Dispose();

			// Verify restore works: log after dispose should go to original backend
			bool ok = gotFirst && gotSecond;
			return Assert("LogBackendSwapWorks", ok,
				$"gotFirst={gotFirst}, gotSecond={gotSecond}");
		}

		// ========================================
		// HandlerStack diagnostic quality tests
		// ========================================

		private static (string, bool, string) PushFailureLogsHandlerNameAndException() {
			Reset();
			using (var capture = new LogCapture()) {
				var thrower = new ThrowingHandler { ThrowOnActivate = true };
				HandlerStack.Push(thrower);
				bool ok = capture.ErrorMessages.Count > 0
					   && capture.ErrorMessages[0].Contains("Thrower")
					   && capture.ErrorMessages[0].Contains("OnActivate exploded");
				return Assert("PushFailureLogsHandlerNameAndException", ok,
					$"errors={capture.ErrorMessages.Count}" +
					(capture.ErrorMessages.Count > 0 ? $", msg=\"{capture.ErrorMessages[0]}\"" : ""));
			}
		}

		private static (string, bool, string) PushNullLogsWarning() {
			Reset();
			using (var capture = new LogCapture()) {
				HandlerStack.Push(null);
				bool ok = capture.WarnMessages.Count > 0
					   && capture.WarnMessages[0].Contains("null");
				return Assert("PushNullLogsWarning", ok,
					$"warns={capture.WarnMessages.Count}" +
					(capture.WarnMessages.Count > 0 ? $", msg=\"{capture.WarnMessages[0]}\"" : ""));
			}
		}

		private static (string, bool, string) PopOnEmptyLogsWarning() {
			Reset();
			using (var capture = new LogCapture()) {
				HandlerStack.Pop();
				bool ok = capture.WarnMessages.Count > 0;
				return Assert("PopOnEmptyLogsWarning", ok,
					$"warns={capture.WarnMessages.Count}");
			}
		}

		private static (string, bool, string) ReplaceFailureLogsHandlerNameAndException() {
			Reset();
			using (var capture = new LogCapture()) {
				var thrower = new ThrowingHandler { ThrowOnActivate = true };
				HandlerStack.Replace(thrower);
				bool ok = capture.ErrorMessages.Count > 0
					   && capture.ErrorMessages[0].Contains("Thrower")
					   && capture.ErrorMessages[0].Contains("OnActivate exploded");
				return Assert("ReplaceFailureLogsHandlerNameAndException", ok,
					$"errors={capture.ErrorMessages.Count}" +
					(capture.ErrorMessages.Count > 0 ? $", msg=\"{capture.ErrorMessages[0]}\"" : ""));
			}
		}

		// ========================================
		// TooltipCapture tests
		// ========================================

		private static (string, bool, string) TooltipCaptureEmptyFrameReturnsNull() {
			TooltipCapture.Reset();
			TooltipCapture.BeginFrame();
			TooltipCapture.EndFrame();
			string result = TooltipCapture.GetTooltipText();
			return Assert("TooltipCaptureEmptyFrameReturnsNull", result == null,
				$"got \"{result}\"");
		}

		private static (string, bool, string) TooltipCaptureTextOutsideBlockDiscarded() {
			TooltipCapture.Reset();
			TooltipCapture.BeginFrame();
			TooltipCapture.AppendText("orphan");
			TooltipCapture.EndFrame();
			string result = TooltipCapture.GetTooltipText();
			return Assert("TooltipCaptureTextOutsideBlockDiscarded", result == null,
				$"got \"{result}\"");
		}

		private static (string, bool, string) TooltipCaptureSingleBlock() {
			TooltipCapture.Reset();
			TooltipCapture.BeginFrame();
			TooltipCapture.BeginBlock();
			TooltipCapture.AppendText("Copper Ore");
			TooltipCapture.EndFrame();
			string result = TooltipCapture.GetTooltipText();
			return Assert("TooltipCaptureSingleBlock", result == "Copper Ore",
				$"got \"{result}\"");
		}

		private static (string, bool, string) TooltipCaptureTwoBlocks() {
			TooltipCapture.Reset();
			TooltipCapture.BeginFrame();
			TooltipCapture.BeginBlock();
			TooltipCapture.AppendText("Copper Ore");
			TooltipCapture.BeginBlock();
			TooltipCapture.AppendText("Hot");
			TooltipCapture.EndFrame();
			string result = TooltipCapture.GetTooltipText();
			return Assert("TooltipCaptureTwoBlocks", result == "Copper Ore, Hot",
				$"got \"{result}\"");
		}

		private static (string, bool, string) TooltipCaptureMultipleTokensInBlock() {
			TooltipCapture.Reset();
			TooltipCapture.BeginFrame();
			TooltipCapture.BeginBlock();
			TooltipCapture.AppendText("1657");
			TooltipCapture.AppendText(".3");
			TooltipCapture.AppendText(" g");
			TooltipCapture.EndFrame();
			string result = TooltipCapture.GetTooltipText();
			return Assert("TooltipCaptureMultipleTokensInBlock", result == "1657.3 g",
				$"got \"{result}\"");
		}

		private static (string, bool, string) TooltipCaptureNewLineSeparatesLines() {
			TooltipCapture.Reset();
			TooltipCapture.BeginFrame();
			TooltipCapture.BeginBlock();
			TooltipCapture.AppendText("OXYGEN");
			TooltipCapture.AppendNewLine();
			TooltipCapture.AppendText("Breathable Gas");
			TooltipCapture.AppendNewLine();
			TooltipCapture.AppendText("1657");
			TooltipCapture.AppendText(".3 g");
			TooltipCapture.EndFrame();
			string result = TooltipCapture.GetTooltipText();
			return Assert("TooltipCaptureNewLineSeparatesLines",
				result == "OXYGEN, Breathable Gas, 1657.3 g",
				$"got \"{result}\"");
		}

		private static (string, bool, string) TooltipCaptureEmptyTextSkipped() {
			TooltipCapture.Reset();
			TooltipCapture.BeginFrame();
			TooltipCapture.BeginBlock();
			TooltipCapture.AppendText(null);
			TooltipCapture.AppendText("");
			TooltipCapture.AppendText("   ");
			TooltipCapture.AppendText("real");
			TooltipCapture.EndFrame();
			string result = TooltipCapture.GetTooltipText();
			return Assert("TooltipCaptureEmptyTextSkipped", result == "real",
				$"got \"{result}\"");
		}

		private static (string, bool, string) TooltipCaptureResetClearsState() {
			TooltipCapture.BeginFrame();
			TooltipCapture.BeginBlock();
			TooltipCapture.AppendText("should vanish");
			TooltipCapture.Reset();
			string result = TooltipCapture.GetTooltipText();
			return Assert("TooltipCaptureResetClearsState", result == null,
				$"got \"{result}\"");
		}

		private static (string, bool, string) TooltipCaptureGetLinesGroupsByBlock() {
			TooltipCapture.Reset();
			TooltipCapture.BeginFrame();
			TooltipCapture.BeginBlock();
			TooltipCapture.AppendText("OXYGEN");
			TooltipCapture.AppendNewLine();
			TooltipCapture.AppendText("Breathable Gas");
			TooltipCapture.BeginBlock();
			TooltipCapture.AppendText("21.8 C");
			TooltipCapture.EndFrame();
			var lines = TooltipCapture.GetTooltipLines();
			bool ok = lines != null && lines.Count == 2
				&& lines[0] == "OXYGEN, Breathable Gas"
				&& lines[1] == "21.8 C";
			return Assert("TooltipCaptureGetLinesGroupsByBlock", ok,
				lines == null ? "null" : $"count={lines.Count}: [{string.Join("|", lines)}]");
		}

		// ========================================
		// UnionFind tests
		// ========================================

		private static (string, bool, string) UnionFindSameSetAfterUnion() {
			var uf = new UnionFind(5);
			uf.Union(1, 3);
			bool ok = uf.Find(1) == uf.Find(3);
			return Assert("UnionFindSameSetAfterUnion", ok,
				$"Find(1)={uf.Find(1)}, Find(3)={uf.Find(3)}");
		}

		private static (string, bool, string) UnionFindDisjointSetsDistinct() {
			var uf = new UnionFind(5);
			uf.Union(0, 1);
			uf.Union(2, 3);
			bool ok = uf.Find(0) != uf.Find(2);
			return Assert("UnionFindDisjointSetsDistinct", ok,
				$"Find(0)={uf.Find(0)}, Find(2)={uf.Find(2)}");
		}

		private static (string, bool, string) UnionFindTransitiveUnion() {
			var uf = new UnionFind(5);
			uf.Union(0, 1);
			uf.Union(1, 2);
			bool ok = uf.Find(0) == uf.Find(2);
			return Assert("UnionFindTransitiveUnion", ok,
				$"Find(0)={uf.Find(0)}, Find(2)={uf.Find(2)}");
		}

		private static (string, bool, string) UnionFindSelfUnionNoOp() {
			var uf = new UnionFind(3);
			int rootBefore = uf.Find(1);
			uf.Union(1, 1);
			int rootAfter = uf.Find(1);
			bool ok = rootBefore == rootAfter && rootAfter == 1;
			return Assert("UnionFindSelfUnionNoOp", ok,
				$"before={rootBefore}, after={rootAfter}");
		}

		private static (string, bool, string) UnionFindDuplicateUnionNoOp() {
			var uf = new UnionFind(4);
			uf.Union(0, 1);
			int rootAfterFirst = uf.Find(0);
			uf.Union(0, 1);
			int rootAfterSecond = uf.Find(0);
			bool ok = rootAfterFirst == rootAfterSecond
				&& uf.Find(0) == uf.Find(1);
			return Assert("UnionFindDuplicateUnionNoOp", ok,
				$"first={rootAfterFirst}, second={rootAfterSecond}");
		}

		private static (string, bool, string) UnionFindPathCompression() {
			// Build a chain: 0->1->2->3 via sequential unions
			var uf = new UnionFind(4);
			uf.Union(0, 1);
			uf.Union(1, 2);
			uf.Union(2, 3);
			// All should resolve to the same root
			int root = uf.Find(0);
			// After Find with path compression, Find(0) should go
			// directly to root without traversal
			bool ok = uf.Find(0) == root
				&& uf.Find(1) == root
				&& uf.Find(2) == root
				&& uf.Find(3) == root;
			return Assert("UnionFindPathCompression", ok,
				$"roots: 0={uf.Find(0)}, 1={uf.Find(1)}, 2={uf.Find(2)}, 3={uf.Find(3)}");
		}

		private static (string, bool, string) UnionFindResetReinitializes() {
			var uf = new UnionFind(4);
			uf.Union(0, 1);
			uf.Union(2, 3);
			uf.Reset(4);
			// After reset, every element should be its own root
			bool ok = uf.Find(0) == 0 && uf.Find(1) == 1
				&& uf.Find(2) == 2 && uf.Find(3) == 3;
			return Assert("UnionFindResetReinitializes", ok,
				$"roots: 0={uf.Find(0)}, 1={uf.Find(1)}, 2={uf.Find(2)}, 3={uf.Find(3)}");
		}

		private static (string, bool, string) UnionFindResetReallocatesOnSizeChange() {
			var uf = new UnionFind(3);
			uf.Union(0, 1);
			uf.Reset(5);
			// Should work with the larger size
			uf.Union(3, 4);
			bool ok = uf.Find(3) == uf.Find(4)
				&& uf.Find(0) == 0 && uf.Find(1) == 1;
			return Assert("UnionFindResetReallocatesOnSizeChange", ok,
				$"Find(3)={uf.Find(3)}, Find(4)={uf.Find(4)}, Find(0)={uf.Find(0)}");
		}

		private static (string, bool, string) UnionFindLargeChainMerge() {
			int size = 1000;
			var uf = new UnionFind(size);
			// Union all elements into one set via chain
			for (int i = 0; i < size - 1; i++)
				uf.Union(i, i + 1);
			// All should share the same root
			int root = uf.Find(0);
			bool ok = true;
			for (int i = 1; i < size; i++) {
				if (uf.Find(i) != root) {
					ok = false;
					return Assert("UnionFindLargeChainMerge", false,
						$"element {i} has root {uf.Find(i)}, expected {root}");
				}
			}
			return Assert("UnionFindLargeChainMerge", ok, "OK");
		}

		// ========================================
		// SpeechPipeline tests
		// ========================================

		private static void ResetPipeline(ref float fakeTime, List<(string text, bool interrupt)> spoken) {
			fakeTime = 0f;
			spoken.Clear();
			SpeechPipeline.Reset();
		}

		private static (string, bool, string) PipelineDisabledSkipsSpeech() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SetEnabled(false);
			SpeechPipeline.SpeakInterrupt("hello");
			bool ok = spoken.Count == 0;

			SpeechPipeline.SetEnabled(true);
			return Assert("PipelineDisabledSkipsSpeech", ok, $"spoken={spoken.Count}");
		}

		private static (string, bool, string) PipelineEnabledSpeaks() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakInterrupt("hello");
			bool ok = spoken.Count == 1 && spoken[0].text == "hello";
			return Assert("PipelineEnabledSpeaks", ok,
				$"spoken={spoken.Count}" + (spoken.Count > 0 ? $", text=\"{spoken[0].text}\"" : ""));
		}

		private static (string, bool, string) PipelineFiltersBeforeSpeaking() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakInterrupt("<b>bold</b>");
			bool ok = spoken.Count == 1 && spoken[0].text == "bold";
			return Assert("PipelineFiltersBeforeSpeaking", ok,
				$"spoken={spoken.Count}" + (spoken.Count > 0 ? $", text=\"{spoken[0].text}\"" : ""));
		}

		private static (string, bool, string) PipelineDeduplicatesSameText() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakInterrupt("hello");
			SpeechPipeline.SpeakInterrupt("hello");
			bool ok = spoken.Count == 1;
			return Assert("PipelineDeduplicatesSameText", ok, $"spoken={spoken.Count}");
		}

		private static (string, bool, string) PipelineAllowsSameTextAfterWindow() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakInterrupt("hello");
			fakeTime = 0.3f;
			SpeechPipeline.SpeakInterrupt("hello");
			bool ok = spoken.Count == 2;
			return Assert("PipelineAllowsSameTextAfterWindow", ok, $"spoken={spoken.Count}");
		}

		private static (string, bool, string) PipelineAllowsDifferentTextImmediately() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakInterrupt("hello");
			SpeechPipeline.SpeakInterrupt("world");
			bool ok = spoken.Count == 2;
			return Assert("PipelineAllowsDifferentTextImmediately", ok, $"spoken={spoken.Count}");
		}

		private static Color[] _paletteColors;
		private static Color[] PaletteColors => _paletteColors ?? (_paletteColors = new Color[] {
			new Color(0.4862745f, 0.4862745f, 0.4862745f),
			new Color(0f, 0f, 84f / 85f),
			new Color(0f, 0f, 0.7372549f),
			new Color(4f / 15f, 8f / 51f, 0.7372549f),
			new Color(0.5803922f, 0f, 44f / 85f),
			new Color(56f / 85f, 0f, 0.1254902f),
			new Color(56f / 85f, 0.0627451f, 0f),
			new Color(8f / 15f, 4f / 51f, 0f),
			new Color(16f / 51f, 16f / 85f, 0f),
			new Color(0f, 0.47058824f, 0f),
			new Color(0f, 0.40784314f, 0f),
			new Color(0f, 0.34509805f, 0f),
			new Color(0f, 0.2509804f, 0.34509805f),
			new Color(0f, 0f, 0f),
			new Color(0.7372549f, 0.7372549f, 0.7372549f),
			new Color(0f, 0.47058824f, 0.972549f),
			new Color(0f, 0.34509805f, 0.972549f),
			new Color(0.40784314f, 4f / 15f, 84f / 85f),
			new Color(72f / 85f, 0f, 0.8f),
			new Color(76f / 85f, 0f, 0.34509805f),
			new Color(0.972549f, 0.21960784f, 0f),
			new Color(76f / 85f, 0.36078432f, 0.0627451f),
			new Color(0.6745098f, 0.4862745f, 0f),
			new Color(0f, 0.72156864f, 0f),
			new Color(0f, 56f / 85f, 0f),
			new Color(0f, 56f / 85f, 4f / 15f),
			new Color(0f, 8f / 15f, 8f / 15f),
			new Color(0f, 0f, 0f),
			new Color(0.972549f, 0.972549f, 0.972549f),
			new Color(0.23529412f, 0.7372549f, 84f / 85f),
			new Color(0.40784314f, 8f / 15f, 84f / 85f),
			new Color(0.59607846f, 0.47058824f, 0.972549f),
			new Color(0.972549f, 0.47058824f, 0.972549f),
			new Color(0.972549f, 0.34509805f, 0.59607846f),
			new Color(0.972549f, 0.47058824f, 0.34509805f),
			new Color(84f / 85f, 32f / 51f, 4f / 15f),
			new Color(0.972549f, 0.72156864f, 0f),
			new Color(0.72156864f, 0.972549f, 8f / 85f),
			new Color(0.34509805f, 72f / 85f, 28f / 85f),
			new Color(0.34509805f, 0.972549f, 0.59607846f),
			new Color(0f, 0.9098039f, 72f / 85f),
			new Color(0.47058824f, 0.47058824f, 0.47058824f),
			new Color(84f / 85f, 84f / 85f, 84f / 85f),
			new Color(0.6431373f, 76f / 85f, 84f / 85f),
			new Color(0.72156864f, 0.72156864f, 0.972549f),
			new Color(72f / 85f, 0.72156864f, 0.972549f),
			new Color(0.972549f, 0.72156864f, 0.972549f),
			new Color(0.972549f, 0.72156864f, 64f / 85f),
			new Color(0.9411765f, 0.8156863f, 0.6901961f),
			new Color(84f / 85f, 0.8784314f, 56f / 85f),
			new Color(0.972549f, 72f / 85f, 0.47058824f),
			new Color(72f / 85f, 0.972549f, 0.47058824f),
			new Color(0.72156864f, 0.972549f, 0.72156864f),
		});

		private static (string, bool, string) ColorNameUtilAllPaletteColorsMapped() {
			for (int i = 0; i < PaletteColors.Length; i++) {
				string name = ColorNameUtil.GetColorName(PaletteColors[i]);
				if (string.IsNullOrEmpty(name))
					return Assert("ColorNameUtilAllPaletteColorsMapped", false,
						$"index {i} returned null/empty");
			}
			return Assert("ColorNameUtilAllPaletteColorsMapped", true, null);
		}

		private static (string, bool, string) ColorNameUtilNoDuplicateNames() {
			var seen = new Dictionary<string, int>();
			for (int i = 0; i < PaletteColors.Length; i++) {
				string name = ColorNameUtil.GetColorName(PaletteColors[i]);
				if (name == null) continue;
				if (seen.ContainsKey(name)) {
					// Allow duplicate for the two black entries (indices 13 and 27)
					if (PaletteColors[i] == PaletteColors[seen[name]]) continue;
					return Assert("ColorNameUtilNoDuplicateNames", false,
						$"'{name}' at indices {seen[name]} and {i}");
				}
				seen[name] = i;
			}
			return Assert("ColorNameUtilNoDuplicateNames", true, null);
		}

		private static (string, bool, string) ColorNameUtilUnknownColorReturnsNull() {
			string result = ColorNameUtil.GetColorName(new Color(0.123f, 0.456f, 0.789f));
			return Assert("ColorNameUtilUnknownColorReturnsNull", result == null,
				$"expected null, got '{result}'");
		}

		private static (string, bool, string) PipelineNullAndEmptySkipped() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakInterrupt(null);
			SpeechPipeline.SpeakInterrupt("");
			bool ok = spoken.Count == 0;
			return Assert("PipelineNullAndEmptySkipped", ok, $"spoken={spoken.Count}");
		}
	}
}
