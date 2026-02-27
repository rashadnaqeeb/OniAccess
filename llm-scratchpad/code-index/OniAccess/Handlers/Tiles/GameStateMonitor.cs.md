namespace OniAccess.Handlers.Tiles

class GameStateMonitor (line 4)
  // Tracks pause state, game speed, and cycle number each frame.
  // Speaks changes via SpeechPipeline.SpeakInterrupt. The first tick
  // initializes state without speaking to avoid spurious announcements on load.

  private bool _firstTick (line 5)
  private bool _wasPaused (line 6)
  private int _lastSpeed (line 7)
  private int _lastCycle (line 8)

  void Tick() (line 10)
    // Called every frame from TileCursorHandler.Tick().
    // On first call: captures initial state, returns without speaking.
    // On pause/unpause change: speaks PAUSEBUTTON or UNPAUSED with speed name.
    // On speed change without pause change: speaks the new speed name alone.
    // On cycle change: speaks CYCLE with the new cycle number.

  void SpeakCycleStatus() (line 43)
    // Speaks current cycle number and schedule block (hour) via CYCLE_STATUS.
    // Called on Q keypress in TileCursorHandler.

  private static string SpeedName(int speed) (line 50)
    // Maps speed int (0/1/2) to STRINGS.UI.SPEED_SLOW/MEDIUM/FAST.
    // Defaults to SPEED_SLOW for unknown values.
