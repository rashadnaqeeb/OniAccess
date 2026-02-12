# OniAccess - Claude Code Instructions

## Build

Always use the build script, never `dotnet build` directly. The project references game assemblies via the `ONI_MANAGED` environment variable which the build script sets automatically.

```
powershell -ExecutionPolicy Bypass -File build.ps1
```

The script builds the DLL, deploys it to the game's local mods directory, and patches mods.json to keep the mod enabled.

LSP diagnostics about missing types (KScreen, KButton, KToggle, etc.) are expected — the language server cannot resolve game assembly references. Ignore these; use the build script to verify compilation.

## Project Structure

- `OniAccess/` — mod source code (C#, .NET Framework 4.7.2, Harmony patches)
- `ONI-Decompiled/` — decompiled game source for reference (read-only, not part of build)
- `tolk/` — screen reader bridge library
- `docs/` — design documentation
- `.planning/` — project planning files
- `bugs.md` — known bug tracker
