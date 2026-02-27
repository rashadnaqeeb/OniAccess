// Mod entry point. Bootstraps all subsystems on load.
class Mod : KMod.UserMod2 (line 13)
  static Mod Instance { get; private set; } (line 14)
  static string ModDir { get; private set; } (line 15)
  static string Version { get; private set; } (line 16)

  // P/Invoke: sets the DLL search path so Windows can find Tolk's native DLLs.
  [DllImport] private static extern bool SetDllDirectory(string lpPathName) (line 19)

  // Runs at mod load: installs Unity logging, loads config, sets the Tolk DLL directory,
  // initializes speech/filter/status, creates the KeyPoller GameObject, registers
  // screen handlers, pushes the baseline handler, and speaks the startup announcement.
  override void OnLoad(Harmony harmony) (line 21)
