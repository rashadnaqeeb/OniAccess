using System;
using System.Collections.Generic;

using Database;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Skills {
	/// <summary>
	/// Tab 2: NestedMenuHandler with categories (Dupe Info, Available, Locked, Mastered, Boosters).
	/// Level 0 = categories, level 1 = items within category, level 2 = hat list (only under hat entry).
	/// </summary>
	internal class SkillsTab: NestedMenuHandler, ISkillsTab {
		private readonly SkillsScreenHandler _parent;

		// Category indices
		private const int CAT_DUPE_INFO = 0;
		private const int CAT_AVAILABLE = 1;
		private const int CAT_LOCKED = 2;
		private const int CAT_MASTERED = 3;
		private const int CAT_BOOSTERS = 4;

		// Dupe Info item indices
		private const int INFO_NAME_POINTS = 0;
		private const int INFO_INTERESTS = 1;
		private const int INFO_MORALE = 2;
		private const int INFO_MORALE_NEED = 3;
		private const int INFO_XP = 4;
		private const int INFO_HAT = 5;

		internal SkillsTab(SkillsScreenHandler parent) : base(screen: null) {
			_parent = parent;
		}

		public string TabName => (string)STRINGS.ONIACCESS.SKILLS.SKILLS_TAB;

		public override string DisplayName => TabName;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry>(NestedNavHelpEntries) {
				new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL),
				new HelpEntry("Space", STRINGS.ONIACCESS.SKILLS.JUMP_TO_TREE_HELP),
				new HelpEntry("Enter", STRINGS.ONIACCESS.SKILLS.LEARN_HELP),
				new HelpEntry("+/-", STRINGS.ONIACCESS.SKILLS.BOOSTER_HELP),
			}.AsReadOnly();

		// ========================================
		// ISkillsTab
		// ========================================

		public void OnTabActivated(bool announce) {
			ResetState();
			if (announce)
				SpeechPipeline.SpeakInterrupt(TabName);
			if (ItemCount > 0) {
				string label = GetItemLabel(_currentIndex);
				if (!string.IsNullOrEmpty(label))
					SpeechPipeline.SpeakQueued(label);
			}
		}

		public void OnTabDeactivated() {
			_search.Clear();
		}

		public bool HandleInput() {
			// Space: jump to tree tab for current skill
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space) && Level == 1) {
				var skill = GetCurrentSkill();
				if (skill != null) {
					_parent.JumpToTreeTab(skill);
					return true;
				}
			}

			// Plus/Minus for booster assign/unassign
			if (Level == 1 && GetIndex(0) == GetBoosterCategoryIndex()) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Equals) ||
					UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.KeypadPlus)) {
					HandleBoosterAssign();
					return true;
				}
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Minus) ||
					UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.KeypadMinus)) {
					HandleBoosterUnassign();
					return true;
				}
			}

			return base.Tick();
		}

		public new bool HandleKeyDown(KButtonEvent e) {
			return base.HandleKeyDown(e);
		}

		// ========================================
		// NestedMenuHandler abstracts
		// ========================================

		protected override int MaxLevel => 2;
		protected override int SearchLevel => 1;
		protected override int StartLevel => 1;

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0) return GetCategoryCount();
			if (level == 1) return GetLevel1Count(indices[0]);
			if (level == 2) return GetLevel2Count(indices[0], indices[1]);
			return 0;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (level == 0) return GetCategoryName(indices[0]);
			if (level == 1) return GetLevel1Label(indices[0], indices[1]);
			if (level == 2) return GetLevel2Label(indices[0], indices[1], indices[2]);
			return null;
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level >= 1) return GetCategoryName(indices[0]);
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			if (indices[0] == CAT_DUPE_INFO && Level == 2) {
				// Hat selection
				SelectHat(indices[2]);
				return;
			}
			if (Level == 1) {
				int cat = indices[0];
				if (cat == CAT_DUPE_INFO) {
					if (indices[1] == INFO_HAT) {
						// Drill down handled by NestedMenuHandler
						return;
					}
					return;
				}
				if (cat == GetBoosterCategoryIndex()) {
					// Booster hint
					SpeechPipeline.SpeakInterrupt(
						STRINGS.ONIACCESS.SKILLS.BOOSTER_HINT);
					return;
				}
				// Skill entry: try to learn
				var skill = GetSkillAtLevel1(cat, indices[1]);
				if (skill != null)
					TryLearnSkill(skill);
			}
		}

		protected override int GetSearchTargetLevel(int flatIndex, int[] mappedIndices) {
			return 1;
		}

		// ========================================
		// Search across all skill categories (excluding Dupe Info and Boosters)
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			return GetAllSearchableSkills().Count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			var skills = GetAllSearchableSkills();
			if (flatIndex < 0 || flatIndex >= skills.Count) return null;
			return skills[flatIndex].Name;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			var skills = GetAllSearchableSkills();
			if (flatIndex < 0 || flatIndex >= skills.Count) return;
			var skill = skills[flatIndex];

			// Determine which category this skill falls into
			var identity = _parent.SelectedDupe;
			var model = SkillsHelper.GetDupeModel(identity);
			for (int cat = CAT_AVAILABLE; cat <= CAT_MASTERED; cat++) {
				var bucket = CategoryToBucket(cat);
				var bucketSkills = SkillsHelper.GetSkillsInBucket(
					bucket, identity, model);
				for (int i = 0; i < bucketSkills.Count; i++) {
					if (bucketSkills[i].Id == skill.Id) {
						outIndices[0] = cat;
						outIndices[1] = i;
						return;
					}
				}
			}
			// Fallback: skill not found in any bucket (timing edge case)
			outIndices[0] = CAT_AVAILABLE;
			outIndices[1] = 0;
		}

		// ========================================
		// Categories
		// ========================================

		private int GetCategoryCount() {
			return ShowBoosters() ? 5 : 4;
		}

		private string GetCategoryName(int cat) {
			switch (cat) {
				case CAT_DUPE_INFO: return STRINGS.ONIACCESS.SKILLS.BUCKET_DUPE_INFO;
				case CAT_AVAILABLE: return STRINGS.ONIACCESS.SKILLS.BUCKET_AVAILABLE;
				case CAT_LOCKED: return STRINGS.ONIACCESS.SKILLS.BUCKET_LOCKED;
				case CAT_MASTERED: return STRINGS.ONIACCESS.SKILLS.BUCKET_MASTERED;
				case CAT_BOOSTERS: return STRINGS.ONIACCESS.SKILLS.BUCKET_BOOSTERS;
				default: return "";
			}
		}

		private int GetBoosterCategoryIndex() {
			return ShowBoosters() ? CAT_BOOSTERS : -1;
		}

		private bool ShowBoosters() {
			var identity = _parent.SelectedDupe;
			if (identity == null) return false;
			if (SkillsHelper.IsStored(identity)) return false;
			return SkillsHelper.IsBionic(identity);
		}

		// ========================================
		// Level 1 items
		// ========================================

		private int GetLevel1Count(int cat) {
			var identity = _parent.SelectedDupe;
			if (identity == null) return 0;
			var model = SkillsHelper.GetDupeModel(identity);

			switch (cat) {
				case CAT_DUPE_INFO:
					return SkillsHelper.IsStored(identity) ? 1 : 6;
				case CAT_AVAILABLE:
				case CAT_LOCKED:
				case CAT_MASTERED:
					return SkillsHelper.GetSkillsInBucket(
						CategoryToBucket(cat), identity, model).Count;
				case CAT_BOOSTERS:
					return GetBoosterItemCount();
				default:
					return 0;
			}
		}

		private string GetLevel1Label(int cat, int idx) {
			var identity = _parent.SelectedDupe;
			if (identity == null) return null;
			var model = SkillsHelper.GetDupeModel(identity);

			switch (cat) {
				case CAT_DUPE_INFO: {
						var labels = SkillsHelper.BuildDupeInfoLabels(identity);
						return idx < labels.Count ? labels[idx] : null;
					}
				case CAT_AVAILABLE:
				case CAT_LOCKED:
				case CAT_MASTERED: {
						var skills = SkillsHelper.GetSkillsInBucket(
							CategoryToBucket(cat), identity, model);
						if (idx < 0 || idx >= skills.Count) return null;
						return SkillsHelper.BuildSkillLabel(skills[idx], identity);
					}
				case CAT_BOOSTERS:
					return GetBoosterLabel(idx);
				default:
					return null;
			}
		}

		// ========================================
		// Level 2: Hat list
		// ========================================

		private int GetLevel2Count(int cat, int idx) {
			if (cat == CAT_DUPE_INFO && idx == INFO_HAT) {
				var resume = SkillsHelper.GetResume(_parent.SelectedDupe);
				if (resume == null) return 0;
				return SkillsHelper.GetAvailableHats(resume).Count;
			}
			return 0;
		}

		private string GetLevel2Label(int cat, int idx, int subIdx) {
			if (cat == CAT_DUPE_INFO && idx == INFO_HAT) {
				var resume = SkillsHelper.GetResume(_parent.SelectedDupe);
				if (resume == null) return null;
				var hats = SkillsHelper.GetAvailableHats(resume);
				if (subIdx < 0 || subIdx >= hats.Count) return null;
				return hats[subIdx].Name;
			}
			return null;
		}

		private void SelectHat(int hatIdx) {
			var resume = SkillsHelper.GetResume(_parent.SelectedDupe);
			if (resume == null) {
				SkillsHelper.PlayRejectSound();
				return;
			}
			var hats = SkillsHelper.GetAvailableHats(resume);
			if (hatIdx < 0 || hatIdx >= hats.Count) return;
			var hat = hats[hatIdx];
			if (string.IsNullOrEmpty(hat.HatId)) {
				// "None" — remove hat immediately
				resume.SetHats(resume.CurrentHat, null);
				resume.ApplyTargetHat();
			} else {
				// Actual hat — set target and queue the chore
				resume.SetHats(resume.CurrentHat, hat.HatId);
				if (resume.OwnsHat(hat.HatId))
					new PutOnHatChore(resume, Db.Get().ChoreTypes.SwitchHat);
			}
			var skillsScreen = _parent.Screen as SkillsScreen;
			if (skillsScreen != null)
				skillsScreen.RefreshAll();
			SkillsHelper.PlayClickSound();
			string msg = string.IsNullOrEmpty(hat.HatId)
				? string.Format(STRINGS.ONIACCESS.SKILLS.HAT_SELECTED, hat.Name)
				: string.Format(STRINGS.ONIACCESS.SKILLS.HAT_QUEUED, hat.Name);
			SpeechPipeline.SpeakInterrupt(msg);
		}

		// ========================================
		// Skill actions
		// ========================================

		private Skill GetCurrentSkill() {
			if (Level < 1) return null;
			int cat = GetIndex(0);
			if (cat == CAT_DUPE_INFO || cat == GetBoosterCategoryIndex()) return null;
			return GetSkillAtLevel1(cat, GetIndex(1));
		}

		private Skill GetSkillAtLevel1(int cat, int idx) {
			var identity = _parent.SelectedDupe;
			if (identity == null) return null;
			var model = SkillsHelper.GetDupeModel(identity);
			var skills = SkillsHelper.GetSkillsInBucket(
				CategoryToBucket(cat), identity, model);
			if (idx < 0 || idx >= skills.Count) return null;
			return skills[idx];
		}

		private void TryLearnSkill(Skill skill) {
			SkillsHelper.TryLearnSkill(skill, _parent.SelectedDupe, _parent.Screen);
		}

		// ========================================
		// Boosters
		// ========================================

		private int GetBoosterItemCount() {
			SkillsHelper.ResolveDupe(
				_parent.SelectedDupe, out var minionIdentity, out _);
			if (minionIdentity == null) return 0;
			// GetBoosterEntries already logs on failure and returns empty
			var entries = SkillsHelper.GetBoosterEntries(minionIdentity);
			return 1 + entries.Count;
		}

		private string GetBoosterLabel(int idx) {
			SkillsHelper.ResolveDupe(
				_parent.SelectedDupe, out var minionIdentity, out _);
			if (minionIdentity == null) return null;
			if (idx == 0)
				return SkillsHelper.BuildSlotSummary(minionIdentity);
			var entries = SkillsHelper.GetBoosterEntries(minionIdentity);
			int entryIdx = idx - 1;
			if (entryIdx < 0 || entryIdx >= entries.Count) return null;
			return SkillsHelper.BuildBoosterLabel(entries[entryIdx]);
		}

		private void HandleBoosterAssign() {
			SkillsHelper.ResolveDupe(
				_parent.SelectedDupe, out var minionIdentity, out _);
			if (minionIdentity == null) { SkillsHelper.PlayRejectSound(); return; }
			int idx = GetIndex(1) - 1; // Subtract 1 for slot summary
			if (idx < 0) { SkillsHelper.PlayRejectSound(); return; }
			try {
				var entries = SkillsHelper.GetBoosterEntries(minionIdentity);
				if (idx >= entries.Count) { SkillsHelper.PlayRejectSound(); return; }
				var entry = entries[idx];
				if (entry.AvailableCount <= 0) {
					SkillsHelper.PlayRejectSound();
					SpeechPipeline.SpeakInterrupt(
						STRINGS.ONIACCESS.SKILLS.NO_BOOSTERS_AVAILABLE);
					return;
				}
				if (SkillsHelper.TryAssignBooster(minionIdentity, entry.Tag)) {
					RefreshGameScreen();
					SkillsHelper.PlayClickSound();
					SpeechPipeline.SpeakInterrupt(
						STRINGS.ONIACCESS.SKILLS.BOOSTER_ASSIGNED);
				} else {
					SkillsHelper.PlayRejectSound();
					SpeechPipeline.SpeakInterrupt(
						STRINGS.ONIACCESS.SKILLS.NO_EMPTY_SLOTS);
				}
			} catch (Exception ex) {
				Util.Log.Warn($"SkillsTab.HandleBoosterAssign: {ex.Message}");
				SkillsHelper.PlayRejectSound();
			}
		}

		private void HandleBoosterUnassign() {
			SkillsHelper.ResolveDupe(
				_parent.SelectedDupe, out var minionIdentity, out _);
			if (minionIdentity == null) { SkillsHelper.PlayRejectSound(); return; }
			int idx = GetIndex(1) - 1;
			if (idx < 0) { SkillsHelper.PlayRejectSound(); return; }
			try {
				var entries = SkillsHelper.GetBoosterEntries(minionIdentity);
				if (idx >= entries.Count) { SkillsHelper.PlayRejectSound(); return; }
				var entry = entries[idx];
				if (entry.AssignedCount <= 0) {
					SkillsHelper.PlayRejectSound();
					SpeechPipeline.SpeakInterrupt(
						STRINGS.ONIACCESS.SKILLS.NONE_ASSIGNED);
					return;
				}
				if (SkillsHelper.TryUnassignBooster(minionIdentity, entry.Tag)) {
					RefreshGameScreen();
					SkillsHelper.PlayClickSound();
					SpeechPipeline.SpeakInterrupt(
						STRINGS.ONIACCESS.SKILLS.BOOSTER_UNASSIGNED);
				} else {
					SkillsHelper.PlayRejectSound();
				}
			} catch (Exception ex) {
				Util.Log.Warn($"SkillsTab.HandleBoosterUnassign: {ex.Message}");
				SkillsHelper.PlayRejectSound();
			}
		}

		private void RefreshGameScreen() {
			var skillsScreen = _parent.Screen as SkillsScreen;
			if (skillsScreen != null)
				skillsScreen.RefreshAll();
		}

		// ========================================
		// Helpers
		// ========================================

		private static SkillsHelper.Bucket CategoryToBucket(int cat) {
			switch (cat) {
				case CAT_AVAILABLE: return SkillsHelper.Bucket.Available;
				case CAT_LOCKED: return SkillsHelper.Bucket.Locked;
				case CAT_MASTERED: return SkillsHelper.Bucket.Mastered;
				default: return SkillsHelper.Bucket.DupeInfo;
			}
		}

		private List<Skill> GetAllSearchableSkills() {
			var identity = _parent.SelectedDupe;
			var model = SkillsHelper.GetDupeModel(identity);
			return SkillsHelper.GetSkillsForModel(model);
		}
	}
}
