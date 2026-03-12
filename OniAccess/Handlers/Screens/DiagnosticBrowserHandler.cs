using System.Collections.Generic;

using OniAccess.Handlers.Tiles;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Two-level diagnostics browser backed by AllDiagnosticsScreen.
	/// Level 0 = diagnostic rows (sorted alphabetically), level 1 = criteria sub-rows.
	///
	/// Space at level 0 cycles pin state (matching game: Always -> Never -> AlertOnly).
	/// Space at level 1 toggles criterion enabled/disabled.
	/// Escape closes the screen.
	/// </summary>
	internal sealed class DiagnosticBrowserHandler: NestedMenuHandler {
		internal DiagnosticBrowserHandler(KScreen screen) : base(screen) { }

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.Space),
		};
		public override IReadOnlyList<ConsumedKey> ConsumedKeys => _consumedKeys;

		public override string DisplayName =>
			(string)STRINGS.ONIACCESS.HANDLERS.DIAGNOSTICS;

		public override void OnActivate() {
			PlaySound("HUD_Click_Open");
			base.OnActivate();

			try {
				var field = HarmonyLib.Traverse.Create(_screen).Field("searchInputField")
					.GetValue<KInputTextField>();
				if (field != null)
					field.DeactivateInputField();
			} catch (System.Exception ex) {
				Util.Log.Warn($"DiagnosticBrowserHandler: failed to deactivate search field: {ex.Message}");
			}

			if (GetDiagnosticIds().Count > 0)
				SpeechPipeline.SpeakQueued(GetItemLabel(0, new int[MaxLevel + 1]));
		}

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry>(NestedNavHelpEntries) {
				new HelpEntry("Space", STRINGS.ONIACCESS.DIAGNOSTICS.HELP_TOGGLE_PIN),
				new HelpEntry("Space (criteria)", STRINGS.ONIACCESS.DIAGNOSTICS.HELP_TOGGLE_CRITERION),
			}.AsReadOnly();

		protected override int MaxLevel => 1;
		protected override int SearchLevel => 0;

		// ========================================
		// DATA ACCESS
		// ========================================

		private int WorldId => ClusterManager.Instance.activeWorldId;

		private List<string> GetDiagnosticIds() {
			var ids = new List<string>();
			var util = ColonyDiagnosticUtility.Instance;
			var settings = util.diagnosticDisplaySettings;
			if (!settings.ContainsKey(WorldId)) return ids;
			var criteriaDisabled = util.diagnosticCriteriaDisabled;
			bool hasCriteria = criteriaDisabled.ContainsKey(WorldId);

			foreach (var kvp in settings[WorldId]) {
				var diag = util.GetDiagnostic(kvp.Key, WorldId);
				if (diag == null) continue;
				if (diag is WorkTimeDiagnostic || diag is ChoreGroupDiagnostic) continue;
				if (hasCriteria && !criteriaDisabled[WorldId].ContainsKey(kvp.Key)) continue;
				ids.Add(kvp.Key);
			}
			ids.Sort((a, b) => {
				string nameA = util.GetDiagnosticName(a);
				string nameB = util.GetDiagnosticName(b);
				return string.Compare(
					STRINGS.UI.StripLinkFormatting(nameA),
					STRINGS.UI.StripLinkFormatting(nameB),
					System.StringComparison.Ordinal);
			});
			return ids;
		}

		private ColonyDiagnostic GetDiagnostic(List<string> ids, int diagIndex) {
			if (diagIndex < 0 || diagIndex >= ids.Count) return null;
			return ColonyDiagnosticUtility.Instance.GetDiagnostic(
				ids[diagIndex], WorldId);
		}

		// ========================================
		// ITEM COUNTS AND LABELS
		// ========================================

		protected override int GetItemCount(int level, int[] indices) {
			var ids = GetDiagnosticIds();
			if (level == 0)
				return ids.Count;
			var diag = GetDiagnostic(ids, indices[0]);
			if (diag == null) return 0;
			return diag.GetCriteria().Length;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			var ids = GetDiagnosticIds();
			if (level == 0) {
				var diag = GetDiagnostic(ids, indices[0]);
				if (diag == null) return null;
				return BuildDiagnosticLabel(diag);
			}
			var parentDiag = GetDiagnostic(ids, indices[0]);
			if (parentDiag == null) return null;
			var criteria = parentDiag.GetCriteria();
			if (indices[1] < 0 || indices[1] >= criteria.Length) return null;
			return BuildCriterionLabel(parentDiag, criteria[indices[1]]);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level >= 1) {
				var ids = GetDiagnosticIds();
				var diag = GetDiagnostic(ids, indices[0]);
				if (diag != null) return diag.name;
			}
			return null;
		}

		private string BuildDiagnosticLabel(ColonyDiagnostic diag) {
			string message = diag.LatestResult.Message;
			bool hasMessage = !string.IsNullOrWhiteSpace(message);

			string value = diag.presentationSetting == ColonyDiagnostic.PresentationSetting.CurrentValue
				? diag.GetCurrentValueString()
				: diag.GetAverageValueString();

			string pinState = GetPinStateLabel(diag.id);

			string label = diag.name + ", ";
			if (hasMessage)
				label += message;
			else
				label += TileCursorHandler.OpinionWord(diag.LatestResult.opinion);
			if (!string.IsNullOrEmpty(value))
				label += ", " + value;
			label += ", " + pinState;
			return label;
		}

		private string BuildCriterionLabel(ColonyDiagnostic diag, DiagnosticCriterion criterion) {
			bool enabled = ColonyDiagnosticUtility.Instance.IsCriteriaEnabled(
				WorldId, diag.id, criterion.id);
			string state = enabled
				? (string)STRINGS.ONIACCESS.STATES.ENABLED
				: (string)STRINGS.ONIACCESS.STATES.DISABLED;
			return criterion.name + ", " + state;
		}

		private string GetPinStateLabel(string diagnosticId) {
			if (ColonyDiagnosticUtility.Instance.IsDiagnosticTutorialDisabled(diagnosticId))
				return (string)STRINGS.ONIACCESS.DIAGNOSTICS.PIN_TUTORIAL_DISABLED;

			var setting = ColonyDiagnosticUtility.Instance
				.diagnosticDisplaySettings[WorldId][diagnosticId];
			switch (setting) {
				case ColonyDiagnosticUtility.DisplaySetting.Always:
					return (string)STRINGS.ONIACCESS.DIAGNOSTICS.PIN_ALWAYS;
				case ColonyDiagnosticUtility.DisplaySetting.AlertOnly:
					return (string)STRINGS.ONIACCESS.DIAGNOSTICS.PIN_ALERT_ONLY;
				case ColonyDiagnosticUtility.DisplaySetting.Never:
					return (string)STRINGS.ONIACCESS.DIAGNOSTICS.PIN_NEVER;
				default:
					return setting.ToString();
			}
		}

		// ========================================
		// LEAF ACTIVATION (Enter at level 1 — no-op)
		// ========================================

		protected override void ActivateLeafItem(int[] indices) { }

		// ========================================
		// SEARCH: flat across diagnostic names
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			return GetDiagnosticIds().Count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			var ids = GetDiagnosticIds();
			var diag = GetDiagnostic(ids, flatIndex);
			return diag?.name;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			outIndices[0] = flatIndex;
			outIndices[1] = 0;
		}

		// ========================================
		// TICK: Space toggles
		// ========================================

		public override bool Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space)
				&& !InputUtil.AnyModifierHeld()) {
				if (Level == 0)
					TogglePin();
				else if (Level == 1)
					ToggleCriterion();
				return true;
			}
			return base.Tick();
		}

		private void TogglePin() {
			var diag = GetDiagnostic(GetDiagnosticIds(), GetIndex(0));
			if (diag == null) return;

			if (ColonyDiagnosticUtility.Instance.IsDiagnosticTutorialDisabled(diag.id)) {
				ColonyDiagnosticUtility.Instance.ClearDiagnosticTutorialSetting(diag.id);
				SyncSidebar();
				PlaySound("HUD_Click");
				SpeechPipeline.SpeakInterrupt(GetPinStateLabel(diag.id));
				return;
			}

			// Match game cycle: Always(0) -> Never(2) -> AlertOnly(1) -> Always(0)
			int current = (int)ColonyDiagnosticUtility.Instance
				.diagnosticDisplaySettings[WorldId][diag.id];
			int next = current - 1;
			if (next < 0) next = 2;
			ColonyDiagnosticUtility.Instance
				.diagnosticDisplaySettings[WorldId][diag.id] =
				(ColonyDiagnosticUtility.DisplaySetting)next;

			SyncSidebar();
			PlaySound("HUD_Click");
			SpeechPipeline.SpeakInterrupt(GetPinStateLabel(diag.id));
		}

		private void ToggleCriterion() {
			var diag = GetDiagnostic(GetDiagnosticIds(), GetIndex(0));
			if (diag == null) return;
			var criteria = diag.GetCriteria();
			int critIdx = GetIndex(1);
			if (critIdx < 0 || critIdx >= criteria.Length) return;
			var criterion = criteria[critIdx];

			bool enabled = ColonyDiagnosticUtility.Instance.IsCriteriaEnabled(
				WorldId, diag.id, criterion.id);
			ColonyDiagnosticUtility.Instance.SetCriteriaEnabled(
				WorldId, diag.id, criterion.id, !enabled);

			SyncSidebar();
			PlaySound("HUD_Click");
			string state = !enabled
				? (string)STRINGS.ONIACCESS.STATES.ENABLED
				: (string)STRINGS.ONIACCESS.STATES.DISABLED;
			SpeechPipeline.SpeakInterrupt(state);
		}

		private void SyncSidebar() {
			if (ColonyDiagnosticScreen.Instance != null)
				ColonyDiagnosticScreen.Instance.RefreshAll();
		}

		// ========================================
		// ESCAPE: close AllDiagnosticsScreen
		// ========================================

		public override bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				CloseScreen();
				return true;
			}
			return base.HandleKeyDown(e);
		}

		internal void CloseScreen() {
			PlaySound("HUD_Click_Close");
			if (AllDiagnosticsScreen.Instance != null)
				AllDiagnosticsScreen.Instance.Show(false);
		}
	}
}
