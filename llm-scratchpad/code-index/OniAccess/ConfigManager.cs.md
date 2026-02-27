// Loads and saves ModConfig from a YAML file (oni-access-config.yml) next to the mod DLL.
// Falls back to defaults on missing file, null parse, or any exception.
static class ConfigManager (line 7)
  private const string FileName = "oni-access-config.yml" (line 8)
  private static string _path (line 9)

  static ModConfig Config { get; private set; } (line 11)

  // Loads config from modDir/oni-access-config.yml. Creates defaults if file absent or parse fails.
  static void Load(string modDir) (line 13)

  // Saves current Config back to the same path it was loaded from.
  static void Save() (line 34)

  // YAML error callback passed to YamlIO.LoadFile. Routes to Log.Warn or Log.Error based on severity.
  private static void OnYamlError(YamlIO.Error error, bool forceLogAsWarning) (line 42)
