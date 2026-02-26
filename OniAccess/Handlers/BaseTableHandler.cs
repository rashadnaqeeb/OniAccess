using System.Collections.Generic;

using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers {
	/// <summary>
	/// Abstract base for 2D table screen handlers. Provides shared infrastructure:
	/// row list with world dividers, 2D cursor navigation, cell speech with
	/// row/column deduplication, sort cycling, and sound effects.
	///
	/// Subclasses implement BuildRowList, GetColumnCount, GetColumnName,
	/// GetCellValue, and GetRowLabel to describe their specific table.
	/// </summary>
	public abstract class BaseTableHandler: BaseScreenHandler {
		protected enum TableRowKind {
			Toolbar,
			ColumnHeader,
			WorldDivider,
			Minion,
			StoredMinion,
			Default
		}

		protected struct RowEntry {
			public TableRowKind Kind;
			public IAssignableIdentity Identity;
			public int WorldId;
		}

		// 2D cursor
		protected int _row, _col;
		protected int _lastSpokenRow = -1;
		protected int _lastSpokenCol = -1;

		// Row list, rebuilt on every navigation event
		protected List<RowEntry> _rows = new List<RowEntry>();

		// Sort state
		protected int _sortColumn = -1;
		protected bool _sortAscending;

		public override bool CapturesAllInput => true;

		protected BaseTableHandler(KScreen screen) : base(screen) { }

		// ========================================
		// ABSTRACT MEMBERS
		// ========================================

		protected abstract void BuildRowList();
		protected abstract int GetColumnCount(TableRowKind kind);
		protected abstract string GetColumnName(int col);
		protected abstract string GetCellValue(RowEntry row);
		protected abstract string GetRowLabel(RowEntry row);

		// ========================================
		// VIRTUAL MEMBERS
		// ========================================

		protected virtual bool ColumnWraps(TableRowKind kind) => kind != TableRowKind.Toolbar;
		protected virtual void OnEnterPressed(RowEntry row) { }
		protected virtual bool IsColumnSortable(int col) => true;

		protected const int StoredMinionWorldId = 255;

		protected virtual string GetWorldName(int worldId) {
			if (worldId == StoredMinionWorldId) return STRINGS.ONIACCESS.TABLE.STORED;
			var world = ClusterManager.Instance.GetWorld(worldId);
			return world != null ? world.GetProperName() : worldId.ToString();
		}

		protected virtual bool IsRowSkipped(TableRowKind kind) => kind == TableRowKind.WorldDivider;

		protected virtual bool HandleModifiedUpDown(int direction) => false;
		protected virtual bool HandleModifiedLeftRight(int direction) => false;
		protected virtual void OnTableActivate() { }

		protected virtual int FindInitialRow() {
			for (int i = 0; i < _rows.Count; i++) {
				var kind = _rows[i].Kind;
				if (kind != TableRowKind.Toolbar
					&& kind != TableRowKind.ColumnHeader
					&& kind != TableRowKind.WorldDivider)
					return i;
			}
			return 0;
		}

		// ========================================
		// SHARED QUERIES
		// ========================================

		protected static List<IAssignableIdentity> GetLiveMinionsForWorld(int worldId) {
			var result = new List<IAssignableIdentity>();
			foreach (var mi in Components.LiveMinionIdentities.Items) {
				if (mi != null && mi.GetMyWorldId() == worldId)
					result.Add(mi);
			}
			return result;
		}

		protected static List<StoredMinionIdentity> GetStoredMinions() {
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
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			OnTableActivate();
			_sortColumn = -1;
			_sortAscending = false;
			BuildRowList();
			_row = FindInitialRow();
			_col = 0;
			_lastSpokenRow = -1;
			_lastSpokenCol = -1;
			base.OnActivate();
			SpeechPipeline.SpeakQueued(BuildCellParts(forceFullContext: true));
		}

		// ========================================
		// SOUNDS
		// ========================================

		protected void PlayHoverSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Mouseover"));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayHoverSound failed: {ex.Message}");
			}
		}

		protected void PlayWrapSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click"));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayWrapSound failed: {ex.Message}");
			}
		}

		// ========================================
		// SPEECH
		// ========================================

		protected void SpeakCell() {
			if (_row < 0 || _row >= _rows.Count) return;
			SpeechPipeline.SpeakInterrupt(BuildCellParts(forceFullContext: false));
		}

		protected string BuildCellParts(bool forceFullContext) {
			var row = _rows[_row];
			var parts = new List<string>();

			if (forceFullContext || _row != _lastSpokenRow) {
				string rowLabel = GetRowLabel(row);
				if (rowLabel != null)
					parts.Add(rowLabel);
			}

			if (forceFullContext || _col != _lastSpokenCol) {
				string colName = GetColumnName(_col);
				if (colName != null)
					parts.Add(colName);
			}

			parts.Add(GetCellValue(row));

			_lastSpokenRow = _row;
			_lastSpokenCol = _col;

			return string.Join(", ", parts);
		}

		// ========================================
		// NAVIGATION
		// ========================================

		protected void NavigateRow(int direction) {
			BuildRowList();
			int newRow = _row + direction;

			if (newRow < 0 || newRow >= _rows.Count) return;

			if (IsRowSkipped(_rows[newRow].Kind)) {
				string worldName = GetWorldName(_rows[newRow].WorldId);
				int beyondDivider = newRow + direction;
				if (beyondDivider < 0 || beyondDivider >= _rows.Count) return;
				while (beyondDivider >= 0 && beyondDivider < _rows.Count
					&& IsRowSkipped(_rows[beyondDivider].Kind)) {
					worldName = GetWorldName(_rows[beyondDivider].WorldId);
					beyondDivider += direction;
				}
				if (beyondDivider < 0 || beyondDivider >= _rows.Count) return;
				_row = beyondDivider;
				ClampCol();
				PlayHoverSound();
				_lastSpokenRow = -1;
				_lastSpokenCol = -1;
				SpeechPipeline.SpeakInterrupt(
					worldName + ", " + BuildCellParts(forceFullContext: true));
				return;
			}

			_row = newRow;
			ClampCol();
			PlayHoverSound();
			SpeakCell();
		}

		protected void NavigateCol(int direction) {
			var row = _rows[_row];
			int maxCol = GetColumnCount(row.Kind) - 1;
			int newCol = _col + direction;

			if (ColumnWraps(row.Kind)) {
				if (newCol < 0) {
					_col = maxCol;
					PlayWrapSound();
				} else if (newCol > maxCol) {
					_col = 0;
					PlayWrapSound();
				} else {
					_col = newCol;
					PlayHoverSound();
				}
			} else {
				if (newCol < 0 || newCol > maxCol) return;
				_col = newCol;
				PlayHoverSound();
			}

			SpeakCell();
		}

		protected void NavigateHome() {
			BuildRowList();
			for (int i = 0; i < _rows.Count; i++) {
				var kind = _rows[i].Kind;
				if (kind != TableRowKind.Toolbar
					&& kind != TableRowKind.ColumnHeader
					&& !IsRowSkipped(kind)) {
					_row = i;
					PlayHoverSound();
					SpeakCell();
					return;
				}
			}
		}

		protected void NavigateEnd() {
			BuildRowList();
			for (int i = _rows.Count - 1; i >= 0; i--) {
				if (!IsRowSkipped(_rows[i].Kind)) {
					_row = i;
					PlayHoverSound();
					SpeakCell();
					return;
				}
			}
		}

		void ClampCol() {
			int maxCol = GetColumnCount(_rows[_row].Kind) - 1;
			if (_col > maxCol)
				_col = maxCol;
			if (_col < 0)
				_col = 0;
		}

		// ========================================
		// SORT
		// ========================================

		protected void CycleSort() {
			if (!IsColumnSortable(_col)) return;

			string colName = GetColumnName(_col);

			if (_sortColumn != _col) {
				_sortColumn = _col;
				_sortAscending = false;
				SpeechPipeline.SpeakInterrupt(
					colName + ", " + STRINGS.ONIACCESS.TABLE.SORT_DESCENDING);
			} else if (!_sortAscending) {
				_sortAscending = true;
				SpeechPipeline.SpeakInterrupt(
					colName + ", " + STRINGS.ONIACCESS.TABLE.SORT_ASCENDING);
			} else {
				_sortColumn = -1;
				SpeechPipeline.SpeakInterrupt(
					colName + ", " + STRINGS.ONIACCESS.TABLE.SORT_CLEARED);
			}

			BuildRowList();
		}

		// ========================================
		// HELP
		// ========================================

		protected static readonly List<HelpEntry> TableNavHelpEntries = new List<HelpEntry> {
			new HelpEntry("Arrows", STRINGS.ONIACCESS.TABLE.NAVIGATE_TABLE),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.TABLE.JUMP_FIRST_LAST),
		};

		protected static readonly HelpEntry TableSortHelpEntry =
			new HelpEntry("Enter", STRINGS.ONIACCESS.TABLE.SORT_COLUMN);

		// ========================================
		// TICK
		// ========================================

		public override bool Tick() {
			if (base.Tick()) return true;

			bool ctrlHeld = InputUtil.CtrlHeld();

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)) {
				if (ctrlHeld) {
					if (!HandleModifiedUpDown(1)) return true;
				} else {
					NavigateRow(-1);
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) {
				if (ctrlHeld) {
					if (!HandleModifiedUpDown(-1)) return true;
				} else {
					NavigateRow(1);
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow)) {
				if (ctrlHeld) {
					if (!HandleModifiedLeftRight(-1)) return true;
				} else {
					NavigateCol(-1);
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow)) {
				if (ctrlHeld) {
					if (!HandleModifiedLeftRight(1)) return true;
				} else {
					NavigateCol(1);
				}
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
					if (row.Kind == TableRowKind.ColumnHeader)
						CycleSort();
					else
						OnEnterPressed(row);
				}
				return true;
			}

			return false;
		}
	}
}
