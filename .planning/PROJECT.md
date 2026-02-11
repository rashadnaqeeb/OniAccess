# Oni-Access

## What This Is

A Harmony mod for Oxygen Not Included that adds screen reader accessibility, making the game playable by blind players. Outputs speech via Tolk (with SAPI fallback) to describe game state, enable navigation, and provide environmental awareness through audio. Covers the base game and all DLC (Spaced Out, Bionic Booster Pack).

## Core Value

A blind player can play a full colony — from game setup through late-game systems — with an experience designed for audio, not a translation of the visual interface.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] Menu navigation (main menu, options, new game setup, colony creation)
- [ ] Grid-based world cursor with tile readout (element, temperature, building, contents)
- [ ] Building construction (build menu, placement, deconstruction)
- [ ] Duplicant management (status, attributes, priorities, schedules, skills)
- [ ] Entity inspection (buildings, critters, plants, geysers with detail panels)
- [ ] Overlay system (oxygen, power, temperature, plumbing, etc. as speech layers)
- [ ] Management screens (research, vitals, consumables, reports, starmap)
- [ ] Notification awareness (alerts surfaced as speech)
- [ ] Scanner system (find and navigate to entities by category)
- [ ] Room system awareness
- [ ] Automation system access
- [ ] Rocketry & multi-asteroid (Spaced Out DLC)
- [ ] Bionic Booster Pack content

### Out of Scope

- Atmospheric sonification (gas timbre, pressure volume, temperature texture) — nice-to-have, not required for playability
- Visual accessibility (colorblind modes, high contrast) — this mod is specifically for screen reader users
- Mobile/console — Windows only, Steam release
- Multiplayer — ONI is single-player

## Context

**Extensive prior research exists in `docs/`:**
- `oni-accessibility-analysis.md` — Full mapping of FactorioAccess concepts to ONI, identifies what transfers, what adapts, what must be invented
- `oni-accessibility-audit.md` — Complete inventory of every UI element, overlay, management screen, and interaction pattern in ONI
- `oni-accessibility-lessons.md` — 18 architectural patterns from FactorioAccess with C# translation guidance
- `fa-architecture-lessons.md` — FactorioAccess design principles (information pipeline, anti-defensive coding, audio-first messages)
- `fa-features.md` — 36 FactorioAccess feature systems catalogued with applicability analysis
- `oni-internals.md` — ONI modding reference (screen system, input handling, UI components, Harmony patterns)
- `CODEBASE_INDEX.md` — Complete namespace reference for decompiled ONI source

**Decompiled game source** available at `ONI-Decompiled/` (~5000+ C# files across Assembly-CSharp and Assembly-CSharp-firstpass). Unity game with custom Klei UI framework (KScreen, KMonoBehaviour, LocText, etc.).

**Speech system** already implemented in `Speech.cs` — Tolk P/Invoke wrapper with rich text filtering, sprite tag conversion, SAPI fallback.

**Key ONI characteristics that affect design:**
- Every tile has 10+ overlapping data layers (element, temperature, germs, pipes, wires, buildings, decor, light, room)
- Side-view 2D grid with gravity (vertical axis matters, unlike Factorio's flat world)
- 15 overlay modes, 15 build categories, 100+ side screen variants
- Duplicants are autonomous agents (managing 3-30+ of them is core gameplay)
- 5 overlapping conduit networks on same tiles (power, liquid, gas, automation, conveyor)

## Design Principles

### 1. Accessible experience, not visual mimicry
Reimagine interfaces for audio. A research tree shown as nodes can become sorted lists. A priority grid can become a focused per-duplicant workflow. Design what works for ears, not what mirrors eyes.

### 2. Reuse game data, avoid hardcoding
Use the game's localized text (`STRINGS` namespace, `LocText` components), UI state, and entity data wherever possible. Hardcoded text becomes stale across game updates and blocks translation. Only hardcode when no game data source exists. Avoid caching for the same reason — stale data is worse than a re-query.

### 3. Concise announcements
Users are experienced screen reader users. Announce name, state, value — nothing more.
- No item counts ("3 of 10")
- No navigation hints ("press Enter to select") unless unusual controls, and on a delay
- No redundant context ("You are now in...")
- No type suffixes when obvious ("Lumber button")

### 4. Conscious hotkey management
ONI has extensive hotkeys. Many are useless to blind players and can be overwritten. But every overwrite is a deliberate decision — document what the original hotkey did and why it's being replaced.

## Build & Deploy

**Build requirement**: Every phase MUST compile against the game's DLLs before completion. Never ship uncompiled code.

**ONI game path**: `C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\`
**Managed DLLs**: `OxygenNotIncluded_Data\Managed\` (set `ONI_MANAGED` env var to this path)
**Deploy target**: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\local\OniAccess\`

**Build command**:
```bash
export ONI_MANAGED="C:/Program Files (x86)/Steam/steamapps/common/OxygenNotIncluded/OxygenNotIncluded_Data/Managed"
dotnet build OniAccess/OniAccess.csproj -c Release
```

**Deploy command**:
```bash
DEST="$USERPROFILE/Documents/Klei/OxygenNotIncluded/mods/local/OniAccess"
mkdir -p "$DEST/tolk/dist"
cp OniAccess/bin/Release/net472/OniAccess.dll "$DEST/"
cp OniAccess/mod_info.yaml "$DEST/"
cp tolk/dist/* "$DEST/tolk/dist/"
```

**Known type conflicts with ONI assemblies** (pitfalls for all phases):
- `Action` — ONI defines its own `Action` type that shadows `System.Action`. Always use `System.Action` explicitly.
- `Input` — If your code is in namespace `OniAccess.Input`, bare `Input.GetKeyDown` resolves to the namespace, not `UnityEngine.Input`. Always fully qualify `UnityEngine.Input`.
- `UnityEngine.InputLegacyModule` — Must be referenced in csproj; `UnityEngine.Input` is forwarded to this assembly.

## Constraints

- **Engine**: Unity (C#), modded via Harmony patches on KMod.UserMod2 entry point
- **Speech**: Tolk library (P/Invoke) with SAPI fallback — DLLs in `tolk/dist/`
- **Target**: Latest Steam release of ONI (base + all DLC)
- **Platform**: Windows only (Tolk is Windows-only)
- **Distribution**: Steam Workshop if feasible, otherwise manual install. Decide later.
- **No game source modification**: All changes via Harmony patches, reflection, and new components
- **Build gate**: Every plan must compile successfully against game DLLs before being marked complete

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Tolk for speech output | Cross-screen-reader support (NVDA, JAWS, SAPI fallback), P/Invoke wrapper already built | -- Pending |
| Audio-first over visual mimicry | Blind users need interfaces designed for sequential audio, not spatial visual layouts | -- Pending |
| Reuse game text over hardcoding | Survives game updates, enables community translations, reduces maintenance | -- Pending |
| Sonification as nice-to-have | Speech descriptions of environment may be sufficient; sonification is high-effort and unproven | -- Pending |
| All DLC in scope | User wants complete game access, not a subset | -- Pending |

---
*Last updated: 2026-02-11 after initialization*
