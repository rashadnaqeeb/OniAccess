using System.Collections.Generic;
using System.Linq;

using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// 2D grid handler for the JobsTableScreen (duplicant priority management).
	/// Extends BaseScreenHandler directly — the grid cursor doesn't fit 1D list navigation.
	///
	/// Builds a virtual table from live game state on every navigation event.
	/// _choreGroups holds a filtered snapshot of Db ChoreGroup resources, which are
	/// immutable database objects — safe to cache for the screen's lifetime.
	/// </summary>
	public class PriorityScreenHandler : BaseScreenHandler {
		enum RowKind { Toolbar, ColumnHeader, WorldDivider, Minion, StoredMinion, NewDuplicants }

		struct RowEntry {
			public RowKind Kind;
			public IAssignableIdentity Identity;
			public int WorldId;
		}

		// State
		int _row, _col;
		List<ChoreGroup> _choreGroups;
		int _sortColumn = -1;
		bool _sortAscending;
		int _lastSpokenRow = -1;
		int _lastSpokenCol = -1;

		// Cached row list, rebuilt on every navigation event
		List<RowEntry> _rows = new List<RowEntry>();

		public override string DisplayName => STRINGS.ONIACCESS.PRIORITY_SCREEN.HANDLER_NAME;
		public override bool CapturesAllInput => true;

		public PriorityScreenHandler(KScreen screen) : base(screen) { }

		// ========================================
		// HELP
		// ========================================

		static readonly List<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Left/Right", STRINGS.ONIACCESS.PRIORITY_SCREEN.NAVIGATE_COLUMNS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("0-5", STRINGS.ONIACCESS.PRIORITY_SCREEN.SET_PRIORITY),
			new HelpEntry("Enter", STRINGS.ONIACCESS.PRIORITY_SCREEN.ACTIVATE_OR_SORT),
			new HelpEntry("Ctrl+Left/Right", STRINGS.ONIACCESS.PRIORITY_SCREEN.ADJUST_ROW),
			new HelpEntry("Ctrl+Up/Down", STRINGS.ONIACCESS.PRIORITY_SCREEN.ADJUST_COLUMN),
			new HelpEntry("Shift+0-5", STRINGS.ONIACCESS.PRIORITY_SCREEN.SET_COLUMN),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			_choreGroups = Db.Get().ChoreGroups.resources
				.Where(g => g.userPrioritizable)
				.ToList();

			_sortColumn = -1;
			_sortAscending = false;
			BuildRowList();

			// Start on first minion row
			_row = 0;
			for (int i = 0; i < _rows.Count; i++) {
				if (_rows[i].Kind == RowKind.Minion) {
					_row = i;
					break;
				}
			}
			_col = 0;
			_lastSpokenRow = -1;
			_lastSpokenCol = -1;

			base.OnActivate();
			SpeechPipeline.SpeakQueued(BuildCellParts(forceFullContext: true));
		}

		// ========================================
		// SOUNDS
		// ========================================

		void PlayHoverSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Mouseover"));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayHoverSound failed: {ex.Message}");
			}
		}

		void PlayWrapSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click"));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayWrapSound failed: {ex.Message}");
			}
		}

		// ========================================
		// ROW LIST BUILDING
		// ========================================

		void BuildRowList() {
			_rows.Clear();
			bool showDividers = DlcManager.FeatureClusterSpaceEnabled();

			// Toolbar
			_rows.Add(new RowEntry { Kind = RowKind.Toolbar });

			// Column headers
			_rows.Add(new RowEntry { Kind = RowKind.ColumnHeader });

			// Per-world minion rows
			var worldIds = ClusterManager.Instance.GetWorldIDsSorted();
			foreach (int worldId in worldIds) {
				var world = ClusterManager.Instance.GetWorld(worldId);
				if (world == null || !world.IsDiscovered) continue;

				var minions = GetLiveMinionsForWorld(worldId);
				if (minions.Count == 0) continue;

				if (showDividers)
					_rows.Add(new RowEntry { Kind = RowKind.WorldDivider, WorldId = worldId });

				if (_sortColumn >= 0 && _sortColumn < _choreGroups.Count) {
					var group = _choreGroups[_sortColumn];
					minions.Sort((a, b) => {
						var ca = ((MinionIdentity)a).GetComponent<ChoreConsumer>();
						var cb = ((MinionIdentity)b).GetComponent<ChoreConsumer>();
						bool disA = ca.IsChoreGroupDisabled(group);
						bool disB = cb.IsChoreGroupDisabled(group);
						if (disA != disB) return disA ? 1 : -1;
						int cmp = ca.GetPersonalPriority(group).CompareTo(cb.GetPersonalPriority(group));
						if (!_sortAscending) cmp = -cmp;
						if (cmp != 0) return cmp;
						return a.GetProperName().CompareTo(b.GetProperName());
					});
				}

				foreach (var minion in minions) {
					_rows.Add(new RowEntry { Kind = RowKind.Minion, Identity = minion });
				}
			}

			// Stored minions
			var stored = GetStoredMinions();
			if (stored.Count > 0) {
				if (showDividers)
					_rows.Add(new RowEntry { Kind = RowKind.WorldDivider, WorldId = 255 });
				foreach (var smi in stored) {
					_rows.Add(new RowEntry { Kind = RowKind.StoredMinion, Identity = smi });
				}
			}

			// New duplicants row
			_rows.Add(new RowEntry { Kind = RowKind.NewDuplicants });
		}

		List<IAssignableIdentity> GetLiveMinionsForWorld(int worldId) {
			var result = new List<IAssignableIdentity>();
			foreach (var mi in Components.LiveMinionIdentities.Items) {
				if (mi != null && mi.GetMyWorldId() == worldId)
					result.Add(mi);
			}
			return result;
		}

		List<StoredMinionIdentity> GetStoredMinions() {
			var result = new List<StoredMinionIdentity>();
			foreach (var storage in Components.MinionStorages.Items) {
				foreach (var info in storage.GetStoredMinionInfo()) {
					if (info.serializedMinion != null) {
						var smi = info.serializedMinion.Get<StoredMinionIdentity>();
						if (smi != null) result.Add(smi);
					}
				}
			}
			return result;
		}

		// ========================================
		// PRIORITY MANAGER ACCESS
		// ========================================

		IPersonalPriorityManager GetPriorityManager(RowEntry row) {
			switch (row.Kind) {
				case RowKind.Minion:
					return ((MinionIdentity)row.Identity).GetComponent<ChoreConsumer>();
				case RowKind.StoredMinion:
					return (StoredMinionIdentity)row.Identity;
				case RowKind.NewDuplicants:
					return Immigration.Instance;
				default:
					return null;
			}
		}

		// ========================================
		// PRIORITY NAME
		// ========================================

		static string GetPriorityName(int value) {
			switch (value) {
				case 0: return STRINGS.UI.JOBSSCREEN.PRIORITY.DISABLED;
				case 1: return STRINGS.UI.JOBSSCREEN.PRIORITY.VERYLOW;
				case 2: return STRINGS.UI.JOBSSCREEN.PRIORITY.LOW;
				case 3: return STRINGS.UI.JOBSSCREEN.PRIORITY.STANDARD;
				case 4: return STRINGS.UI.JOBSSCREEN.PRIORITY.HIGH;
				case 5: return STRINGS.UI.JOBSSCREEN.PRIORITY.VERYHIGH;
				default: return value.ToString();
			}
		}

		// ========================================
		// SPEECH
		// ========================================

		void SpeakCell() {
			if (_row < 0 || _row >= _rows.Count) return;
			SpeechPipeline.SpeakInterrupt(BuildCellParts(forceFullContext: false));
		}

		string BuildCellParts(bool forceFullContext) {
			var row = _rows[_row];
			var parts = new List<string>();

			// Row context
			if (forceFullContext || _row != _lastSpokenRow) {
				switch (row.Kind) {
					case RowKind.Toolbar:
						parts.Add((string)STRINGS.ONIACCESS.PRIORITY_SCREEN.TOOLBAR);
						break;
					case RowKind.ColumnHeader:
						break;
					case RowKind.Minion:
					case RowKind.StoredMinion:
						parts.Add(row.Identity.GetProperName());
						break;
					case RowKind.NewDuplicants:
						parts.Add((string)STRINGS.UI.JOBSCREEN_DEFAULT);
						break;
				}
			}

			// Column context (not on toolbar)
			if ((forceFullContext || _col != _lastSpokenCol) && row.Kind != RowKind.Toolbar) {
				if (_col >= 0 && _col < _choreGroups.Count)
					parts.Add(_choreGroups[_col].Name);
			}

			parts.Add(GetCellValue(row));

			_lastSpokenRow = _row;
			_lastSpokenCol = _col;

			return string.Join(", ", parts);
		}

		string GetCellValue(RowEntry row) {
			switch (row.Kind) {
				case RowKind.Toolbar:
					if (_col == 0) return STRINGS.UI.JOBSSCREEN.RESET_SETTINGS;
					if (_col == 1) return (Game.Instance.advancedPersonalPriorities
						? (string)STRINGS.ONIACCESS.PRIORITY_SCREEN.PROXIMITY_ON
						: (string)STRINGS.ONIACCESS.PRIORITY_SCREEN.PROXIMITY_OFF)
						+ ", " + STRINGS.UI.JOBSSCREEN.TOGGLE_ADVANCED_MODE_TOOLTIP;
					return "";

				case RowKind.ColumnHeader:
					if (_col >= 0 && _col < _choreGroups.Count) {
						var group = _choreGroups[_col];
						var choreNames = string.Join(", ", group.choreTypes.Select(ct => ct.Name));
						return group.description + ", "
							+ string.Format(STRINGS.ONIACCESS.PRIORITY_SCREEN.AFFECTED_ERRANDS, choreNames);
					}
					return "";

				case RowKind.Minion: {
					var manager = GetPriorityManager(row);
					var group = _choreGroups[_col];
					if (manager.IsChoreGroupDisabled(group)) {
						string traitName = GetDisablingTraitName(row.Identity, group);
						return string.Format(STRINGS.ONIACCESS.PRIORITY_SCREEN.DISABLED_TRAIT, traitName);
					}
					int priority = manager.GetPersonalPriority(group);
					int skill = manager.GetAssociatedSkillLevel(group);
					return GetPriorityName(priority) + ", "
						+ string.Format(STRINGS.ONIACCESS.PRIORITY_SCREEN.SKILL, skill);
				}

				case RowKind.StoredMinion: {
					var manager = GetPriorityManager(row);
					var group = _choreGroups[_col];
					if (manager.IsChoreGroupDisabled(group)) {
						string traitName = GetDisablingTraitName(row.Identity, group);
						return string.Format(STRINGS.ONIACCESS.PRIORITY_SCREEN.DISABLED_TRAIT, traitName);
					}
					return GetPriorityName(manager.GetPersonalPriority(group));
				}

				case RowKind.NewDuplicants: {
					var group = _choreGroups[_col];
					return GetPriorityName(Immigration.Instance.GetPersonalPriority(group));
				}

				default:
					return "";
			}
		}

		string GetDisablingTraitName(IAssignableIdentity identity, ChoreGroup group) {
			if (identity is MinionIdentity mi) {
				var traits = mi.GetComponent<Klei.AI.Traits>();
				if (traits != null && traits.IsChoreGroupDisabled(group, out Klei.AI.Trait disablingTrait))
					return disablingTrait.Name;
			}
			if (identity is StoredMinionIdentity smi) {
				if (smi.IsChoreGroupDisabled(group))
					return STRINGS.ONIACCESS.STATES.DISABLED;
			}
			return STRINGS.ONIACCESS.STATES.DISABLED;
		}

		// ========================================
		// NAVIGATION
		// ========================================

		void NavigateRow(int direction) {
			BuildRowList();
			int newRow = _row + direction;

			// Clamp
			if (newRow < 0 || newRow >= _rows.Count) return;

			// Skip world dividers, announce the last one crossed (the world being entered)
			if (_rows[newRow].Kind == RowKind.WorldDivider) {
				string worldName = GetWorldName(_rows[newRow].WorldId);
				int beyondDivider = newRow + direction;
				if (beyondDivider < 0 || beyondDivider >= _rows.Count) return;
				while (beyondDivider >= 0 && beyondDivider < _rows.Count
					&& _rows[beyondDivider].Kind == RowKind.WorldDivider) {
					worldName = GetWorldName(_rows[beyondDivider].WorldId);
					beyondDivider += direction;
				}
				if (beyondDivider < 0 || beyondDivider >= _rows.Count) return;
				_row = beyondDivider;
				ClampColForRow();
				PlayHoverSound();
				_lastSpokenRow = -1;
				_lastSpokenCol = -1;
				SpeechPipeline.SpeakInterrupt(
					worldName + ", " + BuildCellParts(forceFullContext: true));
				return;
			}

			_row = newRow;
			ClampColForRow();
			PlayHoverSound();
			SpeakCell();
		}

		void ClampColForRow() {
			if (_rows[_row].Kind == RowKind.Toolbar)
				_col = UnityEngine.Mathf.Clamp(_col, 0, 1);
		}

		void NavigateCol(int direction) {
			var row = _rows[_row];
			int maxCol = row.Kind == RowKind.Toolbar ? 1 : _choreGroups.Count - 1;

			int newCol = _col + direction;

			if (row.Kind == RowKind.Toolbar) {
				if (newCol < 0 || newCol > maxCol) return;
				_col = newCol;
				PlayHoverSound();
			} else {
				if (newCol < 0) {
					newCol = maxCol;
					_col = newCol;
					PlayWrapSound();
				} else if (newCol > maxCol) {
					newCol = 0;
					_col = newCol;
					PlayWrapSound();
				} else {
					_col = newCol;
					PlayHoverSound();
				}
			}

			SpeakCell();
		}

		void NavigateHome() {
			BuildRowList();
			for (int i = 0; i < _rows.Count; i++) {
				if (_rows[i].Kind != RowKind.Toolbar
					&& _rows[i].Kind != RowKind.ColumnHeader
					&& _rows[i].Kind != RowKind.WorldDivider) {
					_row = i;
					PlayHoverSound();
					SpeakCell();
					return;
				}
			}
		}

		void NavigateEnd() {
			BuildRowList();
			for (int i = _rows.Count - 1; i >= 0; i--) {
				if (_rows[i].Kind != RowKind.WorldDivider) {
					_row = i;
					PlayHoverSound();
					SpeakCell();
					return;
				}
			}
		}

		string GetWorldName(int worldId) {
			if (worldId == 255) return STRINGS.ONIACCESS.PRIORITY_SCREEN.STORED;
			var world = ClusterManager.Instance.GetWorld(worldId);
			return world != null ? world.GetProperName() : worldId.ToString();
		}

		// ========================================
		// EDITING
		// ========================================

		void SetCellPriority(int value) {
			if (_row < 0 || _row >= _rows.Count) return;
			var row = _rows[_row];

			if (row.Kind == RowKind.Toolbar || row.Kind == RowKind.ColumnHeader) return;

			if (row.Kind == RowKind.StoredMinion) {
				string msg = string.Format(
					STRINGS.UI.JOBSSCREEN.CANNOT_ADJUST_PRIORITY,
					row.Identity.GetProperName(),
					((StoredMinionIdentity)row.Identity).GetStorageReason());
				SpeechPipeline.SpeakInterrupt(TextFilter.FilterForSpeech(msg));
				return;
			}

			var manager = GetPriorityManager(row);
			var group = _choreGroups[_col];
			if (manager.IsChoreGroupDisabled(group)) {
				string traitName = GetDisablingTraitName(row.Identity, group);
				SpeechPipeline.SpeakInterrupt(
					string.Format(STRINGS.ONIACCESS.PRIORITY_SCREEN.DISABLED_TRAIT, traitName));
				return;
			}

			manager.SetPersonalPriority(group, value);
			SpeakCell();
		}

		void AdjustRow(int delta) {
			if (_row < 0 || _row >= _rows.Count) return;
			var row = _rows[_row];
			if (row.Kind == RowKind.Toolbar || row.Kind == RowKind.ColumnHeader
				|| row.Kind == RowKind.WorldDivider || row.Kind == RowKind.StoredMinion) return;

			var manager = GetPriorityManager(row);
			foreach (var group in _choreGroups) {
				if (manager.IsChoreGroupDisabled(group)) continue;
				int current = manager.GetPersonalPriority(group);
				int newVal = UnityEngine.Mathf.Clamp(current + delta, 0, 5);
				manager.SetPersonalPriority(group, newVal);
			}

			string announcement = delta > 0
				? (string)STRINGS.ONIACCESS.PRIORITY_SCREEN.ROW_INCREASED
				: (string)STRINGS.ONIACCESS.PRIORITY_SCREEN.ROW_DECREASED;
			_lastSpokenRow = -1;
			_lastSpokenCol = -1;
			SpeechPipeline.SpeakInterrupt(announcement);
		}

		void SetColumnPriority(int value) {
			if (_col < 0 || _col >= _choreGroups.Count) return;
			var group = _choreGroups[_col];

			foreach (var row in _rows) {
				if (row.Kind != RowKind.Minion && row.Kind != RowKind.NewDuplicants) continue;
				var manager = GetPriorityManager(row);
				if (manager.IsChoreGroupDisabled(group)) continue;
				manager.SetPersonalPriority(group, value);
			}

			_lastSpokenRow = -1;
			_lastSpokenCol = -1;
			SpeechPipeline.SpeakInterrupt(
				_choreGroups[_col].Name + ", " + string.Format(STRINGS.ONIACCESS.PRIORITY_SCREEN.COLUMN_SET, GetPriorityName(value)));
		}

		void AdjustColumn(int delta) {
			if (_col < 0 || _col >= _choreGroups.Count) return;
			var group = _choreGroups[_col];

			foreach (var row in _rows) {
				if (row.Kind != RowKind.Minion && row.Kind != RowKind.NewDuplicants) continue;
				var manager = GetPriorityManager(row);
				if (manager.IsChoreGroupDisabled(group)) continue;
				int current = manager.GetPersonalPriority(group);
				int newVal = UnityEngine.Mathf.Clamp(current + delta, 0, 5);
				manager.SetPersonalPriority(group, newVal);
			}

			string announcement = delta > 0
				? (string)STRINGS.ONIACCESS.PRIORITY_SCREEN.COLUMN_INCREASED
				: (string)STRINGS.ONIACCESS.PRIORITY_SCREEN.COLUMN_DECREASED;
			_lastSpokenRow = -1;
			_lastSpokenCol = -1;
			SpeechPipeline.SpeakInterrupt(announcement);
		}

		// ========================================
		// TOOLBAR
		// ========================================

		void ActivateToolbarItem() {
			if (_col == 0) {
				// Reset priorities
				HarmonyLib.Traverse.Create(_screen).Method("OnResetSettingsClicked").GetValue();
				SpeechPipeline.SpeakInterrupt((string)STRINGS.UI.JOBSSCREEN.RESET_SETTINGS);
			} else if (_col == 1) {
				// Toggle proximity
				Game.Instance.advancedPersonalPriorities = !Game.Instance.advancedPersonalPriorities;
				SpeechPipeline.SpeakInterrupt(Game.Instance.advancedPersonalPriorities
					? (string)STRINGS.ONIACCESS.PRIORITY_SCREEN.PROXIMITY_ON
					: (string)STRINGS.ONIACCESS.PRIORITY_SCREEN.PROXIMITY_OFF);
			}
		}

		// ========================================
		// SORT
		// ========================================

		void CycleSort() {
			if (_col < 0 || _col >= _choreGroups.Count) return;

			if (_sortColumn != _col) {
				_sortColumn = _col;
				_sortAscending = false;
				SpeechPipeline.SpeakInterrupt(
					_choreGroups[_col].Name + ", " + STRINGS.ONIACCESS.PRIORITY_SCREEN.SORT_DESCENDING);
			} else if (!_sortAscending) {
				_sortAscending = true;
				SpeechPipeline.SpeakInterrupt(
					_choreGroups[_col].Name + ", " + STRINGS.ONIACCESS.PRIORITY_SCREEN.SORT_ASCENDING);
			} else {
				_sortColumn = -1;
				SpeechPipeline.SpeakInterrupt(
					_choreGroups[_col].Name + ", " + STRINGS.ONIACCESS.PRIORITY_SCREEN.SORT_CLEARED);
			}

			BuildRowList();
		}

		// ========================================
		// TICK
		// ========================================

		public override bool Tick() {
			if (base.Tick()) return true;

			bool ctrlHeld = InputUtil.CtrlHeld();
			bool shiftHeld = InputUtil.ShiftHeld();

			// Number keys: Shift+0-5 sets entire column, plain 0-5 sets cell
			if (!ctrlHeld) {
				for (int n = 0; n <= 5; n++) {
					if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha0 + n)) {
						if (shiftHeld)
							SetColumnPriority(n);
						else
							SetCellPriority(n);
						return true;
					}
				}
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)) {
				if (ctrlHeld)
					AdjustColumn(1);
				else
					NavigateRow(-1);
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) {
				if (ctrlHeld)
					AdjustColumn(-1);
				else
					NavigateRow(1);
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow)) {
				if (ctrlHeld)
					AdjustRow(-1);
				else
					NavigateCol(-1);
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow)) {
				if (ctrlHeld)
					AdjustRow(1);
				else
					NavigateCol(1);
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Home)) {
				NavigateHome();
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.End)) {
				NavigateEnd();
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
				if (_row >= 0 && _row < _rows.Count) {
					var row = _rows[_row];
					if (row.Kind == RowKind.Toolbar)
						ActivateToolbarItem();
					else if (row.Kind == RowKind.ColumnHeader)
						CycleSort();
				}
				return true;
			}

			return false;
		}
	}
}
