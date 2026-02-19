using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OniAccess.Handlers;
using OniAccess.Handlers.Tiles;
using OniAccess.Speech;
using OniAccess.Util;

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
			// Replace Unity time source to avoid native calls in tests
			SpeechPipeline.TimeSource = () => 0f;

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

			// --- CollectHelpEntries (6 new) ---
			results.Add(CollectHelpEntriesEmptyStack());
			results.Add(CollectHelpEntriesSingleHandler());
			results.Add(CollectHelpEntriesTwoNonCapturing());
			results.Add(CollectHelpEntriesBarrierStopsWalk());
			results.Add(CollectHelpEntriesKeyDedup());
			results.Add(CollectHelpEntriesBarrierInclusive());

			// --- Chain-of-responsibility dispatch (8 new) ---
			results.Add(GetAtReturnsNullForOutOfRange());
			results.Add(GetAtReturnsCorrectHandler());
			results.Add(TickWalksStackToBarrier());
			results.Add(TickStopsAtCapturesAllHandler());
			results.Add(TickReachesBottomWhenNoBarrier());
			results.Add(TickSkipsBelowBarrier());
			results.Add(HandleKeyDownStopsOnConsume());
			results.Add(HandleKeyDownBarrierBlocks());

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
			results.Add(MatchTierStartWholeWord());
			results.Add(MatchTierStartPrefix());
			results.Add(MatchTierMidWholeWord());
			results.Add(MatchTierMidPrefix());
			results.Add(MatchTierSubstring());
			results.Add(MatchTierNoMatch());
			results.Add(SearchTierOrdering());

			// --- TextFilter (12 ported + 4 new edge cases) ---
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

			// --- Log class (5 new) ---
			results.Add(LogDebugFormat());
			results.Add(LogInfoFormat());
			results.Add(LogWarnRoutesToWarnFn());
			results.Add(LogErrorRoutesToErrorFn());
			results.Add(LogBackendSwapWorks());

			// --- HandlerStack diagnostic quality (4 new) ---
			results.Add(PushFailureLogsHandlerNameAndException());
			results.Add(PushNullLogsWarning());
			results.Add(PopOnEmptyLogsWarning());
			results.Add(ReplaceFailureLogsHandlerNameAndException());

			HandlerStack.Clear();

			// --- TooltipCapture (8 new) ---
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

			// --- SpeechPipeline (7 new) ---
			results.Add(PipelineDisabledSkipsSpeech());
			results.Add(PipelineEnabledSpeaks());
			results.Add(PipelineFiltersBeforeSpeaking());
			results.Add(PipelineDeduplicatesSameText());
			results.Add(PipelineAllowsSameTextAfterWindow());
			results.Add(PipelineAllowsDifferentTextImmediately());
			results.Add(PipelineNullAndEmptySkipped());

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
		// HandlerStack tests (existing)
		// ========================================

		private static (string, bool, string) ActiveHandlerIsTop() {
			Reset();
			var first = new TestHandler("First");
			var second = new TestHandler("Second");
			HandlerStack.Push(first);
			HandlerStack.Push(second);

			bool ok = HandlerStack.ActiveHandler == second;
			return Assert("ActiveHandlerIsTop", ok,
				$"expected Second, got {HandlerStack.ActiveHandler?.DisplayName ?? "null"}");
		}

		private static (string, bool, string) CapturesAllInputReadable() {
			Reset();
			var handler = new TestHandler("Modal", capturesAll: true);
			HandlerStack.Push(handler);

			bool ok = HandlerStack.ActiveHandler.CapturesAllInput;
			return Assert("CapturesAllInputReadable", ok, "CapturesAllInput was false");
		}

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

		private static (string, bool, string) ClearEmptiesStack() {
			Reset();
			HandlerStack.Push(new TestHandler("A"));
			HandlerStack.Push(new TestHandler("B"));
			HandlerStack.Clear();

			bool ok = HandlerStack.Count == 0 && HandlerStack.ActiveHandler == null;
			return Assert("ClearEmptiesStack", ok,
				$"count={HandlerStack.Count}");
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
		// HandlerStack edge cases (new)
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

		private static (string, bool, string) PushNullIgnored() {
			Reset();
			HandlerStack.Push(new TestHandler("Base"));
			int before = HandlerStack.Count;

			try {
				HandlerStack.Push(null);
			} catch (Exception ex) {
				return Assert("PushNullIgnored", false, $"threw {ex.GetType().Name}");
			}

			bool ok = HandlerStack.Count == before;
			return Assert("PushNullIgnored", ok,
				$"count changed from {before} to {HandlerStack.Count}");
		}

		private static (string, bool, string) ReplaceNullIgnored() {
			Reset();
			var existing = new TestHandler("Existing");
			HandlerStack.Push(existing);
			int before = HandlerStack.Count;

			try {
				HandlerStack.Replace(null);
			} catch (Exception ex) {
				return Assert("ReplaceNullIgnored", false, $"threw {ex.GetType().Name}");
			}

			bool ok = HandlerStack.Count == before
				   && HandlerStack.ActiveHandler == existing;
			return Assert("ReplaceNullIgnored", ok,
				$"count={HandlerStack.Count}, active={HandlerStack.ActiveHandler?.DisplayName ?? "null"}");
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
		// Chain-of-responsibility dispatch tests (new)
		// ========================================

		/// <summary>
		/// Simulate Tick dispatch: walk stack top-to-bottom, tick each handler,
		/// stop after any CapturesAllInput barrier (inclusive).
		/// </summary>
		private static void SimulateTick(List<TestHandler> handlers) {
			int count = handlers.Count;
			for (int i = count - 1; i >= 0; i--) {
				handlers[i].Tick();
				if (handlers[i].CapturesAllInput) break;
			}
		}

		/// <summary>
		/// Simulate HandleKeyDown dispatch: walk stack top-to-bottom, call
		/// HandleKeyDown on each handler, stop when one consumes or at barrier.
		/// </summary>
		private static void SimulateHandleKeyDown(List<TestHandler> handlers) {
			int count = handlers.Count;
			for (int i = count - 1; i >= 0; i--) {
				if (handlers[i].HandleKeyDown(null)) return;
				if (handlers[i].CapturesAllInput) return;
			}
		}

		private static (string, bool, string) GetAtReturnsNullForOutOfRange() {
			Reset();
			var a = new TestHandler("A");
			var b = new TestHandler("B");
			HandlerStack.Push(a);
			HandlerStack.Push(b);

			bool ok = HandlerStack.GetAt(-1) == null && HandlerStack.GetAt(2) == null;
			return Assert("GetAtReturnsNullForOutOfRange", ok,
				$"GetAt(-1)={HandlerStack.GetAt(-1)?.DisplayName ?? "null"}, " +
				$"GetAt(2)={HandlerStack.GetAt(2)?.DisplayName ?? "null"}");
		}

		private static (string, bool, string) GetAtReturnsCorrectHandler() {
			Reset();
			var a = new TestHandler("A");
			var b = new TestHandler("B");
			HandlerStack.Push(a);
			HandlerStack.Push(b);

			bool ok = HandlerStack.GetAt(0) == a && HandlerStack.GetAt(1) == b;
			return Assert("GetAtReturnsCorrectHandler", ok,
				$"GetAt(0)={HandlerStack.GetAt(0)?.DisplayName ?? "null"}, " +
				$"GetAt(1)={HandlerStack.GetAt(1)?.DisplayName ?? "null"}");
		}

		private static (string, bool, string) TickWalksStackToBarrier() {
			// [A(false), B(true), C(true)] — only C ticked (top barrier)
			var handlers = new List<TestHandler> {
				new TestHandler("A", capturesAll: false),
				new TestHandler("B", capturesAll: true),
				new TestHandler("C", capturesAll: true),
			};
			SimulateTick(handlers);

			bool ok = handlers[0].TickCount == 0
				   && handlers[1].TickCount == 0
				   && handlers[2].TickCount == 1;
			return Assert("TickWalksStackToBarrier", ok,
				$"A={handlers[0].TickCount}, B={handlers[1].TickCount}, C={handlers[2].TickCount}");
		}

		private static (string, bool, string) TickStopsAtCapturesAllHandler() {
			// [A(false), B(true)] — B ticked, A not
			var handlers = new List<TestHandler> {
				new TestHandler("A", capturesAll: false),
				new TestHandler("B", capturesAll: true),
			};
			SimulateTick(handlers);

			bool ok = handlers[0].TickCount == 0 && handlers[1].TickCount == 1;
			return Assert("TickStopsAtCapturesAllHandler", ok,
				$"A={handlers[0].TickCount}, B={handlers[1].TickCount}");
		}

		private static (string, bool, string) TickReachesBottomWhenNoBarrier() {
			// [A(false), B(false)] — both ticked
			var handlers = new List<TestHandler> {
				new TestHandler("A", capturesAll: false),
				new TestHandler("B", capturesAll: false),
			};
			SimulateTick(handlers);

			bool ok = handlers[0].TickCount == 1 && handlers[1].TickCount == 1;
			return Assert("TickReachesBottomWhenNoBarrier", ok,
				$"A={handlers[0].TickCount}, B={handlers[1].TickCount}");
		}

		private static (string, bool, string) TickSkipsBelowBarrier() {
			// [A(false), B(true), C(false)] — C and B ticked, A not
			var handlers = new List<TestHandler> {
				new TestHandler("A", capturesAll: false),
				new TestHandler("B", capturesAll: true),
				new TestHandler("C", capturesAll: false),
			};
			SimulateTick(handlers);

			bool ok = handlers[0].TickCount == 0
				   && handlers[1].TickCount == 1
				   && handlers[2].TickCount == 1;
			return Assert("TickSkipsBelowBarrier", ok,
				$"A={handlers[0].TickCount}, B={handlers[1].TickCount}, C={handlers[2].TickCount}");
		}

		private static (string, bool, string) HandleKeyDownStopsOnConsume() {
			// [A(false), B(false,consumes), C(false)] — C and B called, A not
			var handlers = new List<TestHandler> {
				new TestHandler("A", capturesAll: false),
				new TestHandler("B", capturesAll: false) { ConsumeKeyDown = true },
				new TestHandler("C", capturesAll: false),
			};
			SimulateHandleKeyDown(handlers);

			bool ok = handlers[0].HandleKeyDownCount == 0
				   && handlers[1].HandleKeyDownCount == 1
				   && handlers[2].HandleKeyDownCount == 1;
			return Assert("HandleKeyDownStopsOnConsume", ok,
				$"A={handlers[0].HandleKeyDownCount}, B={handlers[1].HandleKeyDownCount}, " +
				$"C={handlers[2].HandleKeyDownCount}");
		}

		private static (string, bool, string) HandleKeyDownBarrierBlocks() {
			// [A(false), B(true,no-consume)] — B called, A not
			var handlers = new List<TestHandler> {
				new TestHandler("A", capturesAll: false),
				new TestHandler("B", capturesAll: true),
			};
			SimulateHandleKeyDown(handlers);

			bool ok = handlers[0].HandleKeyDownCount == 0
				   && handlers[1].HandleKeyDownCount == 1;
			return Assert("HandleKeyDownBarrierBlocks", ok,
				$"A={handlers[0].HandleKeyDownCount}, B={handlers[1].HandleKeyDownCount}");
		}

		// ========================================
		// TypeAheadSearch tests (new)
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

		private static (string, bool, string) SearchNullLabelsSkipped() {
			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('a');
			search.Search(SearchItems.Length, NameByIndex);

			// null at index 5 and "" at index 6 are skipped; Banana matches as substring
			bool ok = search.ResultCount == 3 && search.SelectedOriginalIndex == 0;
			return Assert("SearchNullLabelsSkipped", ok,
				$"ResultCount={search.ResultCount}, SelectedOriginalIndex={search.SelectedOriginalIndex}");
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

		private static (string, bool, string) SearchClearResetsState() {
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

		private static (string, bool, string) SearchEmptyAfterClear() {
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

		private static (string, bool, string) LogDebugFormat() {
			using (var capture = new LogCapture()) {
				Log.Debug("msg");
				bool ok = capture.LogMessages.Count == 1
					   && capture.LogMessages[0] == "[OniAccess] [DEBUG] msg";
				return Assert("LogDebugFormat", ok,
					$"got \"{(capture.LogMessages.Count > 0 ? capture.LogMessages[0] : "<none>")}\"");
			}
		}

		private static (string, bool, string) LogInfoFormat() {
			using (var capture = new LogCapture()) {
				Log.Info("msg");
				bool ok = capture.LogMessages.Count == 1
					   && capture.LogMessages[0] == "[OniAccess] msg";
				return Assert("LogInfoFormat", ok,
					$"got \"{(capture.LogMessages.Count > 0 ? capture.LogMessages[0] : "<none>")}\"");
			}
		}

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
