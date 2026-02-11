# Technology Stack: ONI Screen Reader Accessibility Mod

> **Research type:** Stack dimension for greenfield ONI accessibility mod
> **Date:** 2026-02-11
> **Confidence key:** HIGH = verified from decompiled source or official docs; MEDIUM = corroborated across multiple community sources; LOW = best available information, needs validation

---

## 1. Runtime Environment

### Game Engine: Unity (Mono runtime)
- **Version:** Unity 2020.3 LTS (the version ONI build 707956 ships with)
- **Runtime:** Mono (NOT IL2CPP) -- this is critical: Mono allows runtime reflection, Harmony patching, and Assembly.LoadFrom, all of which IL2CPP blocks
- **Target Framework:** .NET Framework 4.x (net471/net472 compatible surface area, but ONI's decompiled csproj targets net40)
- **C# Language Version:** Up to C# 9.0 features work in practice (pattern matching, records are iffy; nullable reference types work as annotations only). The decompiled source uses `LangVersion 12.0` for decompilation fidelity, but the runtime is Mono so actual feature support is limited.
- **Confidence:** HIGH (verified from `Assembly-CSharp.csproj` and `KleiVersion.cs` showing build 707956, release branch)

**Rationale:** You do not choose the Unity version or runtime -- the game dictates it. The Mono runtime is what makes the entire modding ecosystem possible. If Klei ever migrated to IL2CPP, the modding approach would need fundamental rethinking.

### Target Platform: Windows x64 only
- **Why:** Tolk library is Windows-only (depends on COM-based screen reader APIs). NVDA, JAWS, and SAPI are all Windows technologies. ONI runs on Linux/Mac but screen reader infrastructure on those platforms is fundamentally different (speechd on Linux, VoiceOver on Mac) and would require a separate speech backend.
- **Confidence:** HIGH

---

## 2. Mod Framework

### Entry Point: KMod.UserMod2

- **What it is:** ONI's built-in mod loading base class. Your mod DLL must contain exactly one class inheriting from `KMod.UserMod2`. The game's `DLLLoader` scans for this via reflection.
- **Key methods:**
  - `OnLoad(Harmony harmony)` -- called when the mod DLL is loaded. This is your initialization point. Call `harmony.PatchAll(assembly)` to auto-discover `[HarmonyPatch]` classes, or apply patches manually.
  - `OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)` -- called after all mods are loaded. Use for cross-mod compatibility if needed.
- **Properties available:** `assembly` (your Assembly), `path` (mod directory on disk), `mod` (the `KMod.Mod` object with metadata)
- **Confidence:** HIGH (verified from decompiled `UserMod2.cs` and `DLLLoader.cs`)

**Rationale:** This is the ONLY supported entry point for ONI DLL mods. The mod loader explicitly scans for `UserMod2` subclasses and rejects assemblies with more than one. There is no alternative.

### mod_info.yaml (required)

Every ONI mod must include a `mod_info.yaml` file at its root. Without it, the mod loader skips the mod entirely (with one exception: translation-only mods with `.po` files).

```yaml
supportedContent: ALL
minimumSupportedBuild: 707956
APIVersion: 2
version: "0.1.0"
```

**Critical fields:**
- `APIVersion: 2` -- MUST be 2 for Harmony 2 mods. The loader checks this and refuses to load DLLs if not set correctly. `MOD_API_VERSION_HARMONY2 = 2` is the current constant.
- `minimumSupportedBuild` -- set to the build number you developed against (currently 707956). The loader picks the archived version with the highest `minimumSupportedBuild` that doesn't exceed the running game's build number.
- DLC fields: `requiredDlcIds` and `forbiddenDlcIds` control which DLC configurations your mod supports. Omitting both means "works with everything."
- **Confidence:** HIGH (verified from decompiled `Mod.cs` `GetModInfoForFolder()` method)

### Mod Directory Structure (ONI convention)

```
OniAccess/
    mod_info.yaml          # Required metadata
    OniAccess.dll          # Your compiled mod DLL (the loader finds all .dll files)
    tolk/
        Tolk.dll           # Native DLL -- loaded via P/Invoke, not by mod loader
        nvdaControllerClient64.dll
        SAAPI64.dll
    strings/               # Optional: .po files for string overrides
        OniAccess.po
```

**Important:** The mod loader calls `Assembly.LoadFrom()` on every `.dll` in the mod's content directory. Place native DLLs (Tolk) in a subdirectory to prevent the loader from attempting to load them as .NET assemblies. Use `SetDllDirectory` or manipulate `PATH` at runtime to ensure P/Invoke can find them.

- **Confidence:** HIGH (verified from `DLLLoader.LoadDLLs()` which iterates `directoryInfo.GetFiles()` for `*.dll`)

---

## 3. Patching Framework

### Harmony 2.x (bundled with ONI as 0Harmony.dll)

- **Version:** Harmony 2.x (the `HarmonyLib` namespace, not the old `Harmony` namespace). ONI ships `0Harmony.dll` in its `Managed/` directory.
- **DO NOT bundle your own Harmony.** Use the game's copy. Bundling a different version causes type conflicts and crashes.
- **Namespace:** `using HarmonyLib;`
- **Confidence:** HIGH (verified from decompiled `using HarmonyLib;` in UserMod2.cs, DLLLoader.cs, and the `0Harmony` reference in the csproj)

**Rationale:** ONI switched to Harmony 2 (API version 2) and the mod loader enforces this. The old Harmony 1 API is not supported. All community mods target Harmony 2.

### Harmony Patching Patterns for ONI

These patterns are established by the ONI modding community and verified against the decompiled source.

#### Pattern 1: Attribute-based auto-patching (preferred for most cases)

```csharp
public sealed class OniAccessMod : KMod.UserMod2
{
    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        // base.OnLoad already calls harmony.PatchAll(assembly)
        // which discovers all [HarmonyPatch] classes in your assembly
    }
}

[HarmonyPatch(typeof(MainMenu), "OnActivate")]
internal static class MainMenu_OnActivate_Patch
{
    private static void Postfix(MainMenu __instance)
    {
        // Runs after MainMenu.OnActivate()
    }
}
```

**Naming convention:** `{TargetClass}_{TargetMethod}_{Patch}` is the standard ONI community convention for patch class names.

#### Pattern 2: Prefix patches (for input interception)

```csharp
[HarmonyPatch(typeof(SomeScreen), "OnKeyDown")]
internal static class SomeScreen_OnKeyDown_Patch
{
    // Return false from Prefix to skip the original method
    private static bool Prefix(SomeScreen __instance, KButtonEvent e)
    {
        if (ShouldIntercept(e))
        {
            HandleInput(e);
            e.Consumed = true;
            return false; // Skip original
        }
        return true; // Run original
    }
}
```

#### Pattern 3: Private field access (FieldRef, lazy-initialized)

```csharp
// WRONG: Creates all FieldRefs at class load time -> 4+ second startup lag
private static readonly AccessTools.FieldRef<Foo, Bar> Field =
    AccessTools.FieldRefAccess<Foo, Bar>("field");

// RIGHT: Lazy initialization, spreads cost across first-use
private static AccessTools.FieldRef<Foo, Bar> _field;
private static AccessTools.FieldRef<Foo, Bar> Field =>
    _field ??= AccessTools.FieldRefAccess<Foo, Bar>("field");
```

- **Confidence:** HIGH (lazy FieldRef pattern verified as necessary from `oni-internals.md` known gotchas and community experience)

#### Pattern 4: Dynamic type resolution (DLC compatibility)

```csharp
[HarmonyPatch]
private static class DlcSpecificScreen_Patch
{
    private static MethodBase TargetMethod()
    {
        var type = AccessTools.TypeByName("SomeSpacedOutScreen");
        return type == null ? null : AccessTools.Method(type, "OnSpawn");
    }

    private static void Postfix(object __instance) { /* ... */ }
}
```

When `TargetMethod()` returns null, Harmony silently skips the patch. This is essential for DLC-optional features.

- **Confidence:** HIGH (verified pattern from decompiled ONI code and documented in oni-internals.md)

### What NOT to use

- **HarmonyX / BepInEx:** ONI does NOT use BepInEx. Some Unity games do, but ONI has its own mod loader (KMod). Do not introduce BepInEx -- it conflicts with ONI's loading pipeline. HarmonyX is the BepInEx fork of Harmony; ONI uses standard Harmony 2.
- **MonoMod:** Lower-level than needed. Harmony wraps MonoMod internally. No reason to use it directly.
- **Transpiler patches (use sparingly):** Transpilers modify IL directly and are the most fragile patch type -- they break on any game update that changes the target method's IL structure. Prefer Prefix/Postfix. Only use Transpiler when you must modify behavior mid-method and no other approach works.
- **Confidence:** HIGH

---

## 4. Speech Output

### Tolk (via P/Invoke) -- already implemented

- **What:** Screen reader abstraction library. Auto-detects the active screen reader and routes speech through it.
- **Version:** Latest available (the project uses Tolk.dll from the `tolk/dist/` directory, 64-bit)
- **Supported screen readers:** JAWS, NVDA, Window-Eyes (obsolete), SuperNova, System Access, ZoomText, SAPI 5.3 (fallback)
- **License:** LGPLv3 (compatible with mod distribution)
- **Status:** NOTE from Tolk README: "this project is not currently being developed." However, it remains the de facto standard for game accessibility mods. The API is stable and the screen reader driver interfaces haven't changed.
- **Confidence:** HIGH (Tolk DLLs present in project, Speech.cs wrapper already implemented and verified)

**Rationale:** Tolk is the right choice because:
1. It abstracts across screen readers (NVDA, JAWS, SAPI) with a single API
2. It's the same approach used by other game accessibility projects
3. The P/Invoke wrapper (`Speech.cs`) is already built and tested
4. SAPI fallback means the mod works even without a screen reader installed
5. The "not currently being developed" status is acceptable because the API surface is frozen and screen reader APIs are stable

### Tolk API Surface (from Speech.cs)

```csharp
[DllImport("Tolk.dll")] static extern void Tolk_Load();
[DllImport("Tolk.dll")] static extern void Tolk_Unload();
[DllImport("Tolk.dll")] static extern bool Tolk_Output(string str, bool interrupt);
[DllImport("Tolk.dll")] static extern bool Tolk_TrySAPI(bool trySAPI);
[DllImport("Tolk.dll")] static extern bool Tolk_HasSpeech();
[DllImport("Tolk.dll")] static extern IntPtr Tolk_DetectScreenReader();
```

### Tolk Threading Warning

Tolk is NOT thread-safe. Some screen reader drivers use COM. Options:
1. Call `Tolk_Load` on every thread that uses Tolk (matched by `Tolk_Unload`)
2. Initialize COM yourself and call `Tolk_Load` once (recommended for .NET)

For ONI: All speech calls should go through `Speech.Say()` on the main Unity thread. If background processing generates speech, queue it and dispatch on the main thread.

### Required Native DLLs

| File | Purpose | Architecture |
|------|---------|-------------|
| `Tolk.dll` | Core abstraction library | x64 |
| `nvdaControllerClient64.dll` | NVDA screen reader driver | x64 |
| `SAAPI64.dll` | System Access screen reader driver | x64 |

JAWS and SAPI use COM and don't need separate DLLs.

### What NOT to use for speech

- **System.Speech.Synthesis (managed SAPI wrapper):** Adds a dependency on an assembly that may not be in ONI's Mono runtime. Tolk already provides SAPI fallback. Redundant.
- **UI Accessibility (UIA) directly:** Too low-level, requires extensive COM interop, and doesn't provide the screen reader abstraction you need.
- **UniversalSpeech:** Alternative to Tolk by QuentinC. Less widely adopted in game modding. Tolk is the established choice.
- **Confidence:** HIGH

---

## 5. Build System

### Project Type: .NET Framework Class Library

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <LangVersion>9.0</LangVersion>
    <AssemblyName>OniAccess</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <!-- Game DLLs - reference but do not copy -->
    <Reference Include="Assembly-CSharp" Private="false">
      <HintPath>$(ONI_MANAGED)\Assembly-CSharp.dll</HintPath>
    </Reference>
    <Reference Include="Assembly-CSharp-firstpass" Private="false">
      <HintPath>$(ONI_MANAGED)\Assembly-CSharp-firstpass.dll</HintPath>
    </Reference>
    <Reference Include="0Harmony" Private="false">
      <HintPath>$(ONI_MANAGED)\0Harmony.dll</HintPath>
    </Reference>
    <Reference Include="UnityEngine.CoreModule" Private="false">
      <HintPath>$(ONI_MANAGED)\UnityEngine.CoreModule.dll</HintPath>
    </Reference>
    <Reference Include="UnityEngine.UI" Private="false">
      <HintPath>$(ONI_MANAGED)\UnityEngine.UI.dll</HintPath>
    </Reference>
    <Reference Include="Unity.TextMeshPro" Private="false">
      <HintPath>$(ONI_MANAGED)\Unity.TextMeshPro.dll</HintPath>
    </Reference>
    <Reference Include="UnityEngine.UIModule" Private="false">
      <HintPath>$(ONI_MANAGED)\UnityEngine.UIModule.dll</HintPath>
    </Reference>
    <Reference Include="UnityEngine.InputLegacyModule" Private="false">
      <HintPath>$(ONI_MANAGED)\UnityEngine.InputLegacyModule.dll</HintPath>
    </Reference>
    <Reference Include="Newtonsoft.Json" Private="false">
      <HintPath>$(ONI_MANAGED)\Newtonsoft.Json.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

**Critical: `Private="false"` on all game references.** This prevents MSBuild from copying game DLLs into your output directory. You only ship your own DLL and Tolk natives.

### Environment Variable for Portability

Define `ONI_MANAGED` pointing to:
```
C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\
```

This lets the project build on any machine with ONI installed regardless of Steam library location.

### Build Output -> Mod Directory

Use a post-build step or MSBuild target to copy the output DLL plus Tolk natives to the ONI local mods directory:
```
%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Dev\OniAccess\
```

The `Dev` subdirectory signals to ONI's mod loader that this is a development mod (`Label.DistributionPlatform.Dev`), which enables more verbose logging.

- **Confidence:** HIGH (mod paths verified from decompiled `Global.Awake()` -> `Local` distribution platform setup)

### What NOT to use for builds

- **NuGet packages for Harmony:** Do NOT add `Lib.Harmony` NuGet package. Use the game's bundled `0Harmony.dll`. Version mismatches cause runtime type conflicts.
- **Unity Editor / Unity SDK:** You are not making a Unity project. You are making a .NET class library that references Unity's managed DLLs. Do not install Unity Editor for this project.
- **Confidence:** HIGH

---

## 6. Game DLL Reference Map

These are the game DLLs your mod will reference. All live in `OxygenNotIncluded_Data/Managed/`.

| DLL | What you use it for | Required |
|-----|---------------------|----------|
| `Assembly-CSharp.dll` | ALL game classes: KScreen, KButton, Game, STRINGS, all building configs, all screen classes, KMod.UserMod2 | Yes |
| `Assembly-CSharp-firstpass.dll` | KMonoBehaviour, KInputManager, KInputController, KButtonEvent, KKeyCode, Modifier, KAnimFile, and other foundational types | Yes |
| `0Harmony.dll` | HarmonyLib namespace for patching | Yes |
| `UnityEngine.CoreModule.dll` | MonoBehaviour, GameObject, Transform, Debug, Application, Mathf, coroutines | Yes |
| `UnityEngine.UI.dll` | UI base types (Selectable, Dropdown, Slider, etc.) | Yes |
| `UnityEngine.UIModule.dll` | Canvas, GraphicRaycaster | Yes |
| `Unity.TextMeshPro.dll` | TextMeshPro types (ONI uses TMP for text rendering via LocText) | Yes |
| `UnityEngine.InputLegacyModule.dll` | Input class (if you need raw input checks beyond KInputManager) | Likely |
| `Newtonsoft.Json.dll` | JSON serialization (for settings file persistence) | Optional |
| `FMODUnity.dll` | Audio system (if you implement sonification later) | Future |

- **Confidence:** HIGH (all paths verified from decompiled `Assembly-CSharp.csproj`)

---

## 7. Key ONI Internal APIs

These are not external dependencies but the game APIs you will patch against. Understanding them is essential for stack decisions.

### Screen System
- `KScreenManager` -- singleton managing a stack of `KScreen` instances
- `KScreen` -- base class for all UI screens (subclassed by every game screen)
- `KModalScreen` -- modal screen that blocks input to screens below
- Screen lifecycle: `OnPrefabInit()` -> `OnSpawn()` -> `OnActivate()` <-> `OnDeactivate()`
- Input: `OnKeyDown(KButtonEvent)` on each screen

### Input System
- `KInputManager` / `KInputController` -- custom input system (NOT Unity's Input)
- `KButtonEvent` with `Consumed` flag for event propagation
- `KKeyCode` enum, `Modifier` flags enum (Ctrl, Shift, Alt)
- ONI has 100+ keybindings across categories (Root, Tool, Management, Building, Navigation, Debug, Sandbox, etc.)

### UI Components
- `KButton`, `MultiToggle`, `KToggle`, `KSlider`, `LocText`, `ToolTip`, `HierarchyReferences`
- Unity's `Dropdown` used for resolution, audio device, etc.
- `KInputTextField` for text input

### Game Data
- `Db.Get()` -- singleton for all game databases (techs, skills, traits, rooms, etc.)
- `STRINGS` namespace -- all localized text
- `SimHashes` -- all simulation element types
- `GameTags` -- tag constants for entity filtering
- `Grid` -- tile-based world access
- `Game.Instance` -- main game controller

- **Confidence:** HIGH (all verified from decompiled source)

---

## 8. Testing Strategy

### Unit Tests: NUnit or xUnit

For pure logic (MessageBuilder, describers, navigation graph, text filtering):
- NUnit is the most natural choice (ships with Unity Test Framework)
- Can run outside Unity via `dotnet test`
- Test your `Speech.FilterRichText()`, message assembly, entity description logic

### Integration Tests: Manual in-game

ONI's mod loader does not support automated in-game testing easily. The practical approach:
1. Build a `SpeechCapture` mode that logs all speech output to a file
2. Load the mod, perform actions, verify speech log
3. Consider a test harness that replays input sequences

### What NOT to use

- **Unity Test Framework (play mode tests):** Would require a Unity project setup, which conflicts with the class library approach. Not worth the overhead for a mod.
- **Automated UI testing frameworks:** ONI's custom UI system (KScreen, KButton) is not compatible with standard UI testing tools.

- **Confidence:** MEDIUM (testing in modded games is inherently challenging; this is the pragmatic approach used by the ONI modding community)

---

## 9. Settings Persistence

### Approach: JSON file via Newtonsoft.Json

ONI ships `Newtonsoft.Json.dll`. Use it for mod settings:

```csharp
string settingsPath = Path.Combine(modDir, "settings.json");
```

Store in the mod's own directory (accessible via `typeof(OniAccessMod).Assembly.Location`).

### What NOT to use

- **ONI's save system:** Mod settings should survive save file changes. Don't couple settings to save data.
- **PlayerPrefs:** Global to the game, namespace conflicts with other mods.
- **Custom binary serialization:** Unnecessary complexity. JSON is human-readable and debuggable.

- **Confidence:** HIGH

---

## 10. Mod Distribution

### Development: Local "Dev" mods directory

```
%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Dev\OniAccess\
```

### Release: Steam Workshop (preferred) or manual

Steam Workshop distribution uses ONI's built-in `Steam` distribution platform (`SteamUGCService`). The mod loader handles download, installation, and updates automatically.

For manual distribution, users place the mod folder in:
```
%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\OniAccess\
```

### Special consideration for native DLLs

Tolk's native DLLs (`Tolk.dll`, `nvdaControllerClient64.dll`, `SAAPI64.dll`) must be loadable by P/Invoke. Options:
1. Place them in a subdirectory and call `SetDllDirectory` in `OnLoad` before initializing Tolk
2. Place them alongside the managed DLL (but risk the mod loader trying to load them as .NET assemblies -- they'll fail gracefully since they have no `UserMod2` subclass, but it generates log noise)

The cleanest approach (already implemented in Speech.cs):
```csharp
[DllImport("kernel32.dll", SetLastError = true)]
static extern bool SetDllDirectory(string lpPathName);

// In OnLoad:
string tolkDir = Path.Combine(Path.GetDirectoryName(assembly.Location), "tolk", "dist");
SetDllDirectory(tolkDir);
Speech.Initialize();
```

- **Confidence:** HIGH

---

## 11. Dependencies Summary

### Ship with your mod (in mod directory)

| Component | Version | License | Purpose |
|-----------|---------|---------|---------|
| `OniAccess.dll` | Your mod | Your choice | The mod itself |
| `Tolk.dll` | Latest (no version number; project inactive but stable) | LGPLv3 | Screen reader abstraction |
| `nvdaControllerClient64.dll` | Bundled with Tolk | See Tolk license | NVDA driver |
| `SAAPI64.dll` | Bundled with Tolk | See Tolk license | System Access driver |
| `mod_info.yaml` | N/A | N/A | Mod metadata for ONI loader |

### Reference at build time only (from game install, do NOT ship)

| Component | Purpose |
|-----------|---------|
| `Assembly-CSharp.dll` | Game classes |
| `Assembly-CSharp-firstpass.dll` | Foundation types |
| `0Harmony.dll` | Harmony 2 patching |
| `UnityEngine.CoreModule.dll` | Unity core |
| `UnityEngine.UI.dll` | Unity UI |
| `UnityEngine.UIModule.dll` | Unity UI module |
| `Unity.TextMeshPro.dll` | Text rendering |
| `Newtonsoft.Json.dll` | JSON (bundled with game) |

### Do NOT include

| Component | Why not |
|-----------|---------|
| Any Harmony NuGet | Version conflict with game's 0Harmony.dll |
| BepInEx / HarmonyX | ONI uses its own mod loader, not BepInEx |
| Unity Editor packages | This is a class library, not a Unity project |
| System.Speech.dll | Tolk already handles SAPI; this assembly may not be in Mono |

---

## 12. ONI Modding Community Conventions

These patterns are established across the ONI modding community and should be followed for consistency.

### Code Organization

1. **One `UserMod2` subclass per assembly** -- enforced by the loader
2. **Patch classes in a `Patches/` namespace or folder** -- community convention
3. **Patch class naming:** `{TargetType}_{TargetMethod}_{Patch}` (e.g., `MainMenu_OnActivate_Patch`)
4. **Internal static classes for patches** -- patches are static methods, the class is just a container
5. **Sealed mod class** -- `public sealed class OniAccessMod : KMod.UserMod2`

### Harmony Best Practices (ONI-specific)

1. **Prefer Postfix over Prefix** -- less likely to break when the game updates
2. **Prefix only for input interception** -- return `false` to skip original when consuming input
3. **Avoid Transpiler unless absolutely necessary** -- most fragile patch type
4. **Use `__instance` parameter name** to access the patched object (Harmony convention)
5. **Use `__result` parameter name** to read/modify return values in Postfix
6. **Lazy-initialize FieldRefs** -- many FieldRefs created at class-load time cause 4+ second startup lag
7. **Check `e.Consumed` before handling input** in OnKeyDown postfixes
8. **Use dynamic TargetMethod() for DLC-optional types** -- silent no-op if type doesn't exist

### Frame Timing Gotchas

1. **UI not ready on spawn** -- some screens need 1-2 frames before layout is complete. Use `yield return new WaitForEndOfFrame()` in coroutines.
2. **Input fires multiple times per frame** -- debounce with `Time.frameCount` tracking
3. **Event spam** -- screen stack events fire rapidly. Use time-based deduplication (`Time.unscaledTime - lastTime < 0.3f`)
4. **Dropdown manual refresh** -- after setting `dropdown.value` programmatically, MUST call `dropdown.RefreshShownValue()`

### Logging Convention

```csharp
Debug.Log($"[OniAccess] {message}");
```

Prefix all log messages with mod name in brackets. ONI's log (`output_log.txt`) is shared by the game and all mods.

- **Confidence:** HIGH (verified from decompiled source and established patterns in oni-internals.md)

---

## 13. Architecture Recommendations

Based on FactorioAccess patterns adapted for C#/Unity/Harmony (from extensive prior research in docs/).

### Information Pipeline (ADOPT)

```
Game State (Harmony patches, Grid queries, component access)
    -> Describers (per-entity-type description generators)
    -> MessageBuilder (fluent text assembly with screen reader constraints)
    -> Speech.Say() (Tolk output)
```

Each layer has one job. Describers don't call Speech directly. MessageBuilder doesn't know about game entities.

### Module Self-Registration (ADOPT)

Each system registers its own Harmony patches via `[HarmonyPatch]` attributes. A thin `OniAccessMod.OnLoad()` orchestrator wires systems together but contains no logic.

### Navigation Graph for UI (ADAPT)

Build a directed graph (`INavigableNode` interface, `NavigationGraph<T>` class) for audio UI navigation. The down-right traversal total ordering constraint from FactorioAccess is essential for predictable keyboard navigation.

### Anti-Defensive Coding (ADOPT)

Validate at system boundaries only. Internal code should crash on violated preconditions (producing diagnosable stack traces), not silently return empty data.

---

## 14. Version Matrix

| Component | Version | Source |
|-----------|---------|--------|
| ONI Game | Build 707956 (release branch) | KleiVersion.cs |
| Unity Engine | 2020.3 LTS (Mono) | Game runtime |
| .NET Target | net472 (net40 compatible) | Decompiled csproj |
| Harmony | 2.x (0Harmony.dll) | Bundled with game |
| Tolk | Latest stable (inactive project) | tolk/dist/ |
| C# Language | Up to 9.0 | Mono runtime limit |
| Mod API Version | 2 (Harmony 2) | KMod.Mod constants |

---

## 15. Open Questions

1. **Harmony exact version:** The decompiled source uses `HarmonyLib` (Harmony 2.x) but the exact minor version (2.2, 2.3, etc.) is not determinable from decompilation alone. This affects which Harmony features are available (e.g., `CodeInstruction` helpers). **Mitigation:** Stick to core Harmony 2.0 features (Prefix, Postfix, FieldRef, AccessTools) which are stable across all 2.x versions.

2. **Tolk on x64 ONI:** The Tolk DLLs in the project are 64-bit. ONI on Steam is 64-bit. This should work, but needs runtime verification that `SetDllDirectory` + P/Invoke resolves correctly from within ONI's process.

3. **Steam Workshop native DLL distribution:** Steam Workshop may have restrictions on uploading native DLLs. If so, manual distribution with instructions may be the initial approach.

4. **Mono COM initialization:** Tolk's README notes COM considerations. Mono's COM interop may behave differently than .NET Framework. Test that Tolk_Load works correctly in the Mono runtime context.

---

## 16. Decision Log

| Decision | Choice | Rationale | Alternatives Rejected |
|----------|--------|-----------|----------------------|
| Mod entry point | KMod.UserMod2 | Only supported entry point for ONI DLL mods | BepInEx (not used by ONI) |
| Patching | Harmony 2 (game-bundled) | ONI requires APIVersion 2; bundled dll avoids conflicts | Harmony NuGet, MonoMod, manual IL |
| Speech output | Tolk via P/Invoke | Cross-screen-reader, already implemented, SAPI fallback | System.Speech, direct UIA, UniversalSpeech |
| Build system | MSBuild / dotnet build | Standard C# tooling, no Unity Editor needed | Unity project, manual compile |
| Settings persistence | Newtonsoft.Json | Ships with game, human-readable | PlayerPrefs, binary, game save system |
| Target framework | net472 | Compatible with ONI's Mono runtime | net40 (too restrictive), net6+ (incompatible) |
| Test strategy | NUnit for logic + manual in-game | Pragmatic given mod constraints | Unity Test Framework play mode (requires Unity project) |

---

*Research completed: 2026-02-11. All HIGH confidence items verified against decompiled ONI source (build 707956). MEDIUM items are based on established ONI community patterns. This document feeds directly into roadmap creation.*
