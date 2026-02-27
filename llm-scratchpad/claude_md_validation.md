# CLAUDE.md Validation Report

Generated: 2026-02-27

---

## 1. Build Directions

### Claim: `build.ps1` exists and builds the DLL, deploys it, and patches mods.json
**PASS.** `build.ps1` exists at the repo root. It runs `dotnet build`, copies the output DLL to the game's local mods directory, and patches `mods.json` with `enabled = true`, `crash_count = 0`, and both DLC entries. All three stated behaviors are implemented.

### Claim: build.ps1 uses the `ONI_MANAGED` environment variable
**PASS.** The script checks `$env:ONI_MANAGED` at the top and sets a default if absent:
```powershell
if (-not $env:ONI_MANAGED) {
    $env:ONI_MANAGED = "C:\Program Files (x86)\Steam\steamapps\common\..."
}
```
The `.csproj` references all game assemblies via `$(ONI_MANAGED)`. This matches the claim exactly.

### Claim: "never `dotnet build` directly"
**NOTE (not a factual error).** The build script itself calls `dotnet build` internally — the rule means don't run `dotnet build` yourself, use the script. The claim is accurate as guidance, but it could confuse an LLM that sees `dotnet build` inside `build.ps1`. No correction needed, but phrasing is fine as-is.

---

## 2. Project Structure

### Claim: `OniAccess/` — mod source code (C#, .NET Framework 4.7.2, Harmony patches)
**PASS.** Directory exists. `OniAccess.csproj` confirms `<TargetFramework>net472</TargetFramework>`. The project uses Harmony and C#.

### Claim: `ONI-Decompiled/` — decompiled game source for reference (read-only, not part of build)
**PASS.** Directory exists at repo root with subdirectories `Assembly-CSharp/` and `Assembly-CSharp-firstpass/`, plus `CODEBASE_INDEX.md`. Not referenced in any `.csproj`, confirming it is not part of the build.

### Claim: `docs/` — design documentation
**PASS.** Directory exists and contains: `CODEBASE_INDEX.md`, `hotkey-reference.md`, `details-screen-handler.md`, `factorio-access concepts.md`, `inspection-panels.md`, `oni_accessibility_audit.md`, `schedule-screen-design.md`, `schedule-screen-research.md`.

### Claim: `docs/CODEBASE_INDEX.md` — complete namespace reference for decompiled ONI source
**PASS.** File exists and describes namespaces extracted from `Assembly-CSharp.dll` (4,194 files) and `Assembly-CSharp-firstpass.dll` (2,444 files).

### Claim: `.planning/` — project planning files
**PASS.** Directory exists and contains two files: `schedule-screen-design.md` and `schedule-screen-research.md`.

**MINOR ISSUE:** The `.planning/` directory currently contains only schedule-related planning files. If this was meant to hold broader project planning, the description matches reality at the moment, but is narrow in scope. No correction required — the description just says "project planning files" which is accurate.

---

## 3. Code Style Claims

### Claim: Harmony patch classes follow `GameType_MethodName_Patch` naming pattern (e.g., `KScreen_Activate_Patch`)
**PASS.** All patch classes found follow this convention. Examples from the codebase:
- `KScreen_Activate_Patch`
- `KScreen_Deactivate_Patch`
- `HoverTextDrawer_BeginDrawing_Patch`
- `DragTool_GetConfirmSound_Patch`
- `InputInit_Awake_Patch`
- `PlayerControlledToggleSideScreen_RenderEveryTick_Patch`

No deviations found.

### Claim: All speech goes through `SpeechPipeline`, never call `SpeechEngine.Say()` directly
**PASS.** `SpeechPipeline.cs` exists and is documented as the central dispatch point. The only reference to `SpeechEngine.Say` outside of `SpeechEngine.cs` itself is the `SpeakAction` delegate assignment inside `SpeechPipeline`:
```csharp
internal static System.Action<string, bool> SpeakAction = SpeechEngine.Say;
```
This is the intended indirection point, not a violation. No caller directly invokes `SpeechEngine.Say()`.

### Claim: All logging goes through `Log.Info/Debug/Warn/Error`, never use `Debug.Log` directly
**PASS.** The `Log` class in `OniAccess/Util/LogHelper.cs` exposes `Debug`, `Info`, `Warn`, and `Error` methods. `UnityEngine.Debug.Log` is only referenced inside `LogUnityBackend.cs` (the installer) and a comment in `Mod.cs` — not called directly by any mod code. All production logging goes through `Log.*`.

---

## 4. Test Directions

### Claim: `test.ps1` exists
**PASS.** `test.ps1` exists at the repo root.

