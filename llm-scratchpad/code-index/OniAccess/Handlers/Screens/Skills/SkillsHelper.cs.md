namespace OniAccess.Handlers.Screens.Skills

internal static class SkillsHelper (line 10)

  // --- DUPE QUERIES ---

  internal static void ResolveDupe(IAssignableIdentity identity, out MinionIdentity minionIdentity, out StoredMinionIdentity storedIdentity) (line 15)
  // If identity is MinionAssignablesProxy, resolves via proxy.GetTargetGameObject().
  // Otherwise casts directly. One of the two out params will be non-null.

  internal static MinionResume GetResume(IAssignableIdentity identity) (line 29)
  // Returns MinionResume component from the resolved MinionIdentity, or null for stored dupes.

  internal static Tag GetDupeModel(IAssignableIdentity identity) (line 36)
  // Returns minionIdentity.model or storedIdentity.model; Tag.Invalid if both null.

  internal static bool IsStored(IAssignableIdentity identity) (line 45)
  // Returns true if ResolveDupe yields no MinionIdentity (i.e., identity is stored).

  // --- SKILL QUERIES ---

  internal static List<Skill> GetSkillsForModel(Tag model) (line 54)
  // Returns non-deprecated skills matching model (null model = any model accepted).
  // Sorted by tier, then skill group name, then skill name.

  internal static List<Skill> GetRootSkills(Tag model) (line 73)
  // Returns non-deprecated skills with no priorSkills, matching model.
  // Sorted by skill group name, then skill name.

  internal static IReadOnlyList<Skill> GetChildren(Skill parent) (line 91)
  // Returns non-deprecated skills that list parent.Id in their priorSkills.

  internal static IReadOnlyList<Skill> GetParents(Skill child) (line 101)
  // Returns non-deprecated skills referenced by child.priorSkills.
  // Returns Array.Empty if priorSkills is null or empty.

  // --- BUCKET CLASSIFICATION ---

  internal enum Bucket { DupeInfo, Available, Locked, Mastered, Boosters } (line 117)

  internal static List<Skill> GetSkillsInBucket(Bucket bucket, IAssignableIdentity identity, Tag model) (line 119)
  // Filters GetSkillsForModel by ClassifySkill result.

  private static Bucket ClassifySkill(Skill skill, MinionResume resume, IAssignableIdentity identity) (line 133)
  // With resume: Mastered > CanMaster (Available) > else Locked.
  // Without resume (stored): checks StoredMinionIdentity.HasMasteredSkill; else Locked.

  // --- LABEL BUILDERS ---

  internal static string BuildDupeLabel(IAssignableIdentity identity) (line 154)
  // Stored: "name, stored". Live: "name, N points, morale/expectation".

  internal static string BuildSkillLabel(Skill skill, IAssignableIdentity identity) (line 178)
  // Parts: name, group name, status detail, description, perks, morale cost (if not mastered/granted),
  // mastered-by count. Joined with ". ".

  private static void AddStatusDetail(List<string> parts, Skill skill, MinionResume resume, IAssignableIdentity identity) (line 216)
  // Mastered -> "granted" or "mastered". CanMaster -> "available" + optional stress/aptitude flags.
  // Locked -> GetLockReason result. Stored -> "mastered" or "locked".

  internal static string GetLockReason(Skill skill, MinionResume resume) (line 249)
  // Returns blocking trait name (BLOCKED_BY), CANNOT_LEARN, prerequisite list (NEEDS),
  // NO_SKILL_POINTS, or generic LOCKED depending on conditions.

  private static string GetBlockingTraitName(Skill skill, MinionResume resume) (line 269)
  // Gets the skill group's choreGroupID, then calls traits.IsChoreGroupDisabled.

  private static List<string> GetMissingPrereqs(Skill skill, MinionResume resume) (line 279)
  // Returns names of priorSkill IDs that the resume has not yet mastered.

  internal static string BuildPerksList(Skill skill) (line 290)
  // Returns DLC-filtered perk descriptions joined with ", "; null if no perks.

  internal static string BuildPrereqList(Skill skill) (line 301)
  // Returns "NEEDS name1, name2" or null if no priorSkills.

  internal static int CountMasters(string skillId) (line 312)
  // Counts live MinionIdentities whose MinionResume.HasMasteredSkill returns true.

  // --- DUPE INFO LABELS ---

  internal static List<string> BuildDupeInfoLabels(IAssignableIdentity identity) (line 326)
  // Returns a list of label strings for the Dupe Info category:
  // [0] name + points, [1] interests, [2] morale breakdown,
  // [3] morale need breakdown, [4] XP progress, [5] current hat.

  private static string BuildInterestsLabel(MinionResume resume) (line 370)
  // Lists skill group names with aptitude > 0; falls back to NO_INTERESTS.

  private static string BuildModifierBreakdown(string header, AttributeInstance attr) (line 383)
  // "header total, mod1 +val, mod2 -val, ..." (skips zero-value modifiers).

  // --- HAT QUERIES ---

  internal static string GetCurrentHatName(MinionResume resume) (line 403)
  // Returns skill.Name for the hat skill matching TargetHat ?? CurrentHat.
  // Falls back to the raw hat ID if no matching skill is found.

  internal static List<HatEntry> GetAvailableHats(MinionResume resume) (line 415)
  // Returns a list starting with a "None" entry, followed by entries from resume.GetAllHats().

  internal struct HatEntry (line 424)
    internal readonly string Name (line 425)
    internal readonly string HatId (line 426)
    internal HatEntry(string name, string hatId) (line 427)

  // --- BOOSTER QUERIES (DLC3-safe) ---

  internal static bool IsBionic(IAssignableIdentity identity) (line 437)
  // Returns false if DLC3 not subscribed. Checks model == GameTags.Minions.Models.Bionic.

  internal static string BuildSlotSummary(MinionIdentity minionIdentity) (line 449)
  // Returns BOOSTER_SLOTS format string with assigned/unlocked slot counts from BionicUpgradesMonitor.

  internal static List<BoosterEntry> GetBoosterEntries(MinionIdentity minionIdentity) (line 461)
  // Reads BionicUpgradesMonitor slot assignments and known prefabs with BionicUpgradeComponent.
  // For each booster type: counts assigned slots and unassigned pickupables in dupe's world.
  // Reads description from BionicUpgradeComponentConfig.UpgradesData.

  internal static bool TryAssignBooster(MinionIdentity minionIdentity, Tag boosterTag) (line 519)
  // Finds a truly empty (not locked, not assigned, not mid-ejection) slot, then assigns
  // the first unassigned pickupable of the given tag from the dupe's world inventory.

  internal static bool TryUnassignBooster(MinionIdentity minionIdentity, Tag boosterTag) (line 555)
  // First pass: installed+matching slot (game's DecrementBoosterAssignment order, reverse index).
  // Second pass: any assigned slot of this type (also reverse index).

  internal struct BoosterEntry (line 588)
    internal readonly Tag Tag (line 589)
    internal readonly string Name (line 590)
    internal readonly int AssignedCount (line 591)
    internal readonly int AvailableCount (line 592)
    internal readonly string Description (line 593)
    internal BoosterEntry(Tag tag, string name, int assigned, int available, string desc) (line 594)

  internal static string BuildBoosterLabel(BoosterEntry entry) (line 604)
  // Parts: name, "assigned N", "available N", description. Joined with ". ".

  // --- ACTIONS ---

  internal static void TryLearnSkill(Skill skill, IAssignableIdentity identity, KScreen screen) (line 621)
  // Guards: no resume -> reject. Already mastered -> reject + "mastered".
  // Conditions not met -> reject + GetLockReason.
  // On success: calls resume.MasterSkill, refreshes SkillsScreen, plays click, speaks LEARNED.

  // --- SOUNDS ---

  internal static void PlayClickSound() (line 657)
  // Plays "HUD_Click_Open"; logs on failure.

  internal static void PlayRejectSound() (line 662)
  // Plays "Negative"; logs on failure.
