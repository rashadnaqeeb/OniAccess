using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens {
	public class AssignmentGroupControllerHandler : NestedMenuHandler {
		private AssignmentGroupControllerSideScreen CrewScreen =>
			(AssignmentGroupControllerSideScreen)_screen;

		private bool _pendingActivation;

		private static readonly FieldInfo _targetField = typeof(AssignmentGroupControllerSideScreen)
			.GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo _identityRowMapField = typeof(AssignmentGroupControllerSideScreen)
			.GetField("identityRowMap", BindingFlags.NonPublic | BindingFlags.Instance);

		static AssignmentGroupControllerHandler() {
			if (_targetField == null) Util.Log.Warn("AssignmentGroupControllerHandler: target field not found");
			if (_identityRowMapField == null) Util.Log.Warn("AssignmentGroupControllerHandler: identityRowMap field not found");
		}

		public override string DisplayName => null;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		protected override int MaxLevel => 1;
		protected override int SearchLevel => 1;

		public AssignmentGroupControllerHandler(AssignmentGroupControllerSideScreen screen) : base(screen) {
			HelpEntries = new List<HelpEntry>(NestedNavHelpEntries).AsReadOnly();
		}

		// ========================================
		// DATA ACCESS
		// ========================================

		private AssignmentGroupController GetTarget() {
			return _targetField.GetValue(CrewScreen) as AssignmentGroupController;
		}

		private List<GameObject> GetIdentityRowMap() {
			return _identityRowMapField.GetValue(CrewScreen) as List<GameObject>;
		}

		private struct RowData {
			public MultiToggle Toggle;
			public string Name;
			public string Designation;
			public bool IsOffworld;
		}

		private void GetAllRows(out List<RowData> sameWorld, out List<RowData> offWorld) {
			sameWorld = new List<RowData>();
			offWorld = new List<RowData>();
			var rows = GetIdentityRowMap();
			if (rows == null) return;
			string offworldText = (string)STRINGS.UI.UISIDESCREENS.ASSIGNMENTGROUPCONTROLLER.OFFWORLD;
			foreach (var row in rows) {
				if (!row.activeSelf) continue;
				var refs = row.GetComponent<HierarchyReferences>();
				if (refs == null) continue;
				var toggle = refs.GetReference<MultiToggle>("Toggle");
				var label = refs.GetReference<LocText>("Label");
				var designation = refs.GetReference<LocText>("Designation");
				string desigText = designation != null ? designation.GetParsedText() : "";
				var data = new RowData {
					Toggle = toggle,
					Name = label != null ? label.GetParsedText() : "",
					Designation = desigText,
					IsOffworld = desigText == offworldText
				};
				if (data.IsOffworld)
					offWorld.Add(data);
				else
					sameWorld.Add(data);
			}
		}

		private List<RowData> GetSection(List<RowData> same, List<RowData> off, int section) {
			return section == 0 ? same : off;
		}

		// ========================================
		// NESTED MENU ABSTRACTS
		// ========================================

		protected override int GetItemCount(int level, int[] indices) {
			GetAllRows(out var same, out var off);
			if (level == 0) return off.Count > 0 ? 2 : 1;
			if (level == 1) return GetSection(same, off, indices[0]).Count;
			return 0;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (level == 0) return GetSectionHeader(indices[0]);
			if (level == 1) return GetRowLabel(indices[0], indices[1]);
			return null;
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level == 1) return GetSectionHeader(indices[0]);
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			GetAllRows(out var same, out var off);
			var rows = GetSection(same, off, indices[0]);
			if (indices[1] < 0 || indices[1] >= rows.Count) return;
			var row = rows[indices[1]];
			row.Toggle.onClick?.Invoke();
			// Read state from the captured toggle reference — RefreshRows
			// re-sorts the list, so re-querying by index could hit the wrong row
			int memberCount = GetTarget().GetMembers().Count;
			string state = row.Toggle.CurrentState == 1
				? (string)STRINGS.ONIACCESS.STATES.ASSIGNED
				: (string)STRINGS.ONIACCESS.STATES.UNASSIGNED;
			string feedback = state + ", " + string.Format(STRINGS.ONIACCESS.CREW_SCREEN.TOTAL_FORMAT, memberCount);
			SpeechPipeline.SpeakInterrupt(feedback);
		}

		protected override int GetSearchItemCount(int[] indices) {
			GetAllRows(out var same, out var off);
			return same.Count + off.Count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			GetAllRows(out var same, out var off);
			if (flatIndex < same.Count)
				return BuildRowLabel(same[flatIndex]);
			int offIdx = flatIndex - same.Count;
			if (offIdx < off.Count)
				return BuildRowLabel(off[offIdx]);
			return null;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			GetAllRows(out var same, out var off);
			if (flatIndex < same.Count) {
				outIndices[0] = 0;
				outIndices[1] = flatIndex;
			} else {
				outIndices[0] = 1;
				outIndices[1] = flatIndex - same.Count;
			}
		}

		// ========================================
		// LABELS
		// ========================================

		private string GetSectionHeader(int section) {
			if (section == 0)
				return (string)STRINGS.ONIACCESS.CREW_SCREEN.AVAILABLE;
			return (string)STRINGS.UI.UISIDESCREENS.ASSIGNMENTGROUPCONTROLLER.OFFWORLD;
		}

		private string GetRowLabel(int section, int rowIndex) {
			GetAllRows(out var same, out var off);
			var rows = GetSection(same, off, section);
			if (rowIndex < 0 || rowIndex >= rows.Count) return null;
			return BuildRowLabel(rows[rowIndex]);
		}

		private string BuildRowLabel(RowData row) {
			string label = row.Name;
			// Skip designation for offworld rows — section header already provides context
			if (!row.IsOffworld && !string.IsNullOrEmpty(row.Designation))
				label += ", " + row.Designation;
			string state = row.Toggle.CurrentState == 1
				? (string)STRINGS.ONIACCESS.STATES.ASSIGNED
				: (string)STRINGS.ONIACCESS.STATES.UNASSIGNED;
			label += ", " + state;
			return label;
		}

		// ========================================
		// ESCAPE
		// ========================================

		public override bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				CloseScreen();
				return true;
			}
			return base.HandleKeyDown(e);
		}

		private void CloseScreen() {
			DetailsScreenHandler.PreserveNavigationOnReactivate = true;
			HandlerStack.Pop();
			DetailsScreen.Instance?.ClearSecondarySideScreen();
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			base.OnActivate();
			_pendingActivation = true;
		}

		public override bool Tick() {
			if (_pendingActivation) {
				_pendingActivation = false;
				string title = (string)STRINGS.UI.UISIDESCREENS.ASSIGNMENTGROUPCONTROLLER.TITLE;
				int memberCount = GetTarget().GetMembers().Count;
				string countLabel = string.Format(STRINGS.ONIACCESS.CREW_SCREEN.ASSIGNED_COUNT_FORMAT, memberCount);
				string firstItem = GetItemLabel(0, new[] { 0 });
				string announcement = title + ", " + countLabel + ", " + firstItem;
				SpeechPipeline.SpeakInterrupt(announcement);
				return false;
			}
			return base.Tick();
		}
	}
}