### Claim: Builds and runs the offline test suite (`OniAccess.Tests`)
**PASS.** `test.ps1` runs `dotnet build` on `OniAccess.Tests\OniAccess.Tests.csproj` and then executes the built `.exe`. The `OniAccess.Tests` directory exists with the `.csproj` and a `Program.cs` entry point.

### Claim: Tests run without the game
**PASS.** `OniAccess.Tests.csproj` references the game DLLs for type resolution but does not load the game runtime. `LogHelper.cs` defaults to `Console.WriteLine` (not `UnityEngine.Debug.Log`) so tests work without Unity. `SpeechPipeline` has an injectable `SpeakAction` delegate for test isolation.

---

## 5. Architecture Gotchas

### Claim: New screen handlers must be registered in `ContextDetector.RegisterMenuHandlers()` or they will never activate
**PASS.** `ContextDetector.RegisterMenuHandlers()` exists in `OniAccess/Handlers/ContextDetector.cs` (line 130). It is called from `Mod.OnLoad()` (line 49 of `Mod.cs`). All handlers are registered there. The claim is accurate.

### Claim: Key detection goes in `Tick()` via `UnityEngine.Input.GetKeyDown()`. `HandleKeyDown()` is only for Escape interception through KButtonEvent
**PASS.** `KeyPoller.cs` drives `Tick()` from `Update()`. All key detection in handlers uses `UnityEngine.Input.GetKeyDown()` inside `Tick()` overrides. `HandleKeyDown()` is exclusively used for `e.TryConsume(Action.Escape)` KButtonEvent interception. The `IAccessHandler` interface comment confirms this design.

### Claim: `UnityEngine.Input` must be fully qualified inside the `OniAccess.Input` namespace. Bare `Input` resolves to the namespace, not the Unity class
**PASS.** All files within the `OniAccess.Input` namespace (`KeyPoller.cs`, `ModInputRouter.cs`, `InputUtil.cs`) use the fully qualified `UnityEngine.Input.GetKeyDown()`. `KeyPoller.cs` has an explicit comment confirming this:
```csharp
// All UnityEngine.Input references are fully qualified per Phase 1 decision:
// bare Input resolves to the OniAccess.Input namespace, not UnityEngine.Input.
```
Files outside the `OniAccess.Input` namespace (handlers etc.) also use the fully qualified form consistently.

### Claim: `ShowOptimizedKScreen` subclasses (e.g. `TableScreen`): patch `OnShow` — `KScreen.OnActivate` does not call `OnShow`, so it only fires on ManagementMenu toggles
**PASS.** Confirmed in `ONI-Decompiled/Assembly-CSharp/TableScreen.cs`:
```csharp
public class TableScreen : ShowOptimizedKScreen
```
The codebase patches `TableScreen.OnShow` for this reason. The gotcha description is accurate.

### Claim: `KModalScreen` subclasses (e.g. `ResearchScreen`): patch `Show`, NOT `OnShow` — `KModalScreen.OnActivate` calls `OnShow(true)` directly during prefab init
**PASS.** Confirmed in `ONI-Decompiled/Assembly-CSharp/KModalScreen.cs`:
```csharp
protected override void OnActivate()
    OnShow(show: true);
```
`ResearchScreen` extends `KModalScreen` (confirmed in decompiled source). The actual patch in `ScreenLifecyclePatches.cs` patches `ResearchScreen.Show`, not `OnShow`. The claim is accurate.

**NOTE:** The gotcha says "e.g. `ResearchScreen`" but the `SkillsScreen` handler actually patches `OnShow` (not `Show`), contrary to this guidance. The comment in `ScreenLifecyclePatches.cs` explains: "Unlike ResearchScreen, SkillsScreen does not override Show — only OnShow." So the pattern is more nuanced than the CLAUDE.md gotcha implies — the choice between `Show` and `OnShow` depends on whether the specific subclass overrides `Show`. The gotcha gives a useful rule of thumb but is an oversimplification. This is not wrong, but an LLM following it literally for a new `KModalScreen` subclass might patch the wrong method.

---

## 6. File Paths

### Claim: Player.log is at `C:\Users\rasha\AppData\LocalLow\Klei\Oxygen Not Included\Player.log`
**PASS.** The file exists at that exact path.

**ISSUE — Machine-specific hardcoded path.** This path contains the username `rasha` and is not portable. If another developer clones this repo, the path in CLAUDE.md would be wrong for them. The correct generic form would be: `%LOCALAPPDATA%/../LocalLow/Klei/Oxygen Not Included/Player.log` or simply `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\Player.log`. Since this is a single-developer project and the MEMORY.md also hardcodes the same username, this is a deliberate choice, but worth flagging.

### Claim: `docs/CODEBASE_INDEX.md` exists
**PASS.** File exists at `C:\Users\rasha\Documents\Oni-access\docs\CODEBASE_INDEX.md`.

