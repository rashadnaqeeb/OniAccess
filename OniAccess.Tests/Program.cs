using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OniAccess.Handlers;
using OniAccess.Handlers.Build;
using OniAccess.Handlers.Notifications;
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
			SpeechPipeline.SpeakAction = (text, intr) => { };
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
			results.Add(SearchRepeatLetterSkipsSubstring());
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

			// --- ScannerSearch ---
			results.Add(ScannerSearchPrefixMatch());
			results.Add(ScannerSearchWholeWordMatch());
			results.Add(ScannerSearchWordStartMatch());
			results.Add(ScannerSearchNoMatch());
			results.Add(ScannerSearchCaseInsensitive());
			results.Add(ScannerSearchFilterRemapsCategory());
			results.Add(ScannerSearchBestMatchAcrossPositions());

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
			results.Add(TextFilterStripsControlChars());
			results.Add(TextFilterCombinedMarkup());

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
			results.Add(PipelineInterruptFlagIsTrue());
			results.Add(PipelineQueuedSpeaksWithoutInterrupt());
			results.Add(PipelineQueuedNotDeduplicated());
			results.Add(PipelineInterruptDedupeDoesNotAffectQueued());

			// --- ColorNameUtil ---
			results.Add(ColorNameUtilAllPaletteColorsMapped());
			results.Add(ColorNameUtilNoDuplicateNames());
			results.Add(ColorNameUtilUnknownColorReturnsNull());

			// --- ScannerSnapshot ---
			results.Add(RemoveInstanceKeepsStructure());
			results.Add(RemoveLastInstancePrunesBothSubcategories());
			results.Add(PruneEmptySubcategory());
			results.Add(FullCascadePrunesCategory());

			// --- WrapSkipEmpty ---
			results.Add(WrapSkipEmptyForwardWrap());
			results.Add(WrapSkipEmptyBackwardWrap());
			results.Add(WrapSkipEmptyAllEmptyReturnsCurrent());
			results.Add(WrapSkipEmptySingleNonEmpty());

			// --- ScannerTaxonomy ---
			results.Add(TaxonomyAllCategoriesHaveSubcategories());
			results.Add(TaxonomySortIndicesRoundTrip());

			// --- GlanceComposer ---
			results.Add(ComposerThrowingSectionDoesNotAbortOthers());
			results.Add(ComposerAllEmptyReturnsNull());

			// --- AnnouncementFormatter ---
			results.Add(FormatDistanceSameCellReturnsEmpty());
			results.Add(FormatDistanceVerticalOnly());
			results.Add(FormatDistanceHorizontalOnly());
			results.Add(FormatDistanceBothAxes());
			results.Add(FormatClusterSingleDelegatesToEntity());
			results.Add(FormatClusterMultiIncludesCount());

			// --- BuildMenuData ---
			results.Add(OrientationNameCoversAllKnownValues());
			results.Add(OrientationNameDefaultReturnsUp());

			// --- CleanTooltipEntry ---
			results.Add(CleanTooltipSingleNewline());
			results.Add(CleanTooltipTripleNewlineCollapses());
			results.Add(CleanTooltipBulletWithSpace());
			results.Add(CleanTooltipBulletWithSurroundingSpaces());
			results.Add(CleanTooltipLeadingWhitespaceTrimmed());
			results.Add(CleanTooltipDoubledPeriodCollapsed());
			results.Add(CleanTooltipNullAndEmptyPassthrough());

			// --- AppendTooltip ---
			results.Add(AppendTooltipNullReturnsSpeech());
			results.Add(AppendTooltipEmptySpeechReturnsTooltip());
			results.Add(AppendTooltipDuplicateSegmentSuppressed());
			results.Add(AppendTooltipNonMatchingAppends());
			results.Add(AppendTooltipSubstringNotSuppressed());
			results.Add(AppendTooltipSingleSegmentDuplicate());

			// --- NavigableGraph ---
			results.Add(GraphNavigateDownSetsSiblingContext());
			results.Add(GraphNavigateUpAtRootEstablishesRootContext());
			results.Add(GraphNavigateUpToRootUsesRoots());
			results.Add(GraphCycleSiblingWrapForward());
			results.Add(GraphCycleSiblingWrapBackward());
			results.Add(GraphCycleSiblingNoWrap());
			results.Add(GraphMoveToClearsSiblingContext());
			results.Add(GraphMoveToWithSiblingsPreservesContext());
			results.Add(GraphIndexOfFallbackReturnsZero());
			results.Add(GraphSingleSiblingReturnsNull());

			// --- NotificationAnnouncer ---
			results.Add(AnnouncerLoadPhaseHoldsUntilUnpause());
			results.Add(AnnouncerFirstFlushUsesLongWindow());
			results.Add(AnnouncerSubsequentFlushUsesShortWindow());
			results.Add(AnnouncerCountDeltaOnlyAnnouncesIncreases());
			results.Add(AnnouncerStaleKeyCleanupAllowsReannouncement());
			results.Add(AnnouncerFirstGroupInterruptsRestQueue());
			results.Add(AnnouncerBatchWindowResetsOnNewArrival());

			// --- GridUtil.ValidateCluster ---
			results.Add(ValidateClusterAllPrunedReturnsFalse());
			results.Add(ValidateClusterClosestCellSelected());
			results.Add(ValidateClusterStaleCellsRemoved());
			results.Add(ValidateClusterSingleSurvivor());

			// --- CursorBookmarks.DigitKeyToIndex ---
			results.Add(DigitKeyAlpha1Through9());
			results.Add(DigitKeyAlpha0MapsToNine());
			results.Add(DigitKeyNonDigitReturnsNegativeOne());

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

			public bool Tick() { TickCount++; return false; }
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

			public bool Tick() => false;
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
				   && HandlerStack.ActiveHandler == bottom
				   && bottom.ActivateCount == 2;
			return Assert("ExceptionInOnDeactivateDoesNotCorruptStack", ok,
				$"count={HandlerStack.Count}, " +
				$"active={HandlerStack.ActiveHandler?.DisplayName ?? "null"}, " +
				$"activateCount={bottom.ActivateCount}");
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

			bool ok = firstIdx == 2 && secondIdx == 3
				   && search.Buffer == "b";
			return Assert("SearchRepeatLetterCycles", ok,
				$"first={firstIdx}, second={secondIdx}, buffer=\"{search.Buffer}\"");
		}

		private static (string, bool, string) SearchRepeatLetterSkipsSubstring() {
			// 'a' matches Apple(0,tier1), Apricot(1,tier1), Banana(2,tier4 substring).
			// Cycling should stay within Apple/Apricot, never reach Banana.
			var search = new TypeAheadSearch(() => 0f);
			search.AddChar('a');
			search.Search(SearchItems.Length, NameByIndex);
			int first = search.SelectedOriginalIndex; // Apple(0)

			search.AddChar('a'); // cycle -> Apricot
			search.Search(SearchItems.Length, NameByIndex);
			int second = search.SelectedOriginalIndex; // Apricot(1)

			search.AddChar('a'); // cycle wraps -> Apple
			search.Search(SearchItems.Length, NameByIndex);
			int third = search.SelectedOriginalIndex; // Apple(0), not Banana

			bool ok = first == 0 && second == 1 && third == 0;
			return Assert("SearchRepeatLetterSkipsSubstring", ok,
				$"first={first}, second={second}, third={third}");
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
		// ScannerSearch tests
		// ========================================

		private static (string, bool, string) ScannerSearchPrefixMatch() {
			int key = ScannerSearch.MatchSortKey("Sandstone", "sand");
			bool ok = key == 0;
			return Assert("ScannerSearchPrefixMatch", ok, $"key={key}");
		}

		private static (string, bool, string) ScannerSearchWholeWordMatch() {
			int key = ScannerSearch.MatchSortKey("Polluted Water", "water");
			bool ok = key == 1;
			return Assert("ScannerSearchWholeWordMatch", ok, $"key={key}");
		}

		private static (string, bool, string) ScannerSearchWordStartMatch() {
			int key = ScannerSearch.MatchSortKey("Coal Generator", "gen");
			bool ok = key == 2;
			return Assert("ScannerSearchWordStartMatch", ok, $"key={key}");
		}

		private static (string, bool, string) ScannerSearchNoMatch() {
			int key = ScannerSearch.MatchSortKey("Sandstone", "iron");
			bool ok = key == -1;
			return Assert("ScannerSearchNoMatch", ok, $"key={key}");
		}

		private static (string, bool, string) ScannerSearchCaseInsensitive() {
			int key = ScannerSearch.MatchSortKey("Sandstone", "SAND");
			bool ok = key == 0;
			return Assert("ScannerSearchCaseInsensitive", ok, $"key={key}");
		}

		private static (string, bool, string) ScannerSearchFilterRemapsCategory() {
			var entries = new List<ScanEntry> {
				new ScanEntry { Cell = 0, ItemName = "Sandstone", Category = "Solids", Subcategory = "Stone" },
				new ScanEntry { Cell = 1, ItemName = "Iron Ore", Category = "Solids", Subcategory = "Ores" },
			};
			var results = ScannerSearch.Filter(entries, "sand");
			bool ok = results.Count == 1
				&& results[0].Category == (string)STRINGS.ONIACCESS.SCANNER.CATEGORIES.SEARCH
				&& results[0].Subcategory == "Solids"
				&& results[0].ItemName == "Sandstone"
				&& results[0].SortKey == 0;
			return Assert("ScannerSearchFilterRemapsCategory", ok,
				$"count={results.Count}");
		}

		private static (string, bool, string) ScannerSearchBestMatchAcrossPositions() {
			// "gen" in "X Generators Gen" should find whole-word "Gen" (key=1),
			// not stop at word-start "Generators" (key=2)
			int key = ScannerSearch.MatchSortKey("X Generators Gen", "gen");
			bool ok = key == 1;
			return Assert("ScannerSearchBestMatchAcrossPositions", ok, $"key={key}");
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

		private static (string, bool, string) TextFilterStripsControlChars() {
			string result = TextFilter.FilterForSpeech("Hello\x00World");
			bool ok = result == "HelloWorld";
			return Assert("TextFilterStripsControlChars", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) TextFilterCombinedMarkup() {
			string result = TextFilter.FilterForSpeech(
				"<sprite name=warning><b><color=red>Overheat [45%]</color></b> {Hotkey}Ctrl+X");
			bool ok = result == "warning: Overheat 45%";
			return Assert("TextFilterCombinedMarkup", ok, $"got \"{result}\"");
		}

		// ========================================
		// LogCapture helper
		// ========================================

		private class LogCapture: IDisposable {
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

		// ========================================
		// ScannerSnapshot tests
		// ========================================

		private static (ScannerSnapshot snapshot, ScannerItem item, ScanEntry e1, ScanEntry e2)
				BuildTwoInstanceSnapshot() {
			var snapshot = new ScannerSnapshot(new List<ScanEntry>(), 0);
			var e1 = new ScanEntry { Cell = 1, Category = "Cat", Subcategory = "Sub", ItemName = "Item" };
			var e2 = new ScanEntry { Cell = 2, Category = "Cat", Subcategory = "Sub", ItemName = "Item" };
			var item = new ScannerItem { ItemName = "Item", Instances = new List<ScanEntry> { e1, e2 } };
			var allSub = new ScannerSubcategory { Name = "All", Items = new List<ScannerItem> { item } };
			var namedSub = new ScannerSubcategory { Name = "Sub", Items = new List<ScannerItem> { item } };
			var cat = new ScannerCategory {
				Name = "Cat",
				Subcategories = new List<ScannerSubcategory> { allSub, namedSub }
			};
			snapshot.Categories.Add(cat);
			return (snapshot, item, e1, e2);
		}

		private static (string, bool, string) RemoveInstanceKeepsStructure() {
			var (snapshot, item, e1, _) = BuildTwoInstanceSnapshot();
			snapshot.RemoveInstance(item, e1);
			bool ok = item.Instances.Count == 1
				   && snapshot.CategoryCount == 1
				   && snapshot.GetCategory(0).Subcategories.Count == 2;
			return Assert("RemoveInstanceKeepsStructure", ok,
				$"instances={item.Instances.Count}, cats={snapshot.CategoryCount}, " +
				$"subs={snapshot.GetCategory(0).Subcategories.Count}");
		}

		private static (string, bool, string) RemoveLastInstancePrunesBothSubcategories() {
			var (snapshot, item, e1, e2) = BuildTwoInstanceSnapshot();
			snapshot.RemoveInstance(item, e1);
			snapshot.RemoveInstance(item, e2);
			bool ok = snapshot.CategoryCount == 0;
			return Assert("RemoveLastInstancePrunesBothSubcategories", ok,
				$"cats={snapshot.CategoryCount}");
		}

		private static (string, bool, string) PruneEmptySubcategory() {
			var snapshot = new ScannerSnapshot(new List<ScanEntry>(), 0);
			var e1 = new ScanEntry { Cell = 1, Category = "Cat", Subcategory = "SubA", ItemName = "ItemA" };
			var itemA = new ScannerItem { ItemName = "ItemA", Instances = new List<ScanEntry> { e1 } };
			var e2 = new ScanEntry { Cell = 2, Category = "Cat", Subcategory = "SubB", ItemName = "ItemB" };
			var itemB = new ScannerItem { ItemName = "ItemB", Instances = new List<ScanEntry> { e2 } };
			var allSub = new ScannerSubcategory { Name = "All", Items = new List<ScannerItem> { itemA, itemB } };
			var subA = new ScannerSubcategory { Name = "SubA", Items = new List<ScannerItem> { itemA } };
			var subB = new ScannerSubcategory { Name = "SubB", Items = new List<ScannerItem> { itemB } };
			var cat = new ScannerCategory {
				Name = "Cat",
				Subcategories = new List<ScannerSubcategory> { allSub, subA, subB }
			};
			snapshot.Categories.Add(cat);

			snapshot.RemoveInstance(itemA, e1);
			bool ok = snapshot.CategoryCount == 1
				   && cat.Subcategories.Count == 2
				   && cat.Subcategories[0].Name == "All"
				   && cat.Subcategories[0].Items.Count == 1
				   && cat.Subcategories[1].Name == "SubB";
			return Assert("PruneEmptySubcategory", ok,
				$"cats={snapshot.CategoryCount}, subs={cat.Subcategories.Count}, " +
				$"allItems={cat.Subcategories[0].Items.Count}");
		}

		private static (string, bool, string) FullCascadePrunesCategory() {
			var snapshot = new ScannerSnapshot(new List<ScanEntry>(), 0);
			var e1 = new ScanEntry { Cell = 1, Category = "Cat", Subcategory = "Sub", ItemName = "Item" };
			var item = new ScannerItem { ItemName = "Item", Instances = new List<ScanEntry> { e1 } };
			var allSub = new ScannerSubcategory { Name = "All", Items = new List<ScannerItem> { item } };
			var namedSub = new ScannerSubcategory { Name = "Sub", Items = new List<ScannerItem> { item } };
			var cat = new ScannerCategory {
				Name = "Cat",
				Subcategories = new List<ScannerSubcategory> { allSub, namedSub }
			};
			snapshot.Categories.Add(cat);

			snapshot.RemoveInstance(item, e1);
			bool ok = snapshot.CategoryCount == 0;
			return Assert("FullCascadePrunesCategory", ok,
				$"cats={snapshot.CategoryCount}");
		}

		// ========================================
		// WrapSkipEmpty tests
		// ========================================

		private static int InvokeWrapSkipEmpty(int current, int direction,
				List<string> list, Func<string, bool> isNonEmpty) {
			var method = typeof(ScannerNavigator).GetMethod("WrapSkipEmpty",
				BindingFlags.NonPublic | BindingFlags.Static);
			var generic = method.MakeGenericMethod(typeof(string));
			return (int)generic.Invoke(null, new object[] { current, direction, list, isNonEmpty });
		}

		private static (string, bool, string) WrapSkipEmptyForwardWrap() {
			var list = new List<string> { "a", "", "", "b", "" };
			int result = InvokeWrapSkipEmpty(3, 1, list, s => s.Length > 0);
			bool ok = result == 0;
			return Assert("WrapSkipEmptyForwardWrap", ok, $"result={result}");
		}

		private static (string, bool, string) WrapSkipEmptyBackwardWrap() {
			var list = new List<string> { "a", "", "", "b", "" };
			int result = InvokeWrapSkipEmpty(0, -1, list, s => s.Length > 0);
			bool ok = result == 3;
			return Assert("WrapSkipEmptyBackwardWrap", ok, $"result={result}");
		}

		private static (string, bool, string) WrapSkipEmptyAllEmptyReturnsCurrent() {
			var list = new List<string> { "", "", "" };
			int result = InvokeWrapSkipEmpty(1, 1, list, s => s.Length > 0);
			bool ok = result == 1;
			return Assert("WrapSkipEmptyAllEmptyReturnsCurrent", ok, $"result={result}");
		}

		private static (string, bool, string) WrapSkipEmptySingleNonEmpty() {
			var list = new List<string> { "", "x", "", "" };
			int fwd = InvokeWrapSkipEmpty(0, 1, list, s => s.Length > 0);
			int bwd = InvokeWrapSkipEmpty(2, -1, list, s => s.Length > 0);
			int self = InvokeWrapSkipEmpty(1, 1, list, s => s.Length > 0);
			bool ok = fwd == 1 && bwd == 1 && self == 1;
			return Assert("WrapSkipEmptySingleNonEmpty", ok,
				$"fwd={fwd}, bwd={bwd}, self={self}");
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

		private static (string, bool, string) PipelineInterruptFlagIsTrue() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakInterrupt("hello");
			bool ok = spoken.Count == 1 && spoken[0].interrupt == true;
			return Assert("PipelineInterruptFlagIsTrue", ok,
				$"spoken={spoken.Count}" + (spoken.Count > 0 ? $", interrupt={spoken[0].interrupt}" : ""));
		}

		private static (string, bool, string) PipelineQueuedSpeaksWithoutInterrupt() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakQueued("hello");
			bool ok = spoken.Count == 1 && spoken[0].text == "hello" && spoken[0].interrupt == false;
			return Assert("PipelineQueuedSpeaksWithoutInterrupt", ok,
				$"spoken={spoken.Count}" + (spoken.Count > 0 ? $", text=\"{spoken[0].text}\", interrupt={spoken[0].interrupt}" : ""));
		}

		private static (string, bool, string) PipelineQueuedNotDeduplicated() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakQueued("hello");
			SpeechPipeline.SpeakQueued("hello");
			bool ok = spoken.Count == 2;
			return Assert("PipelineQueuedNotDeduplicated", ok, $"spoken={spoken.Count}");
		}

		private static (string, bool, string) PipelineInterruptDedupeDoesNotAffectQueued() {
			float fakeTime = 0f;
			var spoken = new List<(string text, bool interrupt)>();
			SpeechPipeline.TimeSource = () => fakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => spoken.Add((text, intr));
			SpeechPipeline.Reset();

			SpeechPipeline.SpeakInterrupt("hello");
			SpeechPipeline.SpeakQueued("hello");
			bool ok = spoken.Count == 2
				&& spoken[0].interrupt == true
				&& spoken[1].interrupt == false;
			return Assert("PipelineInterruptDedupeDoesNotAffectQueued", ok,
				$"spoken={spoken.Count}" + (spoken.Count >= 2 ? $", [0].interrupt={spoken[0].interrupt}, [1].interrupt={spoken[1].interrupt}" : ""));
		}

		// ========================================
		// ScannerTaxonomy tests
		// ========================================

		private static (string, bool, string) TaxonomyAllCategoriesHaveSubcategories() {
			var missing = new List<string>();
			foreach (string cat in ScannerTaxonomy.CategoryOrder) {
				if (!ScannerTaxonomy.SubcategoryOrder.ContainsKey(cat))
					missing.Add(cat);
			}
			// Also check reverse: every SubcategoryOrder key is in CategoryOrder
			var catSet = new HashSet<string>(ScannerTaxonomy.CategoryOrder);
			foreach (string key in ScannerTaxonomy.SubcategoryOrder.Keys) {
				if (!catSet.Contains(key))
					missing.Add($"SubcategoryOrder has orphan: {key}");
			}
			bool ok = missing.Count == 0;
			return Assert("TaxonomyAllCategoriesHaveSubcategories", ok,
				ok ? "all synced" : $"missing: {string.Join(", ", missing)}");
		}

		private static (string, bool, string) TaxonomySortIndicesRoundTrip() {
			var failures = new List<string>();
			for (int i = 0; i < ScannerTaxonomy.CategoryOrder.Length; i++) {
				string cat = ScannerTaxonomy.CategoryOrder[i];
				int idx = ScannerTaxonomy.CategorySortIndex(cat);
				if (idx != i)
					failures.Add($"CategorySortIndex({cat})={idx}, expected {i}");
				if (!ScannerTaxonomy.SubcategoryOrder.TryGetValue(cat, out string[] subs))
					continue;
				for (int j = 0; j < subs.Length; j++) {
					int subIdx = ScannerTaxonomy.SubcategorySortIndex(cat, subs[j]);
					if (subIdx != j)
						failures.Add($"SubcategorySortIndex({cat},{subs[j]})={subIdx}, expected {j}");
				}
			}
			// Unknown category returns int.MaxValue
			int unknown = ScannerTaxonomy.CategorySortIndex("Nonexistent");
			if (unknown != int.MaxValue)
				failures.Add($"Unknown category returned {unknown}, expected int.MaxValue");
			bool ok = failures.Count == 0;
			return Assert("TaxonomySortIndicesRoundTrip", ok,
				ok ? "all correct" : string.Join("; ", failures));
		}

		// ========================================
		// GlanceComposer tests
		// ========================================

		private class StubSection: ICellSection {
			private readonly string[] _tokens;
			public StubSection(params string[] tokens) { _tokens = tokens; }
			public IEnumerable<string> Read(int cell, CellContext ctx) => _tokens;
		}

		private class ThrowingSection: ICellSection {
			public IEnumerable<string> Read(int cell, CellContext ctx) {
				throw new InvalidOperationException("section exploded");
			}
		}

		private static (string, bool, string) ComposerThrowingSectionDoesNotAbortOthers() {
			var sections = new List<ICellSection> {
				new StubSection("alpha"),
				new ThrowingSection(),
				new StubSection("beta"),
			};
			var composer = new GlanceComposer(sections.AsReadOnly());
			string result = composer.Compose(0);
			bool ok = result == "alpha, beta";
			return Assert("ComposerThrowingSectionDoesNotAbortOthers", ok,
				$"got \"{result}\"");
		}

		private static (string, bool, string) ComposerAllEmptyReturnsNull() {
			var sections = new List<ICellSection> {
				new StubSection("", null),
				new StubSection(),
			};
			var composer = new GlanceComposer(sections.AsReadOnly());
			string result = composer.Compose(0);
			bool ok = result == null;
			return Assert("ComposerAllEmptyReturnsNull", ok,
				$"got {(result == null ? "null" : $"\"{result}\"")}");
		}

		// ========================================
		// AnnouncementFormatter tests
		// ========================================

		private static void SetupGrid(int width) {
			Grid.WidthInCells = width;
		}

		private static (string, bool, string) FormatDistanceSameCellReturnsEmpty() {
			SetupGrid(100);
			string result = AnnouncementFormatter.FormatDistance(505, 505);
			bool ok = result == "";
			return Assert("FormatDistanceSameCellReturnsEmpty", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) FormatDistanceVerticalOnly() {
			SetupGrid(100);
			// cell 505 = row 5, col 5; cell 805 = row 8, col 5 -> 3 up
			string result = AnnouncementFormatter.FormatDistance(505, 805);
			bool ok = result.Contains("3") && result.Contains("up")
				&& !result.Contains("left") && !result.Contains("right");
			return Assert("FormatDistanceVerticalOnly", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) FormatDistanceHorizontalOnly() {
			SetupGrid(100);
			// cell 505 = row 5, col 5; cell 502 = row 5, col 2 -> 3 left
			string result = AnnouncementFormatter.FormatDistance(505, 502);
			bool ok = result.Contains("3") && result.Contains("left")
				&& !result.Contains("up") && !result.Contains("down");
			return Assert("FormatDistanceHorizontalOnly", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) FormatDistanceBothAxes() {
			SetupGrid(100);
			// cell 505 = row 5, col 5; cell 208 = row 2, col 8 -> 3 down, 3 right
			string result = AnnouncementFormatter.FormatDistance(505, 208);
			bool ok = result.Contains("3") && result.Contains("down")
				&& result.Contains("right");
			return Assert("FormatDistanceBothAxes", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) FormatClusterSingleDelegatesToEntity() {
			SetupGrid(100);
			string cluster = AnnouncementFormatter.FormatClusterInstance(
				1, "Iron", 505, 505, 1, 5);
			string entity = AnnouncementFormatter.FormatEntityInstance(
				"Iron", 505, 505, 1, 5);
			bool ok = cluster == entity;
			return Assert("FormatClusterSingleDelegatesToEntity", ok,
				$"cluster=\"{cluster}\", entity=\"{entity}\"");
		}

		private static (string, bool, string) FormatClusterMultiIncludesCount() {
			SetupGrid(100);
			string single = AnnouncementFormatter.FormatClusterInstance(
				1, "Iron", 505, 505, 1, 5);
			string multi = AnnouncementFormatter.FormatClusterInstance(
				7, "Iron", 505, 505, 1, 5);
			bool ok = multi.Contains("7") && !single.Contains("7");
			return Assert("FormatClusterMultiIncludesCount", ok,
				$"single=\"{single}\", multi=\"{multi}\"");
		}

		// ========================================
		// BuildMenuData.GetOrientationName tests
		// ========================================

		private static (string, bool, string) OrientationNameCoversAllKnownValues() {
			var failures = new List<string>();
			string neutral = BuildMenuData.GetOrientationName(Orientation.Neutral);
			string r90 = BuildMenuData.GetOrientationName(Orientation.R90);
			string r180 = BuildMenuData.GetOrientationName(Orientation.R180);
			string r270 = BuildMenuData.GetOrientationName(Orientation.R270);
			string flipH = BuildMenuData.GetOrientationName(Orientation.FlipH);
			string flipV = BuildMenuData.GetOrientationName(Orientation.FlipV);

			if (neutral != (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_UP)
				failures.Add($"Neutral=\"{neutral}\"");
			if (r90 != (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_RIGHT)
				failures.Add($"R90=\"{r90}\"");
			if (r180 != (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_DOWN)
				failures.Add($"R180=\"{r180}\"");
			if (r270 != (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_LEFT)
				failures.Add($"R270=\"{r270}\"");
			if (flipH != r270)
				failures.Add($"FlipH=\"{flipH}\" != R270=\"{r270}\"");
			if (flipV != r180)
				failures.Add($"FlipV=\"{flipV}\" != R180=\"{r180}\"");

			bool ok = failures.Count == 0;
			return Assert("OrientationNameCoversAllKnownValues", ok,
				ok ? "all correct" : string.Join("; ", failures));
		}

		private static (string, bool, string) OrientationNameDefaultReturnsUp() {
			string result = BuildMenuData.GetOrientationName((Orientation)99);
			string expected = (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_UP;
			bool ok = result == expected;
			return Assert("OrientationNameDefaultReturnsUp", ok,
				$"got \"{result}\", expected \"{expected}\"");
		}

		// ========================================
		// CleanTooltipEntry
		// ========================================

		private static (string, bool, string) CleanTooltipSingleNewline() {
			string result = WidgetOps.CleanTooltipEntry("a\nb");
			bool ok = result == "a. b";
			return Assert("CleanTooltipSingleNewline", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) CleanTooltipTripleNewlineCollapses() {
			string result = WidgetOps.CleanTooltipEntry("a\n\n\nb");
			bool ok = result == "a. b";
			return Assert("CleanTooltipTripleNewlineCollapses", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) CleanTooltipBulletWithSpace() {
			string result = WidgetOps.CleanTooltipEntry("\u2022 text");
			bool ok = result == ". text";
			return Assert("CleanTooltipBulletWithSpace", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) CleanTooltipBulletWithSurroundingSpaces() {
			string result = WidgetOps.CleanTooltipEntry("x \u2022 y");
			bool ok = result == "x. y";
			return Assert("CleanTooltipBulletWithSurroundingSpaces", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) CleanTooltipLeadingWhitespaceTrimmed() {
			string result = WidgetOps.CleanTooltipEntry("  text");
			bool ok = result == "text";
			return Assert("CleanTooltipLeadingWhitespaceTrimmed", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) CleanTooltipDoubledPeriodCollapsed() {
			string result = WidgetOps.CleanTooltipEntry("a..b");
			bool ok = result == "a.b";
			return Assert("CleanTooltipDoubledPeriodCollapsed", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) CleanTooltipNullAndEmptyPassthrough() {
			string rNull = WidgetOps.CleanTooltipEntry(null);
			string rEmpty = WidgetOps.CleanTooltipEntry("");
			bool ok = rNull == null && rEmpty == "";
			return Assert("CleanTooltipNullAndEmptyPassthrough", ok,
				$"null→\"{rNull}\", empty→\"{rEmpty}\"");
		}

		// ========================================
		// AppendTooltip
		// ========================================

		private static (string, bool, string) AppendTooltipNullReturnsSpeech() {
			string result = WidgetOps.AppendTooltip("x", null);
			bool ok = result == "x";
			return Assert("AppendTooltipNullReturnsSpeech", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) AppendTooltipEmptySpeechReturnsTooltip() {
			string r1 = WidgetOps.AppendTooltip("", "tip");
			string r2 = WidgetOps.AppendTooltip(null, "tip");
			bool ok = r1 == "tip" && r2 == "tip";
			return Assert("AppendTooltipEmptySpeechReturnsTooltip", ok,
				$"empty→\"{r1}\", null→\"{r2}\"");
		}

		private static (string, bool, string) AppendTooltipDuplicateSegmentSuppressed() {
			string result = WidgetOps.AppendTooltip("a, b", "a");
			bool ok = result == "a, b";
			return Assert("AppendTooltipDuplicateSegmentSuppressed", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) AppendTooltipNonMatchingAppends() {
			string result = WidgetOps.AppendTooltip("a, b", "c");
			bool ok = result == "a, b, c";
			return Assert("AppendTooltipNonMatchingAppends", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) AppendTooltipSubstringNotSuppressed() {
			string result = WidgetOps.AppendTooltip("ab, c", "a");
			bool ok = result == "ab, c, a";
			return Assert("AppendTooltipSubstringNotSuppressed", ok, $"got \"{result}\"");
		}

		private static (string, bool, string) AppendTooltipSingleSegmentDuplicate() {
			string result = WidgetOps.AppendTooltip("hello", "hello");
			bool ok = result == "hello";
			return Assert("AppendTooltipSingleSegmentDuplicate", ok, $"got \"{result}\"");
		}

		// ========================================
		// NavigableGraph
		// ========================================

		private static NavigableGraph<string> MakeTestGraph(
				out Dictionary<string, List<string>> children,
				out Dictionary<string, List<string>> parents,
				List<string> roots = null) {
			// Default graph:  A -> [B, C],  B -> [D, E]
			var c = new Dictionary<string, List<string>> {
				["A"] = new List<string> { "B", "C" },
				["B"] = new List<string> { "D", "E" },
			};
			var p = new Dictionary<string, List<string>> {
				["B"] = new List<string> { "A" },
				["C"] = new List<string> { "A" },
				["D"] = new List<string> { "B" },
				["E"] = new List<string> { "B" },
			};
			children = c;
			parents = p;
			return new NavigableGraph<string>(
				node => p.TryGetValue(node, out var list) ? list : (IReadOnlyList<string>)Array.Empty<string>(),
				node => c.TryGetValue(node, out var list) ? list : (IReadOnlyList<string>)Array.Empty<string>(),
				roots != null ? (Func<IReadOnlyList<string>>)(() => roots) : null);
		}

		private static (string, bool, string) GraphNavigateDownSetsSiblingContext() {
			var graph = MakeTestGraph(out _, out _);
			graph.MoveTo("A");
			string down = graph.NavigateDown(); // → B
			string sibling = graph.CycleSibling(1, out _); // → C
			bool ok = down == "B" && sibling == "C";
			return Assert("GraphNavigateDownSetsSiblingContext", ok,
				$"down=\"{down}\", sibling=\"{sibling}\"");
		}

		private static (string, bool, string) GraphNavigateUpAtRootEstablishesRootContext() {
			var roots = new List<string> { "A", "X" };
			var graph = MakeTestGraph(out _, out _, roots);
			graph.MoveTo("A");
			string up = graph.NavigateUp(); // null (already root)
			string sibling = graph.CycleSibling(1, out _); // → X
			bool ok = up == null && sibling == "X";
			return Assert("GraphNavigateUpAtRootEstablishesRootContext", ok,
				$"up=\"{up}\", sibling=\"{sibling}\"");
		}

		private static (string, bool, string) GraphNavigateUpToRootUsesRoots() {
			var roots = new List<string> { "A", "X" };
			var graph = MakeTestGraph(out _, out _, roots);
			graph.MoveTo("B");
			string up = graph.NavigateUp(); // → A (a root node)
			// A is a root, so siblings should be the roots list
			string sibling = graph.CycleSibling(1, out _); // → X
			bool ok = up == "A" && sibling == "X";
			return Assert("GraphNavigateUpToRootUsesRoots", ok,
				$"up=\"{up}\", sibling=\"{sibling}\"");
		}

		private static (string, bool, string) GraphCycleSiblingWrapForward() {
			var graph = MakeTestGraph(out _, out _);
			graph.MoveTo("A");
			graph.NavigateDown(); // → B, siblings = [B, C]
			graph.CycleSibling(1, out _); // → C
			string wrapped = graph.CycleSibling(1, out bool didWrap); // → B (wrap)
			bool ok = wrapped == "B" && didWrap;
			return Assert("GraphCycleSiblingWrapForward", ok,
				$"node=\"{wrapped}\", wrapped={didWrap}");
		}

		private static (string, bool, string) GraphCycleSiblingWrapBackward() {
			var graph = MakeTestGraph(out _, out _);
			graph.MoveTo("A");
			graph.NavigateDown(); // → B, siblings = [B, C]
			string wrapped = graph.CycleSibling(-1, out bool didWrap); // → C (wrap)
			bool ok = wrapped == "C" && didWrap;
			return Assert("GraphCycleSiblingWrapBackward", ok,
				$"node=\"{wrapped}\", wrapped={didWrap}");
		}

		private static (string, bool, string) GraphCycleSiblingNoWrap() {
			var graph = MakeTestGraph(out _, out _);
			graph.MoveTo("A");
			graph.NavigateDown(); // → B, siblings = [B, C]
			string next = graph.CycleSibling(1, out bool didWrap); // → C (no wrap)
			bool ok = next == "C" && !didWrap;
			return Assert("GraphCycleSiblingNoWrap", ok,
				$"node=\"{next}\", wrapped={didWrap}");
		}

		private static (string, bool, string) GraphMoveToClearsSiblingContext() {
			var graph = MakeTestGraph(out _, out _);
			graph.MoveTo("A");
			graph.NavigateDown(); // → B, establishes siblings
			graph.MoveTo("D"); // clears siblings
			string result = graph.CycleSibling(1, out _);
			bool ok = result == null;
			return Assert("GraphMoveToClearsSiblingContext", ok,
				$"CycleSibling returned \"{result}\"");
		}

		private static (string, bool, string) GraphMoveToWithSiblingsPreservesContext() {
			var siblings = new List<string> { "B", "C" };
			var graph = MakeTestGraph(out _, out _);
			graph.MoveToWithSiblings("C", siblings); // index 1
			string prev = graph.CycleSibling(-1, out bool didWrap); // → B (natural backward, no wrap)
			bool ok = prev == "B" && !didWrap;
			return Assert("GraphMoveToWithSiblingsPreservesContext", ok,
				$"node=\"{prev}\", wrapped={didWrap}");
		}

		private static (string, bool, string) GraphIndexOfFallbackReturnsZero() {
			var siblings = new List<string> { "X", "Y" };
			var graph = MakeTestGraph(out _, out _);
			graph.MoveToWithSiblings("Z", siblings); // Z not in list → index 0
			string next = graph.CycleSibling(1, out _); // from index 0 → Y
			bool ok = next == "Y";
			return Assert("GraphIndexOfFallbackReturnsZero", ok,
				$"CycleSibling returned \"{next}\"");
		}

		private static (string, bool, string) GraphSingleSiblingReturnsNull() {
			var c = new Dictionary<string, List<string>> {
				["A"] = new List<string> { "B" },
			};
			var p = new Dictionary<string, List<string>> {
				["B"] = new List<string> { "A" },
			};
			var graph = new NavigableGraph<string>(
				node => p.TryGetValue(node, out var list) ? list : (IReadOnlyList<string>)Array.Empty<string>(),
				node => c.TryGetValue(node, out var list) ? list : (IReadOnlyList<string>)Array.Empty<string>());
			graph.MoveTo("A");
			graph.NavigateDown(); // → B, siblings = [B] (only child)
			string result = graph.CycleSibling(1, out _);
			bool ok = result == null;
			return Assert("GraphSingleSiblingReturnsNull", ok,
				$"CycleSibling returned \"{result}\"");
		}

		// ========================================
		// NotificationAnnouncer
		// ========================================

		private static readonly MethodInfo _trackerAdd = typeof(NotificationTracker)
			.GetMethod("OnNotificationAdded", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo _trackerRemove = typeof(NotificationTracker)
			.GetMethod("OnNotificationRemoved", BindingFlags.Instance | BindingFlags.NonPublic);

		private static void AddNotification(NotificationTracker tracker, string title) {
			_trackerAdd.Invoke(tracker, new object[] {
				new Notification(title, NotificationType.Bad)
			});
		}

		private static void RemoveFirstNotification(NotificationTracker tracker, string title) {
			for (int i = 0; i < tracker.Notifications.Count; i++) {
				if (tracker.Notifications[i].titleText == title) {
					_trackerRemove.Invoke(tracker, new object[] { tracker.Notifications[i] });
					return;
				}
			}
		}

		private class AnnouncerHarness {
			public float FakeTime;
			public bool Paused;
			public List<(string text, bool interrupt)> Spoken;
			public NotificationTracker Tracker;
			public NotificationAnnouncer Announcer;
		}

		private static AnnouncerHarness SetupAnnouncer(bool startPaused = true) {
			var h = new AnnouncerHarness {
				FakeTime = 0f,
				Paused = startPaused,
				Spoken = new List<(string, bool)>(),
				Tracker = new NotificationTracker(),
			};
			NotificationAnnouncer.TimeSource = () => h.FakeTime;
			NotificationAnnouncer.IsPaused = () => h.Paused;
			SpeechPipeline.TimeSource = () => h.FakeTime;
			SpeechPipeline.SpeakAction = (text, intr) => h.Spoken.Add((text, intr));
			SpeechPipeline.Reset();
			h.Announcer = new NotificationAnnouncer(h.Tracker);
			return h;
		}

		private static void CleanupAnnouncer(AnnouncerHarness h) {
			h.Announcer.Detach();
			NotificationAnnouncer.TimeSource = () => 0f;
			NotificationAnnouncer.IsPaused = () => false;
		}

		private static (string, bool, string) AnnouncerLoadPhaseHoldsUntilUnpause() {
			var h = SetupAnnouncer(startPaused: true);
			AddNotification(h.Tracker, "Stress");
			h.FakeTime = 5f;
			h.Announcer.Tick();
			h.Announcer.Tick();
			bool held = h.Spoken.Count == 0;

			// Unpause → load phase ends, batch starts
			h.Paused = false;
			h.Announcer.Tick(); // exits load phase, sets batch pending
			bool stillHeld = h.Spoken.Count == 0; // hasn't flushed yet (window not elapsed)

			h.FakeTime = 6.1f;
			h.Announcer.Tick(); // first-flush window (1.0s) elapsed
			bool spoke = h.Spoken.Count == 1 && h.Spoken[0].text == "Stress";

			CleanupAnnouncer(h);
			bool ok = held && stillHeld && spoke;
			return Assert("AnnouncerLoadPhaseHoldsUntilUnpause", ok,
				$"held={held}, stillHeld={stillHeld}, spoke={spoke}, count={h.Spoken.Count}");
		}

		private static (string, bool, string) AnnouncerFirstFlushUsesLongWindow() {
			var h = SetupAnnouncer(startPaused: false);
			// Tick once to exit load phase
			h.Announcer.Tick();
			h.FakeTime = 1.1f;
			h.Announcer.Tick(); // flush the empty first batch

			h.Spoken.Clear();
			SpeechPipeline.Reset();

			// Now add a notification — should trigger first-flush logic
			// Wait, _firstFlush was set false by the Tick above.
			// Need to rethink: _firstFlush is consumed on the first flush after load.
			// Once the load phase exits, the first Tick sets batchPending.
			// The next Tick after the window flushes and clears _firstFlush.
			// So we need to test that the FIRST flush uses 1.0s, not 0.2s.
			CleanupAnnouncer(h);

			// Restart clean: unpause immediately, add notification, verify 1.0s window
			h = SetupAnnouncer(startPaused: false);
			AddNotification(h.Tracker, "Stress");
			h.Announcer.Tick(); // exits load phase, resets batch start to current time (0)
			bool noSpeechYet = h.Spoken.Count == 0;

			// At 0.5s — within 1.0s first-flush window
			h.FakeTime = 0.5f;
			h.Announcer.Tick();
			bool stillWaiting = h.Spoken.Count == 0;

			// At 1.1s — past 1.0s first-flush window
			h.FakeTime = 1.1f;
			h.Announcer.Tick();
			bool spoke = h.Spoken.Count == 1;

			CleanupAnnouncer(h);
			bool ok = noSpeechYet && stillWaiting && spoke;
			return Assert("AnnouncerFirstFlushUsesLongWindow", ok,
				$"noSpeech={noSpeechYet}, stillWaiting={stillWaiting}, spoke={spoke}");
		}

		private static (string, bool, string) AnnouncerSubsequentFlushUsesShortWindow() {
			var h = SetupAnnouncer(startPaused: false);
			AddNotification(h.Tracker, "Stress");
			h.Announcer.Tick(); // exit load phase
			h.FakeTime = 1.1f;
			h.Announcer.Tick(); // first flush (1.0s window)
			h.Spoken.Clear();
			SpeechPipeline.Reset();

			// Add new notification — subsequent flush should use 0.2s window
			h.FakeTime = 2.0f;
			AddNotification(h.Tracker, "Hunger");
			h.FakeTime = 2.1f;
			h.Announcer.Tick();
			bool tooEarly = h.Spoken.Count == 0; // 0.1s < 0.2s window

			h.FakeTime = 2.3f;
			h.Announcer.Tick();
			bool spoke = h.Spoken.Count > 0;

			CleanupAnnouncer(h);
			bool ok = tooEarly && spoke;
			return Assert("AnnouncerSubsequentFlushUsesShortWindow", ok,
				$"tooEarly={tooEarly}, spoke={spoke}, count={h.Spoken.Count}");
		}

		private static (string, bool, string) AnnouncerCountDeltaOnlyAnnouncesIncreases() {
			var h = SetupAnnouncer(startPaused: false);
			AddNotification(h.Tracker, "Stress");
			h.Announcer.Tick(); // exit load phase
			h.FakeTime = 1.1f;
			h.Announcer.Tick(); // first flush → speaks "Stress"

			// Add second "Stress" → count grows from 1 to 2
			h.Spoken.Clear();
			SpeechPipeline.Reset();
			h.FakeTime = 2.0f;
			AddNotification(h.Tracker, "Stress");
			h.FakeTime = 2.3f;
			h.Announcer.Tick();
			string format = (string)STRINGS.ONIACCESS.NOTIFICATIONS.GROUP_COUNT;
			string expected = string.Format(format, "Stress", 2);
			bool spokeIncrease = h.Spoken.Count == 1 && h.Spoken[0].text == expected;

			// Remove one "Stress" → count drops from 2 to 1. Should be silent.
			h.Spoken.Clear();
			SpeechPipeline.Reset();
			h.FakeTime = 3.0f;
			RemoveFirstNotification(h.Tracker, "Stress");
			h.FakeTime = 3.3f;
			h.Announcer.Tick();
			bool silentDecrease = h.Spoken.Count == 0;

			CleanupAnnouncer(h);
			bool ok = spokeIncrease && silentDecrease;
			return Assert("AnnouncerCountDeltaOnlyAnnouncesIncreases", ok,
				$"spokeIncrease={spokeIncrease}, silentDecrease={silentDecrease}");
		}

		private static (string, bool, string) AnnouncerStaleKeyCleanupAllowsReannouncement() {
			var h = SetupAnnouncer(startPaused: false);
			AddNotification(h.Tracker, "Stress");
			h.Announcer.Tick(); // exit load phase
			h.FakeTime = 1.1f;
			h.Announcer.Tick(); // first flush → speaks "Stress", _knownCounts["Stress"]=1

			// Remove "Stress" and add a different notification to trigger a flush
			// while "Stress" is absent — this prunes the stale key.
			h.FakeTime = 2.0f;
			RemoveFirstNotification(h.Tracker, "Stress");
			AddNotification(h.Tracker, "Hunger"); // triggers OnChanged with HasNew
			h.FakeTime = 2.3f;
			h.Announcer.Tick(); // flushes: announces Hunger, prunes "Stress" from _knownCounts

			// Re-add "Stress" — should announce again since key was pruned
			h.Spoken.Clear();
			SpeechPipeline.Reset();
			h.FakeTime = 3.0f;
			AddNotification(h.Tracker, "Stress");
			h.FakeTime = 3.3f;
			h.Announcer.Tick();

			bool reannounced = false;
			for (int i = 0; i < h.Spoken.Count; i++) {
				if (h.Spoken[i].text == "Stress") { reannounced = true; break; }
			}

			CleanupAnnouncer(h);
			return Assert("AnnouncerStaleKeyCleanupAllowsReannouncement", reannounced,
				$"count={h.Spoken.Count}" +
				(h.Spoken.Count > 0 ? $", texts=[{string.Join(", ", h.Spoken.ConvertAll(s => s.text))}]" : ""));
		}

		private static (string, bool, string) AnnouncerFirstGroupInterruptsRestQueue() {
			var h = SetupAnnouncer(startPaused: false);
			AddNotification(h.Tracker, "Stress");
			AddNotification(h.Tracker, "Hunger");
			h.Announcer.Tick(); // exit load phase
			h.FakeTime = 1.1f;
			h.Announcer.Tick(); // first flush

			bool firstInterrupt = h.Spoken.Count >= 1 && h.Spoken[0].interrupt;
			bool secondQueued = h.Spoken.Count >= 2 && !h.Spoken[1].interrupt;
			bool twoTotal = h.Spoken.Count == 2;

			CleanupAnnouncer(h);
			bool ok = firstInterrupt && secondQueued && twoTotal;
			return Assert("AnnouncerFirstGroupInterruptsRestQueue", ok,
				$"count={h.Spoken.Count}, firstInterrupt={firstInterrupt}, secondQueued={secondQueued}");
		}

		private static (string, bool, string) AnnouncerBatchWindowResetsOnNewArrival() {
			var h = SetupAnnouncer(startPaused: false);
			h.Announcer.Tick(); // exit load phase
			h.FakeTime = 1.1f;
			h.Announcer.Tick(); // flush empty first batch
			h.Spoken.Clear();
			SpeechPipeline.Reset();

			// Add notification at t=2.0
			h.FakeTime = 2.0f;
			AddNotification(h.Tracker, "Stress");

			// Add another at t=2.15 — resets the batch timer
			h.FakeTime = 2.15f;
			AddNotification(h.Tracker, "Hunger");

			// At t=2.25: only 0.1s since last arrival (2.15), within 0.2s window
			h.FakeTime = 2.25f;
			h.Announcer.Tick();
			bool tooEarly = h.Spoken.Count == 0;

			// At t=2.4: 0.25s since last arrival (2.15), past 0.2s window
			h.FakeTime = 2.4f;
			h.Announcer.Tick();
			bool spoke = h.Spoken.Count == 2;

			CleanupAnnouncer(h);
			bool ok = tooEarly && spoke;
			return Assert("AnnouncerBatchWindowResetsOnNewArrival", ok,
				$"tooEarly={tooEarly}, spoke={spoke}, count={h.Spoken.Count}");
		}

		// ========================================
		// GridUtil.ValidateCluster
		// ========================================

		private static (string, bool, string) ValidateClusterAllPrunedReturnsFalse() {
			SetupGrid(100);
			var entry = new ScanEntry { Cell = 505 };
			var cells = new List<int> { 10, 20, 30 };
			bool result = GridUtil.ValidateCluster(cells, 505, entry, cell => false);
			bool ok = !result && cells.Count == 0 && entry.Cell == 505;
			return Assert("ValidateClusterAllPrunedReturnsFalse", ok,
				$"result={result}, cells={cells.Count}, entry.Cell={entry.Cell}");
		}

		private static (string, bool, string) ValidateClusterClosestCellSelected() {
			SetupGrid(100);
			// cursor at row 5 col 5 (cell 505)
			// cells at: row 5 col 15 (dist 10), row 5 col 10 (dist 5), row 6 col 20 (dist 16)
			int cursor = 505;
			var cells = new List<int> { 515, 510, 620 };
			var entry = new ScanEntry { Cell = 0 };
			bool result = GridUtil.ValidateCluster(cells, cursor, entry, cell => true);
			bool ok = result && entry.Cell == 510;
			return Assert("ValidateClusterClosestCellSelected", ok,
				$"result={result}, entry.Cell={entry.Cell}");
		}

		private static (string, bool, string) ValidateClusterStaleCellsRemoved() {
			SetupGrid(100);
			int cursor = 505;
			// Keep 510 (dist 5) and 520 (dist 15), prune 515
			var cells = new List<int> { 510, 515, 520 };
			var entry = new ScanEntry { Cell = 0 };
			bool result = GridUtil.ValidateCluster(cells, cursor, entry,
				cell => cell != 515);
			bool ok = result && entry.Cell == 510 && cells.Count == 2
				&& !cells.Contains(515);
			return Assert("ValidateClusterStaleCellsRemoved", ok,
				$"result={result}, entry.Cell={entry.Cell}, cells={cells.Count}");
		}

		private static (string, bool, string) ValidateClusterSingleSurvivor() {
			SetupGrid(100);
			int cursor = 505;
			// Only cell 900 survives (far away but only option)
			var cells = new List<int> { 510, 520, 900 };
			var entry = new ScanEntry { Cell = 0 };
			bool result = GridUtil.ValidateCluster(cells, cursor, entry,
				cell => cell == 900);
			bool ok = result && entry.Cell == 900 && cells.Count == 1;
			return Assert("ValidateClusterSingleSurvivor", ok,
				$"result={result}, entry.Cell={entry.Cell}, cells={cells.Count}");
		}

		// ========================================
		// CursorBookmarks.DigitKeyToIndex
		// ========================================

		private static (string, bool, string) DigitKeyAlpha1Through9() {
			var failures = new List<string>();
			for (int i = 0; i < 9; i++) {
				KeyCode key = KeyCode.Alpha1 + i;
				int result = CursorBookmarks.DigitKeyToIndex(key);
				if (result != i)
					failures.Add($"Alpha{i + 1}→{result} (expected {i})");
			}
			bool ok = failures.Count == 0;
			return Assert("DigitKeyAlpha1Through9", ok,
				ok ? "all correct" : string.Join("; ", failures));
		}

		private static (string, bool, string) DigitKeyAlpha0MapsToNine() {
			int result = CursorBookmarks.DigitKeyToIndex(KeyCode.Alpha0);
			bool ok = result == 9;
			return Assert("DigitKeyAlpha0MapsToNine", ok, $"got {result}");
		}

		private static (string, bool, string) DigitKeyNonDigitReturnsNegativeOne() {
			int result = CursorBookmarks.DigitKeyToIndex(KeyCode.A);
			bool ok = result == -1;
			return Assert("DigitKeyNonDigitReturnsNegativeOne", ok, $"got {result}");
		}
	}
}
