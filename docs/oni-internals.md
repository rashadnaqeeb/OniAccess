# ONI Game Internals Reference

Technical reference for Oxygen Not Included's internal APIs, discovered through decompilation and mod development. Useful for any ONI mod that interacts with the UI or game systems.

## Table of Contents

- [Screen System](#screen-system)
- [Screen Lifecycle](#screen-lifecycle)
- [Input System](#input-system)
- [UI Components](#ui-components)
- [Text and Localization](#text-and-localization)
- [HierarchyReferences](#hierarchyreferences)
- [Game Databases](#game-databases)
- [Mod Entry Point](#mod-entry-point)
- [Harmony Patching Patterns](#harmony-patching-patterns)
- [Known Gotchas](#known-gotchas)

---

## Screen System

ONI manages UI screens through `KScreenManager`, which maintains a private stack of `KScreen` instances.

### KScreenManager

- **Singleton:** `KScreenManager.Instance`
- **Screen stack:** private field `screenStack` (`List<KScreen>`), requires reflection:
  ```csharp
  typeof(KScreenManager).GetField("screenStack", BindingFlags.Instance | BindingFlags.NonPublic);
  ```
- Stack ordering: index 0 = bottom, `Count - 1` = topmost
- Only modal screens block input to screens below them
- **Note:** Do not poll the screen stack to determine input ownership. Use a registration chain or state machine to manage which handler owns input at any given time. The stack is useful for reading screen state, not for input dispatch.

### KScreen

Base class for all ONI screens. Key members:

| Member | Type | Notes |
|--------|------|-------|
| `IsScreenActive()` | method | Whether the screen is currently active |
| `IsModal()` | method | Whether it blocks input to screens below |
| `Deactivate()` | method | Hide/close the screen |
| `Show(bool)` | method | Show or hide |
| `isActiveAndEnabled` | property | Unity's active check |

### Known Screen Classes

**Frontend menus:**
- `MainMenu` - Main menu
- `OptionsMenuScreen` - Options hub
- `GameOptionsScreen` - Game settings
- `AudioOptionsScreen` - Audio settings
- `GraphicsOptionsScreen` - Graphics settings (may not exist in all versions; resolve dynamically)
- `ModsScreen` - Mod management
- `LanguageOptionsScreen` - Language selection
- `LoadScreen` - Save file loading
- `InputBindingsScreen` - Key binding configuration

**Dialogs:**
- `ConfirmDialogScreen` - Confirmation prompts
- `MetricsOptionsScreen` - Analytics opt-in
- `FeedbackScreen` - Bug report / feedback
- `CreditsScreen` - Game credits
- `VideoScreen` - Video playback

**New game flow:**
- `ModeSelectScreen` - Survival vs No Sweat
- `ClusterCategorySelectionScreen` - World style (vanilla, classic, spaced out, event)
- `ColonyDestinationSelectScreen` - Asteroid/world selection (complex, multi-panel)
- `WorldGenScreen` - World generation progress

**Gameplay:**
- `Game` - Main game controller
- `SpeedControlScreen` - Pause/speed controls

---

## Screen Lifecycle

ONI screens follow this lifecycle, similar to Unity's MonoBehaviour but with ONI-specific names:

```
OnPrefabInit()  -->  OnSpawn()  -->  OnActivate()  <-->  OnDeactivate()
   (Awake)          (Start)        (becomes visible)   (becomes hidden)
```

- **OnPrefabInit** - Called when the GameObject is instantiated. Use for one-time setup.
- **OnSpawn** - Called after all prefabs are initialized. Use for initialization that depends on other objects.
- **OnActivate** - Called when the screen becomes active/visible. Can fire multiple times.
- **OnDeactivate** - Called when the screen is hidden or closed.
- **OnKeyDown(KButtonEvent)** - Called for keyboard input while the screen is active.

These are the methods you patch with Harmony to hook into screen behavior.

---

## Input System

### KButtonEvent

Passed to `OnKeyDown()` handlers on screens.

- `Consumed` (bool) - Set to `true` to prevent the game from processing the key further.

### KInputManager

- `KInputManager.isFocused` - Whether the game window has focus
- `KInputManager.currentController` - The current `KInputController`

### KInputController

- `GetKeyDown(KKeyCode)` - Check if a key was pressed this frame
- `mActiveModifiers` (private) - Current modifier keys held, requires reflection:
  ```csharp
  typeof(KInputController).GetField("mActiveModifiers", BindingFlags.Instance | BindingFlags.NonPublic);
  ```
  Returns a `Modifier` flags enum.

### KKeyCode (enum, partial list)

```
Escape, Tab, Return, KeypadEnter, Space,
UpArrow, DownArrow, LeftArrow, RightArrow,
Home, End, Mouse0
```

### Modifier (flags enum)

```
Ctrl, Shift, Alt
```

Check with bitwise AND:
```csharp
var mods = (Modifier)ActiveModifiersField.GetValue(KInputManager.currentController);
bool ctrlHeld = (mods & Modifier.Ctrl) != 0;
```

---

## UI Components

### KButton

ONI's button component.

| Member | Type | Notes |
|--------|------|-------|
| `SignalClick(KKeyCode.Mouse0)` | method | Programmatically trigger a click |
| `isInteractable` | property | Whether the button can be clicked |
| `gameObject` | property | The underlying GameObject |

### MultiToggle

Multi-state toggle (typically 2-state: on/off).

| Member | Type | Notes |
|--------|------|-------|
| `CurrentState` | int property | Current state (0 = off, 1 = on typically) |
| `onClick` | event/delegate | Invoked on click |
| `NextState()` | method | Cycle to next state |
| `gameObject` | property | The underlying GameObject |

### KToggle

Simple toggle component.

| Member | Type | Notes |
|--------|------|-------|
| `Click()` | method | Programmatically trigger |
| `IsInteractable()` | method | Whether it can be toggled |
| `gameObject` | property | The underlying GameObject |

### KSlider

Slider component (extends Unity's Slider).

| Member | Type | Notes |
|--------|------|-------|
| `value` | float | Current value (get/set) |
| `minValue` | float | Minimum value |
| `maxValue` | float | Maximum value |
| `wholeNumbers` | bool | Integer steps only |
| `interactable` | bool | Whether it can be adjusted |
| `gameObject` | property | The underlying GameObject |

Adjustment pattern:
```csharp
float step = slider.wholeNumbers ? 1f : (slider.maxValue - slider.minValue) / 20f;
slider.value = Mathf.Clamp(slider.value + direction * step, slider.minValue, slider.maxValue);
```

### Dropdown (UnityEngine.UI.Dropdown)

Standard Unity dropdown, used by ONI for resolution, color mode, audio device, etc.

| Member | Type | Notes |
|--------|------|-------|
| `value` | int | Selected index |
| `options` | List\<OptionData\> | Available options |
| `interactable` | bool | Whether it can be changed |
| `RefreshShownValue()` | method | **Must call** after setting value programmatically |

### KInputTextField

Text input field (used for world seed, etc.).

| Member | Type | Notes |
|--------|------|-------|
| `text` | string | Current text value |

### ToolTip

Tooltip component attached to UI elements.

| Member | Type | Notes |
|--------|------|-------|
| `RebuildDynamicTooltip()` | method | **Must call** before reading - updates dynamic content |
| `multiStringCount` | int | Number of tooltip lines |
| `GetMultiString(int)` | method | Get a specific tooltip line |

Tooltip search order (check all three locations):
```csharp
transform.GetComponent<ToolTip>()
transform.GetComponentInChildren<ToolTip>(true)
transform.GetComponentInParent<ToolTip>()
```

---

## Text and Localization

### LocText

ONI's localized text component (replaces Unity's Text/TMP).

| Member | Type | Notes |
|--------|------|-------|
| `text` | string | Current displayed text |
| `key` | string | Localization key |

### STRINGS namespace

Game strings live under the `STRINGS` namespace. Use these for localized output:
```csharp
using STRINGS;
string title = UI.FRONTEND.PAUSE_SCREEN.TITLE;
string tooltip = UI.TOOLTIPS.SPEEDBUTTON_SLOW;
```

### Strings.Get()

Dynamic localization lookup:
```csharp
string name = Strings.Get(layout.name);
```

### Localization class

| Member | Type | Notes |
|--------|------|-------|
| `PreinstalledLanguages` | static collection | Built-in language list |
| `GetPreinstalledLocalizationTitle(string)` | static method | Human-readable language name |
| `GetSelectedLanguageType()` | method | Returns `SelectedLanguageType` enum |
| `GetCurrentLanguageCode()` | method | e.g. "en", "zh_klei" |
| `DEFAULT_LANGUAGE_CODE` | const | Default language code |

`SelectedLanguageType` enum: `Preinstalled`, `UGC`, `None`

### Unity markup in text

ONI text contains Unity Rich Text tags (`<color=#FF0000>`, `<b>`, `<link>`, etc.). Strip with:
```csharp
private static readonly Regex MarkupRegex = new Regex("<.*?>", RegexOptions.Compiled);
string clean = MarkupRegex.Replace(text, string.Empty).Replace("\n", " ").Trim();
```

### Hotkey placeholders

Tooltips contain `{Hotkey}` placeholders. Strip them:
```csharp
int idx = text.IndexOf("{Hotkey}");
if (idx >= 0) text = text.Substring(0, idx).TrimEnd(' ', ':');
```

---

## HierarchyReferences

ONI's pattern for caching references to child components by name. More efficient than repeated `Find()` or `GetComponent()` calls.

```csharp
var refs = gameObject.GetComponent<HierarchyReferences>();
var title = refs.GetReference<LocText>("Title");
var toggle = refs.GetReference<MultiToggle>("EnabledToggle");
bool exists = refs.HasReference("ManageButton");
```

This is how ONI's UI prefabs wire up references to their children. When traversing screen hierarchies, check for `HierarchyReferences` first.

---

## Game Databases

### Db singleton

```csharp
var db = Db.Get();
```

#### Stories / Story Traits
```csharp
var stories = Db.Get().Stories.resources;
foreach (var story in stories)
{
    var trait = story.StoryTrait;
    string name = Strings.Get(trait.name);
    string desc = Strings.Get(trait.description);
}
```

### ProcGen (Procedural Generation)

#### Cluster layouts
```csharp
var layout = ProcGen.SettingsCache.clusterLayouts.GetClusterData(clusterPath);
string name = Strings.Get(layout.name);
string desc = Strings.Get(layout.description);
int difficulty = layout.difficulty; // 0-6
```

### Global singleton

```csharp
var global = Global.Instance;
var modManager = global.modManager;
var mods = modManager.mods; // List of installed mods
// Each mod has:
//   mod.label.id   - mod identifier
//   mod.title      - display name
```

### Game class

| Member | Type | Notes |
|--------|------|-------|
| `Game.OnSpawn()` | method | Game world fully loaded |
| `Game.DestroyInstances()` | method | Game is shutting down |

### SpeedControlScreen

| Member | Type | Notes |
|--------|------|-------|
| `IsPaused` | bool property | Whether the game is paused |
| `GetSpeed()` | method | Returns 0 (slow), 1 (medium), 2 (fast) |

### ToolMenu

| Member | Type | Notes |
|--------|------|-------|
| `ChooseTool(ToolInfo)` | method | Called when player selects a tool |

`ToolMenu.ToolInfo` struct:
- `toolName` (string)
- `text` (string) - display name
- `tooltip` (string)

---

## Mod Entry Point

ONI mods subclass `KMod.UserMod2`:

```csharp
public sealed class MyMod : KMod.UserMod2
{
    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        // Initialize your systems here
        harmony.PatchAll(typeof(MyMod).Assembly); // Auto-discovers [HarmonyPatch] classes
    }
}
```

### Finding your mod's directory at runtime

```csharp
string dllPath = typeof(MyMod).Assembly.Location;
string modDir = Path.GetDirectoryName(dllPath);
```

---

## Harmony Patching Patterns

### Basic Postfix (most common)

```csharp
[HarmonyPatch(typeof(MainMenu), "OnActivate")]
internal static class MainMenu_OnActivate_Patch
{
    private static void Postfix(MainMenu __instance)
    {
        // Runs after OnActivate
    }
}
```

### Input interception

```csharp
[HarmonyPatch(typeof(MainMenu), "OnKeyDown")]
internal static class MainMenu_OnKeyDown_Patch
{
    private static void Postfix(MainMenu __instance, KButtonEvent e)
    {
        if (e.Consumed) return; // Another handler already took it
        // Handle input...
        e.Consumed = true; // Prevent game from processing it
    }
}
```

### Private field access (compiled delegate, fast)

```csharp
private static readonly AccessTools.FieldRef<OptionsMenuScreen, KButton> CloseButton =
    AccessTools.FieldRefAccess<OptionsMenuScreen, KButton>("closeButton");

// Usage:
KButton btn = CloseButton(screenInstance);
```

**Use lazy initialization** to avoid startup lag when you have many:
```csharp
private static AccessTools.FieldRef<ModeSelectScreen, MultiToggle> _survivalButton;
private static AccessTools.FieldRef<ModeSelectScreen, MultiToggle> SurvivalButton =>
    _survivalButton ??= AccessTools.FieldRefAccess<ModeSelectScreen, MultiToggle>("survivalButton");
```

### Dynamic type resolution (DLC/version compat)

When a class might not exist in all game versions:

```csharp
[HarmonyPatch]
private static class GraphicsOptions_OnSpawn_Patch
{
    private static MethodBase TargetMethod()
    {
        var type = AccessTools.TypeByName("GraphicsOptionsScreen");
        return type == null ? null : AccessTools.Method(type, "OnSpawn");
    }

    private static void Postfix(object __instance) { /* ... */ }
}
```

Patch silently no-ops if the type doesn't exist.

---

## Known Screen Private Fields

Quick reference of private fields discovered via decompilation. Access with `AccessTools.FieldRefAccess`.

### ConfirmDialogScreen
`confirmButton` (GameObject), `cancelButton` (GameObject), `configurableButton` (GameObject), `titleText` (LocText), `popupMessage` (LocText)

### OptionsMenuScreen
`closeButton` (KButton), `backButton` (KButton)

### GameOptionsScreen
`resetTutorialButton` (KButton), `controlsButton` (KButton), `sandboxButton` (KButton), `doneButton` (KButton), `closeButton` (KButton), `defaultToCloudSaveToggle` (GameObject), `cameraSpeedSlider` (KSlider), `cameraSpeedSliderLabel` (LocText), `unitConfiguration` (UnitConfigurationScreen)

### AudioOptionsScreen
`closeButton` (KButton), `doneButton` (KButton), `sliderGroup` (GameObject), `alwaysPlayMusicButton` (GameObject), `alwaysPlayAutomationButton` (GameObject), `muteOnFocusLostToggle` (GameObject), `deviceDropdown` (Dropdown)

### GraphicsOptionsScreen
`resolutionDropdown` (Dropdown), `fullscreenToggle` (MultiToggle), `lowResToggle` (MultiToggle), `uiScaleSlider` (KSlider), `sliderLabel` (LocText), `colorModeDropdown` (Dropdown), `applyButton` (KButton), `doneButton` (KButton), `closeButton` (KButton)

### ModsScreen
`closeButton` (KButton), `toggleAllButton` (KButton), `workshopButton` (KButton), `entryParent` (Transform)

### LanguageOptionsScreen
`dismissButton` (KButton), `closeButton` (KButton), `workshopButton` (KButton), `uninstallButton` (KButton), `preinstalledLanguagesContainer` (GameObject), `ugcLanguagesContainer` (GameObject), `buttons` (List\<GameObject\>)

### LoadScreen
`closeButton` (KButton), `saveButtonRoot` (GameObject), `colonyListRoot` (GameObject), `colonyViewRoot` (GameObject), `loadMoreButton` (KButton), `colonyCloudButton` (KButton), `colonyLocalButton` (KButton), `colonyInfoButton` (KButton)

### InputBindingsScreen
`backButton` (KButton), `closeButton` (KButton), `resetButton` (KButton), `prevScreenButton` (KButton), `nextScreenButton` (KButton), `screenTitle` (LocText), `parent` (GameObject)

### MetricsOptionsScreen
`dismissButton` (KButton), `closeButton` (KButton), `enableButton` (GameObject), `openKleiAccountButton` (KButton)

### FeedbackScreen
`dismissButton` (KButton), `closeButton` (KButton), `bugForumsButton` (KButton), `suggestionForumsButton` (KButton), `logsDirectoryButton` (KButton), `saveFilesDirectoryButton` (KButton)

### CreditsScreen
`CloseButton` (KButton), `entryContainer` (Transform)

### ModeSelectScreen
`survivalButton` (MultiToggle), `nosweatButton` (MultiToggle), `closeButton` (KButton)

### ClusterCategorySelectionScreen
`closeButton` (KButton); public: `vanillaStyle`, `classicStyle`, `spacedOutStyle`, `eventStyle` (ButtonConfig)

### ColonyDestinationSelectScreen
`menuTabs` (MultiToggle[]), `selectedMenuTabIdx` (int), `backButton` (KButton), `customizeButton` (KButton), `launchButton` (KButton), `shuffleButton` (KButton), `storyTraitShuffleButton` (KButton), `destinationMapPanel` (DestinationSelectPanel), `storyContentPanel` (StoryContentPanel), `mixingPanel` (MixingContentPanel), `newGameSettingsPanel` (NewGameSettingsPanel), `gameSettingsPanel` (RectTransform), `customSettings` (GameObject)

### OfflineWorldGen
`updateText` (LocText), `percentText` (LocText)

---

## Known Gotchas

### UI not ready on spawn
Some screens need 1-2 frames before layout is complete. Use coroutines:
```csharp
yield return new WaitForEndOfFrame();
yield return new WaitForEndOfFrame();
// Now safe to read layout
```

### Input fires multiple times per frame
Debounce with frame tracking:
```csharp
if (Time.frameCount == lastInputFrame) return;
lastInputFrame = Time.frameCount;
```

### Announcement/event spam
Screen stack events fire rapidly. Use time-based deduplication:
```csharp
if (Time.unscaledTime - lastTime < 0.3f) return;
```

### Dropdown requires manual refresh
After programmatically setting `dropdown.value`, you **must** call:
```csharp
dropdown.RefreshShownValue();
```

### FieldRef startup lag
Creating many `AccessTools.FieldRefAccess` calls at once causes 4+ second lag. Use lazy `??=` initialization.

### Null and visibility checks
Always check both null and visibility before accessing UI elements, as DLC or version differences may hide components:
```csharp
if (button != null && button.gameObject.activeInHierarchy) { ... }
```
