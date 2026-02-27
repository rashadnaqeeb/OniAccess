namespace OniAccess.Handlers.Screens.Research

// Shared helpers for building tech speech labels. Used by all three
// research tabs. All data is read live from game singletons on every call.

internal static class ResearchHelper (line 10)

  // Build a full spoken label for a tech in Browse or Tree context.
  // Format: "Name, state. cost. unlocks: items. description"
  internal static string BuildTechLabel(Tech tech) (line 15)

  // Build a label for a queued tech, including live progress.
  // Format: "Name, active/queued. progress per type. unlocks. description"
  internal static string BuildQueuedTechLabel(TechInstance ti, bool isActive) (line 56)

  // Build cost string: "15 Alpha Research, 30 Beta Research"
  internal static string BuildCostString(Tech tech) (line 81)

  // Build progress string: "15 of 50 Alpha Research, 0 of 30 Beta Research"
  internal static string BuildProgressString(TechInstance ti) (line 94)

  // Build prerequisite list: "needs Tech A completed, Tech B"
  internal static string BuildPrereqList(Tech tech) (line 110)

  // Build unlocks list: "unlocks Gas Pipe, Gas Pump"
  internal static string BuildUnlocksList(Tech tech) (line 126)

  // Build the global research point inventory string.
  internal static string BuildPointInventoryString() (line 138)
  // Returns null if UseGlobalPointInventory is false or inventory is empty.

  // Get all techs sorted by tier ascending, then name.
  internal static List<Tech> GetAllTechs() (line 157)

  // Get techs matching a bucket filter (0=available, 1=locked, 2=complete), sorted by tier then name.
  internal static List<Tech> GetTechsInBucket(int bucket) (line 169)

  // Get root techs (no prerequisites), ordered by tier then name.
  internal static List<Tech> GetRootTechs() (line 187)

  internal static string GetBucketName(int bucket) (line 200)
  // Maps 0->BUCKET_AVAILABLE, 1->BUCKET_LOCKED, 2->BUCKET_COMPLETED.

  static bool HasProgress(TechInstance ti) (line 209)
  // Returns true if any research type has > 0 points in progressInventory.

  internal static void PlayClickSound() (line 216)
  // Plays "HUD_Click_Open" sound; logs on failure.

  internal static void PlayRejectSound() (line 221)
  // Plays "Negative" sound; logs on failure.

  static string GetResearchTypeName(string typeId) (line 226)
  // Looks up Research.Instance.GetResearchType(typeId).name; falls back to typeId.
