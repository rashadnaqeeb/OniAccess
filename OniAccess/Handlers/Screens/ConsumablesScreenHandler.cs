using System.Collections.Generic;

using Klei.AI;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// 2D grid handler for the ConsumablesTableScreen (duplicant food/medicine permissions).
	///
	/// Builds a virtual table from live game state on every navigation event.
	/// Column 0 is QoL Expectations; remaining columns are discovered consumables
	/// sorted by MajorOrder/MinorOrder (food by quality, then batteries, then medicine).
	///
	/// Enter on a column header triggers the super-checkbox (toggle all duplicants
	/// for that consumable). Enter on a data row toggles the individual permission.
	/// </summary>
	public class ConsumablesScreenHandler : BaseTableHandler {
		struct ColumnDef {
			public string Name;
			public IConsumableUIItem ConsumableInfo;
		}

		List<ColumnDef> _columns = new List<ColumnDef>();

		public override string DisplayName => STRINGS.ONIACCESS.CONSUMABLES_SCREEN.HANDLER_NAME;

		public ConsumablesScreenHandler(KScreen screen) : base(screen) { }

		// ========================================
		// HELP
		// ========================================

		static readonly List<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Arrows", STRINGS.ONIACCESS.TABLE.NAVIGATE_TABLE),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.TABLE.JUMP_FIRST_LAST),
			new HelpEntry("Enter (header)", STRINGS.ONIACCESS.CONSUMABLES_SCREEN.TOGGLE_ALL),
			new HelpEntry("Enter (data row)", STRINGS.ONIACCESS.CONSUMABLES_SCREEN.TOGGLE_PERMISSION),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		// ========================================
		// COLUMN BUILDING
		// ========================================

		void BuildColumnList() {
			_columns.Clear();

			_columns.Add(new ColumnDef {
				Name = STRINGS.UI.VITALSSCREEN.QUALITYOFLIFE_EXPECTATIONS,
				ConsumableInfo = null
			});

			var consumables = new List<IConsumableUIItem>();

			foreach (var food in EdiblesManager.GetAllFoodTypes())
				consumables.Add(food);

			foreach (var prefab in Assets.GetPrefabsWithTag(GameTags.Medicine)) {
				var pill = prefab.GetComponent<MedicinalPillWorkable>();
				if (pill != null)
					consumables.Add(pill);
			}

			if (Game.IsDlcActiveForCurrentSave("DLC3_ID")) {
				var incompatible = new HashSet<Tag>(GameTags.BionicIncompatibleBatteries);
				foreach (var prefab in Assets.GetPrefabsWithTag(GameTags.ChargedPortableBattery)) {
					if (prefab.HasTag(GameTags.DeprecatedContent)) continue;
					bool isIncompatible = false;
					foreach (var tag in incompatible) {
						if (prefab.HasTag(tag)) {
							isIncompatible = true;
							break;
						}
					}
					if (isIncompatible) continue;
					var bank = prefab.GetComponent<Electrobank>();
					if (bank != null)
						consumables.Add(bank);
				}

				consumables.Add(new SymbolicConsumableItem(
					ConsumerManager.OXYGEN_TANK_ID,
					STRINGS.MISC.TAGS.OXYGENCANISTER, 1, 1, true, "ui_sprite_oxygen_canister",
					delegate {
						foreach (MinionIdentity mi in Components.LiveMinionIdentities)
							if (mi.HasTag(GameTags.Minions.Models.Bionic))
								return true;
						return false;
					}));
			}

			consumables.Sort((a, b) => {
				int cmp = a.MajorOrder.CompareTo(b.MajorOrder);
				if (cmp == 0)
					cmp = a.MinorOrder.CompareTo(b.MinorOrder);
				return cmp;
			});

			foreach (var item in consumables) {
				if (!item.Display) continue;
				if (!DebugHandler.InstantBuildMode && !item.RevealTest()) continue;
				_columns.Add(new ColumnDef {
					Name = item.ConsumableName,
					ConsumableInfo = item
				});
			}
		}

		// ========================================
		// ROW LIST BUILDING
		// ========================================

		protected override int FindInitialRow() {
			for (int i = 0; i < _rows.Count; i++) {
				if (_rows[i].Kind == TableRowKind.Minion)
					return i;
			}
			return base.FindInitialRow();
		}

		protected override void BuildRowList() {
			BuildColumnList();
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

			_rows.Add(new RowEntry { Kind = TableRowKind.Default });
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
				case TableRowKind.Default:
					return STRINGS.UI.CONSUMABLESSCREEN.TITLE;
				default:
					return null;
			}
		}

		protected override string GetCellValue(RowEntry row) {
			if (_col < 0 || _col >= _columns.Count)
				return "";

			var colDef = _columns[_col];

			switch (row.Kind) {
				case TableRowKind.ColumnHeader:
					return colDef.Name;

				case TableRowKind.Minion:
					if (colDef.ConsumableInfo == null)
						return GetQoLValue((MinionIdentity)row.Identity);
					return GetConsumableValue((MinionIdentity)row.Identity, colDef.ConsumableInfo);

				case TableRowKind.StoredMinion: {
					var smi = (StoredMinionIdentity)row.Identity;
					string reason = TextFilter.FilterForSpeech(string.Format(
						STRINGS.UI.TABLESCREENS.INFORMATION_NOT_AVAILABLE_TOOLTIP,
						smi.GetStorageReason(), smi.GetProperName()));
					return STRINGS.UI.TABLESCREENS.NA + ", " + reason;
				}

				case TableRowKind.Default:
					if (colDef.ConsumableInfo == null)
						return "";
					return GetDefaultValue(colDef.ConsumableInfo);

				default:
					return "";
			}
		}

		// ========================================
		// WORLD NAME
		// ========================================

		protected override string GetWorldName(int worldId) {
			if (worldId == 255) return STRINGS.ONIACCESS.CONSUMABLES_SCREEN.STORED;
			return base.GetWorldName(worldId);
		}

		// ========================================
		// CELL VALUE BUILDERS
		// ========================================

		static string GetQoLValue(MinionIdentity mi) {
			var attr = Db.Get().Attributes.QualityOfLife.Lookup(mi);
			return attr.GetFormattedValue();
		}

		static string GetConsumableValue(MinionIdentity mi, IConsumableUIItem consumable) {
			var consumer = mi.GetComponent<ConsumableConsumer>();
			string id = consumable.ConsumableId;

			if (consumer.IsDietRestricted(id))
				return (string)STRINGS.ONIACCESS.CONSUMABLES_SCREEN.RESTRICTED;

			bool permitted = consumer.IsPermitted(id);
			string state = permitted
				? (string)STRINGS.ONIACCESS.CONSUMABLES_SCREEN.PERMITTED
				: (string)STRINGS.ONIACCESS.CONSUMABLES_SCREEN.FORBIDDEN;

			var foodInfo = consumable as EdiblesManager.FoodInfo;
			if (foodInfo != null) {
				int adjustedQuality = foodInfo.Quality
					+ UnityEngine.Mathf.RoundToInt(
						mi.GetAttributes().Get(Db.Get().Attributes.FoodExpectation).GetTotalValue());
				string effectId = Edible.GetEffectForFoodQuality(adjustedQuality);
				int morale = 0;
				foreach (var mod in Db.Get().effects.Get(effectId).SelfModifiers) {
					if (mod.AttributeId == Db.Get().Attributes.QualityOfLife.Id)
						morale += UnityEngine.Mathf.RoundToInt(mod.Value);
				}
				string qualityDesc = GameUtil.GetFormattedFoodQuality(foodInfo.Quality);
				string moraleStr = GameUtil.AddPositiveSign(morale.ToString(), morale > 0);
				return state + ", " + TextFilter.FilterForSpeech(qualityDesc)
					+ ", " + string.Format(STRINGS.ONIACCESS.CONSUMABLES_SCREEN.MORALE, moraleStr);
			}

			return state;
		}

		static string GetDefaultValue(IConsumableUIItem consumable) {
			bool forbidden = ConsumerManager.instance.DefaultForbiddenTagsList
				.Contains(consumable.ConsumableId.ToTag());
			return forbidden
				? (string)STRINGS.ONIACCESS.CONSUMABLES_SCREEN.FORBIDDEN
				: (string)STRINGS.ONIACCESS.CONSUMABLES_SCREEN.PERMITTED;
		}

		// ========================================
		// ENTER
		// ========================================

		protected override void OnEnterPressed(RowEntry row) {
			if (_col < 0 || _col >= _columns.Count) return;
			var colDef = _columns[_col];
			if (colDef.ConsumableInfo == null) return;

			switch (row.Kind) {
				case TableRowKind.Minion:
					ToggleMinionPermission(row, colDef.ConsumableInfo);
					break;
				case TableRowKind.Default:
					ToggleDefaultPermission(colDef.ConsumableInfo);
					break;
				case TableRowKind.StoredMinion:
					SpeakCell();
					break;
			}
		}

		void ToggleMinionPermission(RowEntry row, IConsumableUIItem consumable) {
			var mi = (MinionIdentity)row.Identity;
			var consumer = mi.GetComponent<ConsumableConsumer>();
			string id = consumable.ConsumableId;

			if (consumer.IsDietRestricted(id)) {
				SpeakCell();
				return;
			}

			bool newState = !consumer.IsPermitted(id);
			consumer.SetPermitted(id, newState);
			PlayWrapSound();
			SpeakCell();
		}

		void ToggleDefaultPermission(IConsumableUIItem consumable) {
			var tag = consumable.ConsumableId.ToTag();
			var list = ConsumerManager.instance.DefaultForbiddenTagsList;
			if (list.Contains(tag))
				list.Remove(tag);
			else
				list.Add(tag);
			PlayWrapSound();
			SpeakCell();
		}

		// ========================================
		// SUPER-CHECKBOX (HEADER ENTER)
		// ========================================

		public override bool Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
				if (_row >= 0 && _row < _rows.Count
					&& _rows[_row].Kind == TableRowKind.ColumnHeader) {
					ToggleColumn();
					return true;
				}
			}
			return base.Tick();
		}

		void ToggleColumn() {
			if (_col < 0 || _col >= _columns.Count) return;
			var colDef = _columns[_col];
			if (colDef.ConsumableInfo == null) return;

			string id = colDef.ConsumableInfo.ConsumableId;
			bool allPermitted = true;
			bool anyToggleable = false;

			foreach (var row in _rows) {
				if (row.Kind != TableRowKind.Minion) continue;
				var consumer = ((MinionIdentity)row.Identity).GetComponent<ConsumableConsumer>();
				if (consumer.IsDietRestricted(id)) continue;
				anyToggleable = true;
				if (!consumer.IsPermitted(id)) {
					allPermitted = false;
					break;
				}
			}

			if (!anyToggleable) return;

			bool newState = !allPermitted;

			var tag = colDef.ConsumableInfo.ConsumableId.ToTag();
			var defaultList = ConsumerManager.instance.DefaultForbiddenTagsList;
			if (newState)
				defaultList.Remove(tag);
			else if (!defaultList.Contains(tag))
				defaultList.Add(tag);

			foreach (var row in _rows) {
				if (row.Kind != TableRowKind.Minion) continue;
				var consumer = ((MinionIdentity)row.Identity).GetComponent<ConsumableConsumer>();
				if (consumer.IsDietRestricted(id)) continue;
				consumer.SetPermitted(id, newState);
			}

			PlayWrapSound();
			SpeechPipeline.SpeakInterrupt(newState
				? (string)STRINGS.ONIACCESS.CONSUMABLES_SCREEN.ALL_PERMITTED
				: (string)STRINGS.ONIACCESS.CONSUMABLES_SCREEN.ALL_FORBIDDEN);
		}
	}
}
