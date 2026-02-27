# VitalsScreenHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

2D grid handler for `VitalsTableScreen` (duplicant health stats). Builds a virtual table from live game state on every navigation event. Column list is built once on activation.

Columns: Stress, Morale (QualityOfLife), Power Banks (DLC3 only, bionic dupes), Fullness (Calories), Health (HitPoints), Disease (Sickness). Each column's value is assembled programmatically from game Amount/Attribute data with delta + modifier sources rather than using pre-formatted game tooltips.

Rows: ColumnHeader, World Dividers (Spaced Out only), Minions, Stored Minions. No Toolbar row (unlike PriorityScreen). Enter on a Minion row focuses the camera on that duplicant.

---

## class VitalsScreenHandler : BaseTableHandler (line 20)

  **Private Struct**
  - `struct ColumnDef` (line 21)
    - `public string Name` (line 22)
    - `public string HeaderDescription` (line 23)
    - `public Func<MinionIdentity, string> GetValue` (line 24)
    - `public Func<MinionIdentity, float> GetSortValue` (line 25)
    - `public bool Sortable` (line 26)

  **Fields**
  - `List<ColumnDef> _columns` (line 29)

  **Properties**
  - `public override string DisplayName => STRINGS.ONIACCESS.VITALS_SCREEN.HANDLER_NAME` (line 31)
  - `static readonly List<HelpEntry> _helpEntries` (line 39) — extends `TableNavHelpEntries` with `TableSortHelpEntry` and an Enter (data row) entry for duplicant focus
  - `public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries` (line 44)

  **Constructor**
  - `public VitalsScreenHandler(KScreen screen) : base(screen)` (line 33)

  **Table Setup**
  - `protected override void OnTableActivate()` (line 50) — builds `_columns` list: Stress, Morale, Power Banks (DLC3 conditional), Fullness, Health, Disease; each with `Name`, `HeaderDescription`, `GetValue` lambda, `GetSortValue` lambda, `Sortable` flag

  **Row List Building**
  - `protected override void BuildRowList()` (line 118) — adds ColumnHeader; iterates worlds with world dividers (Spaced Out); sorts minions by active sort column; adds StoredMinion rows

  **Table Shape**
  - `protected override int GetColumnCount(TableRowKind kind)` (line 166) — always `_columns.Count`
  - `protected override string GetColumnName(int col)` (line 170) — null for ColumnHeader row; otherwise `_columns[col].Name`
  - `protected override string GetRowLabel(RowEntry row)` (line 178) — null for ColumnHeader; dupe name for Minion/StoredMinion
  - `protected override string GetCellValue(RowEntry row)` (line 190) — ColumnHeader: "Name, description"; Minion: calls column's `GetValue` lambda; StoredMinion: "N/A, [storage reason tooltip]"
  - `protected override bool IsColumnSortable(int col)` (line 219)

  **Enter**
  - `protected override void OnEnterPressed(RowEntry row)` (line 229) — only acts on Minion rows; calls `SelectTool.Instance.SelectAndFocus` to move the camera to the dupe and announces "focused"

  **Amount Breakdown Helper**
  - `static string FormatAmountBreakdown(AmountInstance amount)` (line 243) — formats delta rate + all modifier descriptions into a period-separated string

  **Column Value Builders**
  - `static string GetStressValue(MinionIdentity mi)` (line 263) — stress value + amount breakdown
  - `static string GetMoraleValue(MinionIdentity mi)` (line 268) — QoL attribute formatted value + all modifiers with descriptions
  - `static string GetPowerBanksValue(MinionIdentity mi)` (line 283) — "N/A" for non-bionic; battery amount + current wattage label + non-zero modifier names for bionic
  - `static string GetFullnessValue(MinionIdentity mi)` (line 305) — calorie value + amount breakdown + rations eaten today
  - `static string GetHealthValue(MinionIdentity mi)` (line 321) — HP value + amount breakdown
  - `static string GetSicknessValue(MinionIdentity mi)` (line 326) — sickness label + detail tooltip text

  **Sickness Label**
  - `static string GetSicknessLabel(MinionIdentity mi)` (line 338) — builds list of active sicknesses (including radiation sickness); returns "no sicknesses" / multiple sicknesses format / single sickness name + time remaining
  - `static string GetSicknessDetail(MinionIdentity mi)` (line 382) — returns radiation effect tooltip + sickness status item tooltips joined with commas; null if no sicknesses
