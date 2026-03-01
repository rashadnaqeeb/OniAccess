using System;
using System.Collections.Generic;

using OniAccess.Input;
using OniAccess.Speech;

using STRINGS;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for the ReportScreen (daily reports). Standalone NestedMenuHandler.
	///
	/// Level 0: 3 section headers + Colony Summary action
	/// Level 1: Visible stat categories within each section
	/// Level 2: Per-entity context entries, or note breakdowns when no context entries exist
	///
	/// Tab/Shift+Tab cycles between report days.
	/// Data is read live from ReportManager on every call.
	///
	/// Lifecycle: OnShow-patch on ReportScreen.OnShow(bool).
	/// </summary>
	public class ReportScreenHandler: NestedMenuHandler {
		private int _currentDay;

		// Section structure derived from ReportGroups. This is static metadata
		// (the dictionary is initialized once and never modified), so it is
		// built once in OnActivate rather than re-queried per frame.
		private List<SectionInfo> _sections;

		private readonly List<ReportManager.ReportType> _visibleTypesScratch =
			new List<ReportManager.ReportType>();
		private readonly List<ReportManager.ReportEntry.Note> _notesScratch =
			new List<ReportManager.ReportEntry.Note>();

		public ReportScreenHandler(KScreen screen) : base(screen) { }

		public override string DisplayName =>
			(string)STRINGS.ONIACCESS.REPORT.HANDLER_NAME;

		public override bool CapturesAllInput => true;

		protected override int MaxLevel => 2;
		protected override int SearchLevel => 1;
		protected override int StartLevel => 1;

		private static readonly List<HelpEntry> _helpEntries = new List<HelpEntry>(NestedNavHelpEntries) {
			new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.REPORT.HELP_CYCLE),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			_currentDay = ReportManager.Instance.TodaysReport.day;
			BuildSections();
			base.OnActivate();
			// base.OnActivate interrupts with DisplayName; queue title and first item after it
			SpeechPipeline.SpeakQueued(TextFilter.FilterForSpeech(GetCycleTitle()));
			int count = GetItemCount(Level, new int[] {
				GetIndex(0), GetIndex(1), GetIndex(2) });
			if (count > 0) {
				string label = GetItemLabel(Level, new int[] {
					GetIndex(0), GetIndex(1), GetIndex(2) });
				if (!string.IsNullOrWhiteSpace(label))
					SpeechPipeline.SpeakQueued(label);
			}
		}

		// ========================================
		// SECTION / DATA HELPERS
		// ========================================

		private struct SectionInfo {
			public string name;
			public List<ReportManager.ReportType> types;
		}

		private void BuildSections() {
			_sections = new List<SectionInfo>();
			SectionInfo current = default;
			current.types = null;

			foreach (var kvp in ReportManager.Instance.ReportGroups) {
				if (kvp.Value.isHeader) {
					if (current.types != null)
						_sections.Add(current);
					current = new SectionInfo {
						name = kvp.Value.stringKey,
						types = new List<ReportManager.ReportType>()
					};
				} else {
					current.types?.Add(kvp.Key);
				}
			}
			if (current.types != null)
				_sections.Add(current);
		}

		private ReportManager.DailyReport GetReport() {
			return ReportManager.Instance.FindReport(_currentDay)
				?? ReportManager.Instance.TodaysReport;
		}

		private List<ReportManager.ReportType> GetVisibleTypes(int sectionIndex) {
			_visibleTypesScratch.Clear();
			if (sectionIndex < 0 || sectionIndex >= _sections.Count)
				return _visibleTypesScratch;
			var report = GetReport();
			var section = _sections[sectionIndex];
			foreach (var type in section.types) {
				var entry = report.GetEntry(type);
				var group = ReportManager.Instance.ReportGroups[type];
				if (entry.accumulate != 0f || group.reportIfZero)
					_visibleTypesScratch.Add(type);
			}
			return _visibleTypesScratch;
		}

		/// <summary>
		/// Collect and sort notes from an entry into _notesScratch.
		/// </summary>
		private void CollectNotes(ReportManager.ReportEntry entry, ReportManager.ReportGroup group) {
			_notesScratch.Clear();
			entry.IterateNotes(note => _notesScratch.Add(note));
			if (group.posNoteOrder == ReportManager.ReportEntry.Order.Descending)
				_notesScratch.Sort((a, b) => b.value.CompareTo(a.value));
			else if (group.posNoteOrder == ReportManager.ReportEntry.Order.Ascending)
				_notesScratch.Sort((a, b) => a.value.CompareTo(b.value));
		}

		/// <summary>
		/// Level 2 item count: context entries when available, otherwise notes.
		/// </summary>
		private int GetLevel2Count(ReportManager.ReportEntry entry, ReportManager.ReportGroup group) {
			if (entry.contextEntries.Count > 0)
				return entry.contextEntries.Count;
			CollectNotes(entry, group);
			return _notesScratch.Count;
		}

		// ========================================
		// NestedMenuHandler ABSTRACTS
		// ========================================

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0)
				return _sections.Count + 1; // +1 for Colony Summary

			// Colony Summary has no children
			if (indices[0] == _sections.Count)
				return 0;

			if (level == 1)
				return GetVisibleTypes(indices[0]).Count;

			// Level 2: context entries, or notes when no context entries exist
			var types = GetVisibleTypes(indices[0]);
			if (indices[1] < 0 || indices[1] >= types.Count)
				return 0;
			var reportType = types[indices[1]];
			var entry = GetReport().GetEntry(reportType);
			var group = ReportManager.Instance.ReportGroups[reportType];
			return GetLevel2Count(entry, group);
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (level == 0) {
				if (indices[0] == _sections.Count)
					return (string)STRINGS.ONIACCESS.REPORT.COLONY_SUMMARY;
				if (indices[0] < 0 || indices[0] >= _sections.Count)
					return null;
				return _sections[indices[0]].name;
			}

			if (indices[0] < 0 || indices[0] >= _sections.Count)
				return null;

			var types = GetVisibleTypes(indices[0]);
			if (indices[1] < 0 || indices[1] >= types.Count)
				return null;

			var reportType = types[indices[1]];
			var group = ReportManager.Instance.ReportGroups[reportType];
			var entry = GetReport().GetEntry(reportType);

			if (level == 1)
				return BuildStatLabel(entry, group);

			// Level 2: context entry or note fallback
			if (entry.contextEntries.Count > 0) {
				if (indices[2] < 0 || indices[2] >= entry.contextEntries.Count)
					return null;
				return BuildContextLabel(entry.contextEntries[indices[2]], group);
			}
			CollectNotes(entry, group);
			if (indices[2] < 0 || indices[2] >= _notesScratch.Count)
				return null;
			return BuildNoteLabel(_notesScratch[indices[2]], group);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level == 2) {
				// At level 2, parent is the level 1 stat category
				var types = GetVisibleTypes(indices[0]);
				if (indices[1] >= 0 && indices[1] < types.Count) {
					var group = ReportManager.Instance.ReportGroups[types[indices[1]]];
					return group.stringKey;
				}
			}
			if (level == 1 && indices[0] >= 0 && indices[0] < _sections.Count)
				return _sections[indices[0]].name;
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			if (Level == 0 && indices[0] == _sections.Count) {
				ActivateColonySummary();
				return;
			}
		}

		// ========================================
		// SEARCH
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			int count = 0;
			for (int s = 0; s < _sections.Count; s++)
				count += GetVisibleTypes(s).Count;
			return count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			int remaining = flatIndex;
			for (int s = 0; s < _sections.Count; s++) {
				var types = GetVisibleTypes(s);
				if (remaining < types.Count) {
					var group = ReportManager.Instance.ReportGroups[types[remaining]];
					return group.stringKey;
				}
				remaining -= types.Count;
			}
			return null;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			int remaining = flatIndex;
			for (int s = 0; s < _sections.Count; s++) {
				var types = GetVisibleTypes(s);
				if (remaining < types.Count) {
					outIndices[0] = s;
					outIndices[1] = remaining;
					outIndices[2] = 0;
					return;
				}
				remaining -= types.Count;
			}
		}

		// ========================================
		// CYCLE NAVIGATION (Tab/Shift+Tab)
		// ========================================

		protected override void NavigateTabForward() {
			int maxDay = ReportManager.Instance.TodaysReport.day;
			if (_currentDay >= maxDay) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.REPORT.NO_LATER_CYCLE);
				return;
			}
			_currentDay++;
			OnCycleChanged();
		}

		protected override void NavigateTabBackward() {
			if (ReportManager.Instance.FindReport(_currentDay - 1) == null) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.REPORT.NO_EARLIER_CYCLE);
				return;
			}
			_currentDay--;
			OnCycleChanged();
		}

		private void OnCycleChanged() {
			ClampIndices();
			SyncCurrentIndex();
			PlayHoverSound();
			SpeakCycleTitleAndCurrentItem();
		}

		private void ClampIndices() {
			if (Level >= 1) {
				int sectionIdx = GetIndex(0);
				if (sectionIdx >= _sections.Count)
					return; // On Colony Summary, stay there

				var types = GetVisibleTypes(sectionIdx);
				int l1 = GetIndex(1);
				if (l1 >= types.Count)
					SetIndex(1, Math.Max(0, types.Count - 1));

				if (Level >= 2 && types.Count > 0) {
					var reportType = types[GetIndex(1)];
					var entry = GetReport().GetEntry(reportType);
					var group = ReportManager.Instance.ReportGroups[reportType];
					int l2Count = GetLevel2Count(entry, group);
					int l2 = GetIndex(2);
					if (l2 >= l2Count)
						SetIndex(2, Math.Max(0, l2Count - 1));
				}
			}
		}

		private string GetCycleTitle() {
			int todayDay = ReportManager.Instance.TodaysReport.day;
			if (_currentDay == todayDay)
				return string.Format(UI.ENDOFDAYREPORT.DAY_TITLE_TODAY, _currentDay);
			if (_currentDay == todayDay - 1)
				return string.Format(UI.ENDOFDAYREPORT.DAY_TITLE_YESTERDAY, _currentDay);
			return string.Format(UI.ENDOFDAYREPORT.DAY_TITLE, _currentDay);
		}

		/// <summary>
		/// Speak cycle title as interrupt, then queue the current item label
		/// so the item doesn't cut off the title announcement.
		/// </summary>
		private void SpeakCycleTitleAndCurrentItem() {
			SpeechPipeline.SpeakInterrupt(TextFilter.FilterForSpeech(GetCycleTitle()));
			int count = GetItemCount(Level, new int[] {
				GetIndex(0), GetIndex(1), GetIndex(2) });
			if (count > 0) {
				string label = GetItemLabel(Level, new int[] {
					GetIndex(0), GetIndex(1), GetIndex(2) });
				if (!string.IsNullOrWhiteSpace(label))
					SpeechPipeline.SpeakQueued(label);
			}
		}

		// ========================================
		// COLONY SUMMARY
		// ========================================

		private void ActivateColonySummary() {
			try {
				var data = RetireColonyUtility.GetCurrentColonyRetiredColonyData();
				MainMenu.ActivateRetiredColoniesScreenFromData(
					PauseScreen.Instance.transform.parent.gameObject, data);
			} catch (Exception ex) {
				Util.Log.Error($"ReportScreenHandler.ActivateColonySummary failed: {ex.Message}");
				PlayNegativeSound();
			}
		}

		// ========================================
		// LABEL BUILDING
		// ========================================

		/// <summary>
		/// Format a value using the group's formatfn, falling back to ToString
		/// when formatfn is null (critters, chores, level-ups, etc.).
		/// </summary>
		private static string FormatValue(float value, ReportManager.ReportGroup group) {
			return group.formatfn != null ? group.formatfn(value) : value.ToString();
		}

		private string BuildStatLabel(
			ReportManager.ReportEntry entry,
			ReportManager.ReportGroup group) {

			var parts = new List<string>(4);
			parts.Add(group.stringKey);

			// Stats that only accumulate in one direction (critters, time, level-ups,
			// etc.) are totals/counts, not deltas. Show plain values without
			// net/added/removed framing. Only use delta framing when both positive
			// and negative values exist.
			bool isDelta = entry.Positive != 0f && entry.Negative != 0f;

			if (group.groupFormatfn != null) {
				// Mirror game's per-column denominator logic (ReportScreenEntryRow.SetLine).
				// When context entries exist, use their count. Otherwise count notes per sign.
				int ctxCount = entry.contextEntries.Count;
				float addedDenom, removedDenom, netDenom;
				if (ctxCount > 0) {
					addedDenom = removedDenom = netDenom = ctxCount;
				} else {
					int posCount = 0, negCount = 0;
					entry.IterateNotes(note => {
						if (note.value > 0f) posCount++;
						else if (note.value < 0f) negCount++;
					});
					addedDenom = Math.Max(posCount, 1f);
					removedDenom = Math.Max(negCount, 1f);
					netDenom = Math.Max(posCount + negCount, 1f);
				}

				if (isDelta) {
					parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.NET,
						group.groupFormatfn(entry.Net, netDenom)));
					parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.ADDED,
						group.groupFormatfn(entry.Positive, addedDenom)));
					parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.REMOVED,
						group.groupFormatfn(0f - entry.Negative, removedDenom)));
				} else {
					parts.Add(group.groupFormatfn(entry.Net, netDenom));
				}
			} else if (isDelta) {
				parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.NET,
					FormatValue(entry.Net, group)));
				parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.ADDED,
					FormatValue(entry.Positive, group)));
				parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.REMOVED,
					FormatValue(0f - entry.Negative, group)));
			} else {
				parts.Add(FormatValue(entry.Net, group));
			}

			return string.Join(", ", parts);
		}

		private string BuildContextLabel(
			ReportManager.ReportEntry contextEntry,
			ReportManager.ReportGroup group) {

			var parts = new List<string>(8);
			parts.Add(contextEntry.context);

			parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.NET,
				FormatValue(contextEntry.Net, group)));
			if (contextEntry.Positive != 0f)
				parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.ADDED,
					FormatValue(contextEntry.Positive, group)));
			if (contextEntry.Negative != 0f)
				parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.REMOVED,
					FormatValue(0f - contextEntry.Negative, group)));

			CollectNotes(contextEntry, group);
			foreach (var note in _notesScratch) {
				parts.Add(string.Format(STRINGS.ONIACCESS.REPORT.NOTE,
					note.note, FormatValue(note.value, group)));
			}

			return string.Join(", ", parts);
		}

		private string BuildNoteLabel(
			ReportManager.ReportEntry.Note note,
			ReportManager.ReportGroup group) {
			return string.Format(STRINGS.ONIACCESS.REPORT.NOTE,
				note.note, FormatValue(note.value, group));
		}
	}
}
