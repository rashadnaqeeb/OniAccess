# MinionSelectHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `MinionSelectScreen` (initial colony start) and `ImmigrantScreen` (Printing Pod, recurring every 3 cycles). Two-level navigation:

- **Top level** (Up/Down): Colony name, Shuffle name, Select duplicants, Embark/Back
- **Dupe mode** (Up/Down within slot, Tab/Shift+Tab between slots): name, interests, traits, expectations, attributes, description, interest filter, model filter (DLC3), reroll

Colony name is editable (Enter opens text field, Escape cancels). `CharacterContainer` is a `KScreen` but is NOT pushed to `KScreenManager`; navigation is handled entirely within this handler.

Locked decisions: traits speak full info upfront; attributes speak one per arrow press; after reroll, name and interests are announced automatically.

`CharacterContainer` uses coroutines (`DelayedGeneration`, `SetAttributes`) that take multiple frames; `MaxDiscoveryRetries` is 10.

---

## class MinionSelectHandler : BaseWidgetHandler (line 32)

  **Fields**
  - `private int _currentSlot` (line 33)
  - `private UnityEngine.Component[] _containers` (line 34) — array of active `CharacterContainer` instances
  - `private System.Action _pendingAnnounce` (line 35) — deferred one-frame callback (used after reroll, filter changes, shuffles)
  - `private bool _pendingColonyNameAnnounce` (line 36) — set true when colony name field is empty at discovery time; re-announces once BaseNaming populates it
  - `private bool _inDupeMode` (line 37)
  - `private static readonly System.Type MinionSelectScreenType` (line 38)

  **Properties**
  - `private bool IsMinionSelectScreen` (line 45) — true when the screen is MinionSelectScreen (has colony naming); ImmigrantScreen does not
  - `protected override int MaxDiscoveryRetries => 10` (line 52)
  - `public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.MINION_SELECT` (line 54)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 56)

  **Constructor**
  - `public MinionSelectHandler(KScreen screen) : base(screen)` (line 58)

  **Lifecycle**
  - `public override void OnActivate()` (line 64) — resets dupe mode and pending state before base

  **Tab Navigation**
  - `protected override void NavigateTabForward()` (line 75) — only active in dupe mode; cycles to next slot
  - `protected override void NavigateTabBackward()` (line 83)
  - `private void RediscoverAndSpeakSlot()` (line 92) — rediscovers for current slot, interrupts with "Slot N, [first widget]"

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 105) — dispatches to dupe mode or top-level discovery
  - `private bool DiscoverTopLevelWidgets(KScreen screen)` (line 118) — colony name (BaseNaming.inputField), shuffle name button, "Select duplicants" virtual button, Embark/proceed button, Back button (MinionSelectScreen only)
  - `private bool DiscoverDupeModeWidgets(KScreen screen)` (line 220) — finds containers, checks that `stats` field is populated (coroutine guard), then calls `DiscoverSlotWidgets`
  - `private void DiscoverSlotWidgets(CharacterContainer container)` (line 258) — builds full widget list for one slot in order: name, interests, traits, expectations, attributes, description, interest filter, model filter (DLC3), reroll button
  - `private void DiscoverNameWidget(CharacterContainer container, Traverse traverse)` (line 294) — reads `characterNameTitle.titleText`; also adds bionic model type label if applicable; adds rename button and shuffle name button
  - `private void DiscoverInterestsWidget(CharacterContainer container, Traverse traverse)` (line 349) — reads `aptitudeEntries`; combines all LocTexts per entry into one label
  - `private void DiscoverTraitWidgets(CharacterContainer container, Traverse traverse)` (line 386) — reads `MinionStartingStats.Traits` starting at index 1; labels as positive/negative trait or bionic upgrade/bug; flattens tooltip into label
  - `private void DiscoverExpectationWidgets(CharacterContainer container, Traverse traverse)` (line 431) — reads `expectationLabels`; appends tooltip text
  - `private void DiscoverDescriptionWidget(CharacterContainer container, Traverse traverse)` (line 463)
  - `private void DiscoverAttributeWidgets(CharacterContainer container, Traverse traverse)` (line 478) — reads `iconGroups`; appends flattened tooltip
  - `private void DiscoverFilterDropdown(CharacterContainer container, Traverse traverse)` (line 514) — reads `archetypeDropDown`; `SpeechFunc` re-reads live state
  - `private string GetInterestFilterLabel(CharacterContainer container)` (line 532) — reads `guaranteedAptitudeID` from container; resolves to SkillGroup name; returns "Any" if empty
  - `private void DiscoverModelDropdown(CharacterContainer container, Traverse traverse)` (line 818) — reads `modelDropDown`; DLC3 only
  - `private string GetModelFilterLabel()` (line 835) — reads `permittedModels` from current slot; returns "All", "Bionic", or "Standard"
  - `private void DiscoverRerollButton(CharacterContainer container, Traverse traverse)` (line 599)

  **Widget Speech Overrides**
  - `protected override bool IsWidgetValid(Widget widget)` (line 626) — allows `dupe_shuffle_name` button to be navigated even when its GameObject is inactive (game hides it by default)
  - `protected override string GetTooltipText(Widget widget)` (line 637) — suppresses auto-tooltip for widgets that already bake tooltip into their label, or where auto-discovery picks up a wrong tooltip

  **Widget Activation**
  - `protected override void ActivateCurrentItem()` (line 662) — handles: enter_dupe_mode (switches mode), colony_name (text edit via base), colony_shuffle (click + defer announce), dupe_rename (TextEdit on EditableTitleBar), dupe_shuffle_name (click + defer), reroll (click + defer); else base
  - `private void AnnounceAfterReroll()` (line 733) — rediscovers, repositions cursor to reroll button, announces name and interests
  - `private void AnnounceAfterColonyShuffle()` (line 739) — reads new colony name from BaseNaming.inputField and interrupts
  - `private void AnnounceAfterDupeShuffle()` (line 756) — rediscovers, repositions to shuffle button, speaks first widget
  - `private int FindWidgetByTag(string targetTag)` (line 767) — linear scan returning matching widget index or clamped fallback
  - `private void AnnounceNameAndInterests()` (line 779) — interrupts with first widget (name), then queues interest-tagged widgets
  - `private void QueueNameAndInterests(bool includeName = true)` (line 789) — queues name (if includeName) and all interest-tagged widgets; does not change `_currentIndex`
  - `private void AnnounceAfterFilterChange()` (line 804) — rediscovers, repositions to filter widget, interrupts with new filter label, queues name and interests

  **Dropdown Cycling**
  - `protected override void CycleDropdown(Widget widget, int direction)` (line 549) — routes to `CycleModelDropdown` for model_filter tag, or cycles `archetypeDropDown` for interest_filter; invokes `onEntrySelectedAction` directly; sets `_pendingAnnounce`
  - `private void CycleModelDropdown(int direction)` (line 855) — same pattern as CycleDropdown for the model dropdown
  - `private void AnnounceAfterModelChange()` (line 897) — rediscovers, repositions to model_filter, interrupts and queues

  **Key Handling**
  - `public override bool HandleKeyDown(KButtonEvent e)` (line 912) — intercepts Escape in dupe mode to exit back to top level

  **Tick**
  - `public override bool Tick()` (line 935) — handles `_pendingColonyNameAnnounce` (re-announces when BaseNaming populates the field); fires `_pendingAnnounce` one frame after it was set; then delegates to base