### Claim: `docs/hotkey-reference.md` exists
**PASS.** File exists at `C:\Users\rasha\Documents\Oni-access\docs\hotkey-reference.md`.

---

## 7. Stale File Directory Listings

**PASS — no stale listings found.** CLAUDE.md contains no file-by-file directory listings. The project structure section lists directories only (not files within them), so it cannot become stale from normal development. No issues here.

---

## 8. Machine-Specific Paths

Two hardcoded machine-specific paths were found:

### In CLAUDE.md (Game Log section):
```
C:\Users\rasha\AppData\LocalLow\Klei\Oxygen Not Included\Player.log
```
Contains username `rasha`. Not portable across users.

### In build.ps1 (ONI_MANAGED default):
```powershell
$env:ONI_MANAGED = "C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed"
```
This is a fallback default — it will be wrong for non-Steam installs or installs on non-default drives. However, since `ONI_MANAGED` can be set externally to override it, this is acceptable as a developer convenience default. No correction needed, but worth noting.

### In test.ps1 (ONI_MANAGED default):
Same pattern as `build.ps1`. Same assessment — acceptable as a fallback.

---

## Summary Table

| Claim | Status | Notes |
|---|---|---|
| build.ps1 exists | PASS | |
| build.ps1 builds DLL, deploys, patches mods.json | PASS | All three behaviors verified |
| Uses ONI_MANAGED env var | PASS | Set as default if absent |
| OniAccess/ directory exists | PASS | |
| .NET Framework 4.7.2 | PASS | net472 confirmed in .csproj |
| ONI-Decompiled/ exists | PASS | |
| docs/ exists | PASS | |
| docs/CODEBASE_INDEX.md exists | PASS | |
| .planning/ exists | PASS | Contains 2 schedule-related files |
| Patch naming convention | PASS | All patches follow GameType_MethodName_Patch |
| SpeechPipeline is speech entry point | PASS | No direct SpeechEngine.Say() calls outside SpeechPipeline |
| Log class with Info/Debug/Warn/Error | PASS | All four methods verified |
| No Debug.Log calls in mod code | PASS | Only in LogUnityBackend (intentional) |
| test.ps1 exists | PASS | |
| OniAccess.Tests project exists | PASS | |
| Tests run offline | PASS | Console backend default, injectable speech action |
| ContextDetector.RegisterMenuHandlers() exists | PASS | Called from Mod.OnLoad() |
| Key detection in Tick() via UnityEngine.Input.GetKeyDown() | PASS | Consistent throughout handlers |
| HandleKeyDown() for Escape/KButtonEvent only | PASS | Confirmed pattern |
| UnityEngine.Input must be fully qualified in OniAccess.Input namespace | PASS | All usages are fully qualified |
| ShowOptimizedKScreen -> patch OnShow | PASS | Confirmed from decompiled TableScreen and actual patches |
| KModalScreen -> patch Show not OnShow | PARTIAL | Accurate for ResearchScreen; SkillsScreen (also KModalScreen-derived) patches OnShow instead because it doesn't override Show. The gotcha is an oversimplification. |
| Player.log path | PASS (exists) | Hardcoded with username `rasha` — not portable |
| docs/hotkey-reference.md exists | PASS | |
| No stale file listings | PASS | Only directories listed, no file-by-file inventory |

---

## Issues Requiring Attention

### Issue 1 (Minor): KModalScreen gotcha is an oversimplification
**Location:** Architecture Gotchas section, second bullet under "Show-lifecycle patches"

The CLAUDE.md says: `KModalScreen` subclasses patch `Show`, NOT `OnShow`. This is true for `ResearchScreen` but not for `SkillsScreen`, which also descends from `KModalScreen` yet is patched at `OnShow`. The actual rule is: patch whichever the specific class overrides (`Show` or `OnShow`), and check whether `OnActivate` would trigger it during prefab init. The current text could lead an LLM to always patch `Show` on KModalScreen subclasses, which would be wrong for classes like `SkillsScreen` that only override `OnShow`.

**Suggested correction:** Add a qualifier: the choice depends on whether the subclass overrides `Show` or only `OnShow` — check the decompiled source for each new screen.

### Issue 2 (Low): Machine-specific path in Game Log section
**Location:** `## Game Log` section

`C:\Users\rasha\AppData\LocalLow\...` hardcodes the username. This is fine for a single-developer project but would mislead any other contributor. Could use `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\Player.log` or a note that the path uses the current user's profile directory.

### Issue 3 (Observation, not an error): .planning/ is narrow
The `.planning/` directory currently only contains two schedule-screen planning files. The description "project planning files" is accurate today but implies a broader purpose than the directory currently fulfills. No action required.
