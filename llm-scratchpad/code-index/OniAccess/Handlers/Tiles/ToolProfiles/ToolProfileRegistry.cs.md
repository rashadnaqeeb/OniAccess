# ToolProfileRegistry.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles

## Summary
Singleton registry that maps game tool types (e.g. `DigTool`, `BuildTool`) to their
`ToolProfile` (name + `GlanceComposer`). `Build()` constructs all profiles and sets
`Instance`. `MakeProfile` is a private convenience factory for the common
`[Selection, toolSection, Building, Element]` composer layout.

---

```
sealed class ToolProfileRegistry (line 5)
  static ToolProfileRegistry Instance { get; private set; } (line 6)
  internal static readonly ICellSection Selection (line 8)
    // Shared SelectionSection instance used by all standard tool profiles
  private readonly Dictionary<Type, ToolProfile> _profiles (line 10)

  void Register(Type toolType, ToolProfile profile) (line 13)
  GlanceComposer GetComposer(Type toolType) (line 17)
    // Returns null if the tool type has no registered profile
  static ToolProfileRegistry Build() (line 23)
    // Constructs all section instances, registers all tool profiles, sets Instance, returns registry
  private static ToolProfile MakeProfile(string name, ICellSection toolSection) (line 76)
    // Wraps a single tool-specific section in the standard [Selection, toolSection, Building, Element] composer
```
