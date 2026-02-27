# ToolProfile.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles

## Summary
Pairs a tool's name with the GlanceComposer that produces cell speech while that tool is active.

---

```
sealed class ToolProfile (line 6)
  string ToolName { get; } (line 7)
  GlanceComposer Composer { get; } (line 8)
  ToolProfile(string toolName, GlanceComposer composer) (line 10)
```
