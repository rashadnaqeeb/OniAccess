using System;
using System.Collections.Generic;

using Klei.AI;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// 2D grid handler for the VitalsTableScreen (duplicant health stats).
	///
	/// Builds a virtual table from live game state on every navigation event.
	/// Column list is built once on activation; DLC3 Power Banks column is
	/// conditionally included.
	/// </summary>
	public class VitalsScreenHandler : BaseTableHandler {
		struct ColumnDef {
			public string Name;
			public Func<MinionIdentity, string> GetValue;
			public Func<MinionIdentity, string> GetTooltip;
			public Func<MinionIdentity, float> GetSortValue;
			public bool Sortable;
		}

		List<ColumnDef> _columns;

		public override string DisplayName => STRINGS.ONIACCESS.VITALS_SCREEN.HANDLER_NAME;

		public VitalsScreenHandler(KScreen screen) : base(screen) { }

		// ========================================
		// HELP
		// ========================================

		static readonly List<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Arrows", STRINGS.ONIACCESS.TABLE.NAVIGATE_TABLE),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.TABLE.JUMP_FIRST_LAST),
			new HelpEntry("Enter", STRINGS.ONIACCESS.TABLE.SORT_COLUMN),
			new HelpEntry("Enter (data row)", STRINGS.ONIACCESS.VITALS_SCREEN.FOCUS_DUPLICANT),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		// ========================================
		// TABLE SETUP
		// ========================================

		protected override void OnTableActivate() {
			_columns = new List<ColumnDef> {
				new ColumnDef {
					Name = STRINGS.UI.VITALSSCREEN.STRESS,
					GetValue = mi => Db.Get().Amounts.Stress.Lookup(mi).GetValueString(),
					GetTooltip = mi => Db.Get().Amounts.Stress.Lookup(mi).GetTooltip(),
					GetSortValue = mi => Db.Get().Amounts.Stress.Lookup(mi).value,
					Sortable = true
				},
				new ColumnDef {
					Name = STRINGS.UI.VITALSSCREEN.QUALITYOFLIFE_EXPECTATIONS,
					GetValue = mi => Db.Get().Attributes.QualityOfLife.Lookup(mi).GetFormattedValue(),
					GetTooltip = mi => Db.Get().Attributes.QualityOfLife.Lookup(mi).GetAttributeValueTooltip(),
					GetSortValue = mi => Db.Get().Attributes.QualityOfLifeExpectation.Lookup(mi).GetTotalValue(),
					Sortable = true
				},
			};

			if (Game.IsDlcActiveForCurrentSave("DLC3_ID")) {
				_columns.Add(new ColumnDef {
					Name = STRINGS.UI.VITALSSCREEN_POWERBANKS,
					GetValue = mi => {
						if (mi.HasTag(GameTags.Minions.Models.Bionic))
							return GameUtil.GetFormattedJoules(
								mi.GetAmounts().Get(Db.Get().Amounts.BionicInternalBattery).value);
						return STRINGS.UI.TABLESCREENS.NA;
					},
					GetTooltip = mi => {
						if (mi.HasTag(GameTags.Minions.Models.Bionic))
							return mi.GetAmounts().Get(Db.Get().Amounts.BionicInternalBattery).GetDescription();
						return null;
					},
					GetSortValue = mi => {
						if (mi.HasTag(GameTags.Minions.Models.Bionic))
							return mi.GetAmounts().Get(Db.Get().Amounts.BionicInternalBattery).value;
						return -1f;
					},
					Sortable = true
				});
			}

			_columns.Add(new ColumnDef {
				Name = STRINGS.UI.VITALSSCREEN_CALORIES,
				GetValue = mi => {
					var amount = Db.Get().Amounts.Calories.Lookup(mi);
					return amount != null ? amount.GetValueString() : (string)STRINGS.UI.TABLESCREENS.NA;
				},
				GetTooltip = mi => {
					var amount = Db.Get().Amounts.Calories.Lookup(mi);
					if (amount == null) return null;
					string tip = amount.GetTooltip();
					var ration = mi.GetSMI<RationMonitor.Instance>();
					if (ration != null) {
						tip += ", " + string.Format(STRINGS.UI.VITALSSCREEN.EATEN_TODAY_TOOLTIP,
							GameUtil.GetFormattedCalories(ration.GetRationsAteToday()));
					}
					return tip;
				},
				GetSortValue = mi => {
					var amount = Db.Get().Amounts.Calories.Lookup(mi);
					return amount != null ? amount.value : -1f;
				},
				Sortable = true
			});

			_columns.Add(new ColumnDef {
				Name = STRINGS.UI.VITALSSCREEN_HEALTH,
				GetValue = mi => Db.Get().Amounts.HitPoints.Lookup(mi).GetValueString(),
				GetTooltip = mi => Db.Get().Amounts.HitPoints.Lookup(mi).GetTooltip(),
				GetSortValue = mi => Db.Get().Amounts.HitPoints.Lookup(mi).value,
				Sortable = true
			});

			_columns.Add(new ColumnDef {
				Name = STRINGS.UI.VITALSSCREEN_SICKNESS,
				GetValue = mi => GetSicknessLabel(mi),
				GetTooltip = mi => GetSicknessTooltip(mi),
				GetSortValue = null,
				Sortable = false
			});
		}

		protected override int FindInitialRow() {
			for (int i = 0; i < _rows.Count; i++) {
				if (_rows[i].Kind == TableRowKind.Minion)
					return i;
			}
			return base.FindInitialRow();
		}

		// ========================================
		// ROW LIST BUILDING
		// ========================================

		protected override void BuildRowList() {
			_rows.Clear();
			bool showDividers = DlcManager.FeatureClusterSpaceEnabled();

			_rows.Add(new RowEntry { Kind = TableRowKind.ColumnHeader });

			var worldIds = ClusterManager.Instance.GetWorldIDsSorted();
			foreach (int worldId in worldIds) {
				var world = ClusterManager.Instance.GetWorld(worldId);
				if (world == null || !world.IsDiscovered) continue;

				var minions = GetLiveMinionsForWorld(worldId);
				if (minions.Count == 0) continue;

				if (showDividers)
					_rows.Add(new RowEntry { Kind = TableRowKind.WorldDivider, WorldId = worldId });

				if (_sortColumn >= 0 && _sortColumn < _columns.Count && _columns[_sortColumn].Sortable) {
					var colDef = _columns[_sortColumn];
					minions.Sort((a, b) => {
						var miA = (MinionIdentity)a;
						var miB = (MinionIdentity)b;
						int cmp = colDef.GetSortValue(miA).CompareTo(colDef.GetSortValue(miB));
						if (!_sortAscending) cmp = -cmp;
						if (cmp != 0) return cmp;
						return a.GetProperName().CompareTo(b.GetProperName());
					});
				}

				foreach (var minion in minions) {
					_rows.Add(new RowEntry { Kind = TableRowKind.Minion, Identity = minion });
				}
			}

			var stored = GetStoredMinions();
			if (stored.Count > 0) {
				if (showDividers)
					_rows.Add(new RowEntry { Kind = TableRowKind.WorldDivider, WorldId = 255 });
				foreach (var smi in stored) {
					_rows.Add(new RowEntry { Kind = TableRowKind.StoredMinion, Identity = smi });
				}
			}
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
		// TABLE SHAPE
		// ========================================

		protected override int GetColumnCount(TableRowKind kind) {
			return _columns.Count;
		}

		protected override string GetColumnName(int col) {
			if (_rows[_row].Kind == TableRowKind.ColumnHeader)
				return null;
			if (col >= 0 && col < _columns.Count)
				return _columns[col].Name;
			return null;
		}

		protected override string GetRowLabel(RowEntry row) {
			switch (row.Kind) {
				case TableRowKind.ColumnHeader:
					return null;
				case TableRowKind.Minion:
				case TableRowKind.StoredMinion:
					return row.Identity.GetProperName();
				default:
					return null;
			}
		}

		protected override string GetCellValue(RowEntry row) {
			switch (row.Kind) {
				case TableRowKind.ColumnHeader:
					if (_col >= 0 && _col < _columns.Count)
						return _columns[_col].Name;
					return "";

				case TableRowKind.Minion: {
					var mi = (MinionIdentity)row.Identity;
					if (_col >= 0 && _col < _columns.Count)
						return _columns[_col].GetValue(mi);
					return "";
				}

				case TableRowKind.StoredMinion:
					return STRINGS.UI.TABLESCREENS.NA;

				default:
					return "";
			}
		}

		protected override bool IsColumnSortable(int col) {
			if (col >= 0 && col < _columns.Count)
				return _columns[col].Sortable;
			return false;
		}

		// ========================================
		// TOOLTIP
		// ========================================

		protected override string GetCellTooltip(RowEntry row) {
			if (row.Kind == TableRowKind.StoredMinion) {
				var smi = (StoredMinionIdentity)row.Identity;
				return TextFilter.FilterForSpeech(string.Format(
					STRINGS.UI.TABLESCREENS.INFORMATION_NOT_AVAILABLE_TOOLTIP,
					smi.GetStorageReason(), smi.GetProperName()));
			}
			if (row.Kind != TableRowKind.Minion) return null;
			if (_col < 0 || _col >= _columns.Count) return null;
			var getTooltip = _columns[_col].GetTooltip;
			if (getTooltip == null) return null;
			string tip = getTooltip((MinionIdentity)row.Identity);
			if (string.IsNullOrEmpty(tip)) return null;
			return TextFilter.FilterForSpeech(tip);
		}

		// ========================================
		// WORLD NAME
		// ========================================

		protected override string GetWorldName(int worldId) {
			if (worldId == 255) return STRINGS.ONIACCESS.VITALS_SCREEN.STORED;
			return base.GetWorldName(worldId);
		}

		// ========================================
		// ENTER
		// ========================================

		protected override void OnEnterPressed(RowEntry row) {
			if (row.Kind != TableRowKind.Minion) return;
			var mi = (MinionIdentity)row.Identity;
			SelectTool.Instance.SelectAndFocus(
				mi.transform.GetPosition(),
				mi.GetComponent<KSelectable>(),
				new UnityEngine.Vector3(8f, 0f, 0f));
			SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.VITALS_SCREEN.FOCUSED);
		}

		// ========================================
		// SICKNESS LABEL
		// ========================================

		static string GetSicknessLabel(MinionIdentity mi) {
			var sicknessList = new List<KeyValuePair<string, float>>();

			foreach (SicknessInstance sickness in mi.GetComponent<MinionModifiers>().sicknesses) {
				sicknessList.Add(new KeyValuePair<string, float>(
					sickness.modifier.Name, sickness.GetInfectedTimeRemaining()));
			}

			if (DlcManager.FeatureRadiationEnabled()) {
				var radMonitor = mi.GetSMI<RadiationMonitor.Instance>();
				if (radMonitor != null && radMonitor.sm.isSick.Get(radMonitor)) {
					var effects = mi.GetComponent<Effects>();
					string radName;
					if (effects.HasEffect(RadiationMonitor.minorSicknessEffect)
						|| effects.HasEffect(RadiationMonitor.bionic_minorSicknessEffect))
						radName = Db.Get().effects.Get(RadiationMonitor.minorSicknessEffect).Name;
					else if (effects.HasEffect(RadiationMonitor.majorSicknessEffect)
						|| effects.HasEffect(RadiationMonitor.bionic_majorSicknessEffect))
						radName = Db.Get().effects.Get(RadiationMonitor.majorSicknessEffect).Name;
					else if (effects.HasEffect(RadiationMonitor.extremeSicknessEffect)
						|| effects.HasEffect(RadiationMonitor.bionic_extremeSicknessEffect))
						radName = Db.Get().effects.Get(RadiationMonitor.extremeSicknessEffect).Name;
					else
						radName = STRINGS.DUPLICANTS.MODIFIERS.RADIATIONEXPOSUREDEADLY.NAME;
					sicknessList.Add(new KeyValuePair<string, float>(
						radName, radMonitor.SicknessSecondsRemaining()));
				}
			}

			if (sicknessList.Count == 0)
				return STRINGS.UI.VITALSSCREEN.NO_SICKNESSES;

			if (sicknessList.Count > 1) {
				float minTime = float.MaxValue;
				foreach (var item in sicknessList)
					minTime = UnityEngine.Mathf.Min(minTime, item.Value);
				return string.Format(STRINGS.UI.VITALSSCREEN.MULTIPLE_SICKNESSES,
					GameUtil.GetFormattedCycles(minTime));
			}

			return string.Format(STRINGS.UI.VITALSSCREEN.SICKNESS_REMAINING,
				sicknessList[0].Key, GameUtil.GetFormattedCycles(sicknessList[0].Value));
		}

		static string GetSicknessTooltip(MinionIdentity mi) {
			var parts = new List<string>();

			if (DlcManager.FeatureRadiationEnabled()) {
				var radMonitor = mi.GetSMI<RadiationMonitor.Instance>();
				if (radMonitor != null && radMonitor.sm.isSick.Get(radMonitor))
					parts.Add(radMonitor.GetEffectStatusTooltip());
			}

			var sicknesses = mi.GetComponent<MinionModifiers>().sicknesses;
			if (sicknesses.IsInfected()) {
				foreach (SicknessInstance item in sicknesses) {
					parts.Add(item.modifier.Name);
					var statusItem = item.GetStatusItem();
					parts.Add(statusItem.GetTooltip(item.ExposureInfo));
				}
			}

			if (parts.Count == 0)
				return null;

			return string.Join(", ", parts);
		}
	}
}
