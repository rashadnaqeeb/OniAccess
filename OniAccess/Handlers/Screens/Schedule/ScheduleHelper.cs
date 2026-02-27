using System.Collections.Generic;

using Database;

namespace OniAccess.Handlers.Screens.Schedule {
	internal static class ScheduleHelper {
		/// Game ScheduleGroup IDs in number-key order: 1=Worktime, 2=Hygene,
		/// 3=Recreation, 4=Sleep. Differs from the game's allGroups insertion
		/// order (Hygene first) for usability — Work as key 1 is more natural.
		internal static readonly string[] BrushGroupIds = {
			"Worktime", "Hygene", "Recreation", "Sleep"
		};

		internal static string BuildDupeLabel(MinionIdentity mi) {
			string name = mi.GetProperName();
			var traits = mi.GetComponent<Klei.AI.Traits>();
			string traitTag = null;
			if (traits != null) {
				if (traits.HasTrait("NightOwl"))
					traitTag = Db.Get().traits.Get("NightOwl").Name;
				else if (traits.HasTrait("EarlyBird"))
					traitTag = Db.Get().traits.Get("EarlyBird").Name;
			}
			var schedulable = mi.GetComponent<Schedulable>();
			var schedule = ScheduleManager.Instance.GetSchedule(schedulable);
			string scheduleName = schedule != null ? schedule.name : "?";

			if (traitTag != null)
				return $"{name}, {traitTag}, {scheduleName}";
			return $"{name}, {scheduleName}";
		}

		internal static string GetGroupName(string groupId) {
			var group = Db.Get().ScheduleGroups.Get(groupId);
			return group != null ? group.Name : groupId;
		}

		internal static string BuildCellLabel(global::Schedule schedule, int timetableIdx, int col) {
			int blockIdx = timetableIdx * 24 + col;
			var block = schedule.GetBlock(blockIdx);
			string groupName = GetGroupName(block.GroupId);
			return string.Format(STRINGS.ONIACCESS.SCHEDULE.BLOCK_LABEL, groupName, col);
		}

		internal static string BuildWarnings(global::Schedule schedule) {
			var blockTypeCounts = new Dictionary<string, int>();
			foreach (var bt in Db.Get().ScheduleBlockTypes.resources)
				blockTypeCounts[bt.Id] = 0;
			foreach (var block in schedule.GetBlocks()) {
				foreach (var at in block.allowed_types)
					blockTypeCounts[at.Id]++;
			}
			var warnings = new List<string>();
			foreach (var kvp in blockTypeCounts) {
				if (kvp.Value == 0) {
					string typeName = Db.Get().ScheduleBlockTypes.Get(kvp.Key).Name;
					warnings.Add(string.Format(
						STRINGS.UI.SCHEDULEGROUPS.NOTIME, typeName));
				}
			}
			return warnings.Count > 0 ? string.Join(". ", warnings) : null;
		}

		internal static int GetTimetableCount(global::Schedule schedule) {
			return schedule.GetBlocks().Count / 24;
		}

		internal static ScheduleScreenEntry GetScreenEntry(ScheduleScreen screen, int scheduleIndex) {
			try {
				var entries = HarmonyLib.Traverse.Create(screen)
					.Field<List<ScheduleScreenEntry>>("scheduleEntries").Value;
				if (entries != null && scheduleIndex >= 0 && scheduleIndex < entries.Count)
					return entries[scheduleIndex];
			} catch (System.Exception ex) {
				Util.Log.Warn($"ScheduleHelper.GetScreenEntry: {ex.Message}");
			}
			return null;
		}

		internal static KInputTextField GetEntryInputField(ScheduleScreen screen, int scheduleIndex) {
			var entry = GetScreenEntry(screen, scheduleIndex);
			if (entry == null) return null;
			try {
				var titleBar = HarmonyLib.Traverse.Create(entry)
					.Field<EditableTitleBar>("title").Value;
				return titleBar?.inputField;
			} catch (System.Exception ex) {
				Util.Log.Warn($"ScheduleHelper.GetEntryInputField: {ex.Message}");
				return null;
			}
		}

		internal static void PlayClickSound() {
			try { KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click")); }
			catch (System.Exception ex) { Util.Log.Error($"PlayClickSound failed: {ex.Message}"); }
		}

		internal static void PlayHoverSound() => BaseScreenHandler.PlayHoverSound();

		internal static void PlayWrapSound() => BaseScreenHandler.PlayWrapSound();

		internal static void PlayPaintSound(int dragCount) {
			try {
				var pos = SoundListenerController.Instance.transform.GetPosition();
				string sound = GlobalAssets.GetSound("ScheduleMenu_Select");
				if (sound == null) return;
				var instance = SoundEvent.BeginOneShot(sound, pos);
				instance.setParameterByName("Drag_Count", dragCount);
				SoundEvent.EndOneShot(instance);
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayPaintSound failed: {ex.Message}");
			}
		}

		internal static void PlayPaintNoneSound() {
			try {
				var pos = SoundListenerController.Instance.transform.GetPosition();
				string sound = GlobalAssets.GetSound("ScheduleMenu_Select_none");
				if (sound == null) return;
				SoundEvent.EndOneShot(SoundEvent.BeginOneShot(sound, pos));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayPaintNoneSound failed: {ex.Message}");
			}
		}

		private static bool _shiftUpToggle;
		private static bool _shiftDownToggle;

		internal static void PlayShiftUpSound() {
			try {
				string name = _shiftUpToggle ? "ScheduleMenu_Shift_up_reset" : "ScheduleMenu_Shift_up";
				_shiftUpToggle = !_shiftUpToggle;
				var pos = SoundListenerController.Instance.transform.GetPosition();
				string sound = GlobalAssets.GetSound(name);
				if (sound == null) return;
				SoundEvent.EndOneShot(SoundEvent.BeginOneShot(sound, pos));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayShiftUpSound failed: {ex.Message}");
			}
		}

		internal static void PlayShiftDownSound() {
			try {
				string name = _shiftDownToggle ? "ScheduleMenu_Shift_down_reset" : "ScheduleMenu_Shift_down";
				_shiftDownToggle = !_shiftDownToggle;
				var pos = SoundListenerController.Instance.transform.GetPosition();
				string sound = GlobalAssets.GetSound(name);
				if (sound == null) return;
				SoundEvent.EndOneShot(SoundEvent.BeginOneShot(sound, pos));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayShiftDownSound failed: {ex.Message}");
			}
		}
	}
}
