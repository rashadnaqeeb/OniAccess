// Decides which building status items to speak based on the active overlay.
// Overlay-specific warnings only appear when their overlay is active;
// the default set covers everything else.

static class StatusFilter (line 9)
  // Status item IDs that only appear in a specific overlay.
  // Keyed by overlay mode HashedString, values are sets of StatusItem.Id.
  private static Dictionary<HashedString, HashSet<string>> overlayItems (line 14)

  // Neutral status items that should always be spoken regardless of overlay.
  private static HashSet<string> alwaysNeutrals (line 19)

  // Union of all overlay-specific IDs, for fast exclusion in default mode.
  private static HashSet<string> allOverlayItems (line 24)

  public static void Initialize() (line 26)
    // Populates overlayItems with per-overlay sets of status item IDs:
    //   Power: NeedPower, NotEnoughPower, PowerLoopDetected, NoWireConnected,
    //          NoPowerConsumers, WireDisconnected, GeneratorOffline, PowerButtonOff,
    //          Overloaded, ReactorRefuelDisabled
    //   Gas:   NeedGasIn, NeedGasOut, GasPipeEmpty, GasVentObstructed,
    //          GasVentOverPressure, NoGasElementToPump
    //   Liquid: NeedLiquidIn, NeedLiquidOut, LiquidPipeEmpty, LiquidVentObstructed,
    //           LiquidVentOverPressure, NoLiquidElementToPump
    //   Solid:  NeedSolidIn, NeedSolidOut
    //   Logic:  NoLogicWireConnected, LogicOverloaded, LogicSwitchStatusActive,
    //           LogicSwitchStatusInactive, LogicSensorStatusActive, LogicSensorStatusInactive
    //   Farming: NeedPlant, NeedSeed, NoAvailableSeed, NeedEgg, NoAvailableEgg,
    //            NoFishableWaterBelow, TrapNeedsArming, NoLureElementSelected,
    //            AwaitingSeedDelivery, AwaitingBaitDelivery, AwaitingEggDelivery,
    //            CREATURE_REUSABLE_TRAP.READY, CREATURE_REUSABLE_TRAP.SPRUNG
    //   Oxygen: PressureOk, UnderPressure
    // Builds allOverlayItems as union of all sets.
    // Populates alwaysNeutrals: UnderConstruction, UnderConstructionNoWorker,
    //   PendingSwitchToggle, PendingUpgrade, BuildingDisabled, Expired,
    //   WaitingForRepairMaterials, MissingRequirements

  // Returns true if the active overlay is a focused utility view
  // that should skip non-essential building layers like backwalls.
  public static bool IsOverlayFocused(HashedString activeOverlay) (line 95)
    // Returns overlayItems.ContainsKey(activeOverlay).

  // Returns true if this status item should be spoken given the active overlay.
  public static bool ShouldSpeak(StatusItem item, HashedString activeOverlay, bool isPlant) (line 102)
    // Logic:
    //   1. If activeOverlay has a set in overlayItems: speak only items in that set
    //      (plus Neutral plant items in the Crop overlay).
    //   2. Otherwise (default view): always speak alwaysNeutrals.
    //   3. Suppress all other Neutral/positive severity items.
    //   4. Speak Bad/BadMinor items not claimed by any overlay set.
