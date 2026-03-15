using System.Collections.Generic;

using D = STRINGS.ONIACCESS.VIDEO.DESCRIPTIONS;

namespace OniAccess.Handlers.Screens {
	public static class VideoDescriptions {
		private static readonly Dictionary<string, List<(double, string)>> Descriptions =
			new Dictionary<string, List<(double, string)>> {
				["Artifact"] = new List<(double, string)> {
					(0, D.ARTIFACT.RESTORE),
					(4, D.ARTIFACT.DISPLAY),
					(8, D.ARTIFACT.CHECKLIST),
					(12, D.ARTIFACT.CELEBRATE),
				},
				["Artifact_loop"] = new List<(double, string)> {
					(0, D.ARTIFACT_LOOP.DISPLAY),
					(5, D.ARTIFACT_LOOP.DETAILS),
				},
				["Digging"] = new List<(double, string)> {
					(0, D.DIGGING.GREETING),
					(7, D.DIGGING.MINING),
					(13, D.DIGGING.BUILDING),
					(21, D.DIGGING.DIRT),
					(30, D.DIGGING.RECKLESS),
					(35, D.DIGGING.FLOOD),
				},
				["Geothermal"] = new List<(double, string)> {
					(0, D.GEOTHERMAL.ARRIVAL),
					(3, D.GEOTHERMAL.POD_ACTIVATES),
					(6, D.GEOTHERMAL.WAVE),
				},
				["Insulation"] = new List<(double, string)> {
					(0, D.INSULATION.PEACEFUL),
					(4, D.INSULATION.WILT),
					(13, D.INSULATION.OVERLAY),
					(21, D.INSULATION.ICE_FAN),
					(29, D.INSULATION.HEAT_RETURNS),
					(37, D.INSULATION.IDEA),
					(45, D.INSULATION.INSULATED_WALLS),
					(51, D.INSULATION.COOLING),
					(57, D.INSULATION.HEAT_BLOCKED),
					(62, D.INSULATION.FREEZE),
				},
				["LargeImpactorDefeatedVideo"] = new List<(double, string)> {
					(0, D.LARGE_IMPACTOR_DEFEATED.ASTEROID),
					(1, D.LARGE_IMPACTOR_DEFEATED.ROCKETS),
					(3, D.LARGE_IMPACTOR_DEFEATED.EXPLOSION),
					(5, D.LARGE_IMPACTOR_DEFEATED.FRAGMENTS),
					(7, D.LARGE_IMPACTOR_DEFEATED.WATCHING),
					(8, D.LARGE_IMPACTOR_DEFEATED.CELEBRATION),
				},
				["LargeImpactorSpacePOIVideo"] = new List<(double, string)> {
					(0, D.LARGE_IMPACTOR_SPACE_POI.SPACE),
					(6, D.LARGE_IMPACTOR_SPACE_POI.BIOMES),
					(13, D.LARGE_IMPACTOR_SPACE_POI.ROTATION),
				},
				["Leave"] = new List<(double, string)> {
					(0, D.LEAVE.LAUNCH),
					(3, D.LEAVE.MISSION_CONTROL),
					(7, D.LEAVE.PLUNGE),
				},
				["Leave_loop"] = new List<(double, string)> {
					(0, D.LEAVE_LOOP.VORTEX),
					(4, D.LEAVE_LOOP.ASTEROIDS),
				},
				["Locomotion"] = new List<(double, string)> {
					(0, D.LOCOMOTION.WAVE),
					(4, D.LOCOMOTION.GAP_BLOCKED),
					(9, D.LOCOMOTION.GAP_JUMP),
					(13, D.LOCOMOTION.WALL_BLOCKED),
					(17, D.LOCOMOTION.WALL_CLIMB),
					(21, D.LOCOMOTION.SHINE_BUG),
					(24, D.LOCOMOTION.CORRIDOR),
					(28, D.LOCOMOTION.COLLISION),
					(31, D.LOCOMOTION.STUCK),
					(36, D.LOCOMOTION.REUNION),
				},
				["Morale"] = new List<(double, string)> {
					(0, D.MORALE.INTRO),
					(10, D.MORALE.SKILL),
					(18, D.MORALE.MISERABLE),
					(30, D.MORALE.FOOD),
					(40, D.MORALE.SECOND_SKILL),
					(48, D.MORALE.STRESS),
					(54, D.MORALE.ARCADE),
					(67, D.MORALE.BED),
				},
				["Piping"] = new List<(double, string)> {
					(0, D.PIPING.MISERABLE),
					(6, D.PIPING.OVERVIEW),
					(10, D.PIPING.OVERLAY),
					(16, D.PIPING.PIPE_BUILT),
					(26, D.PIPING.SPLASH),
					(30, D.PIPING.LESSON),
				},
				["Power"] = new List<(double, string)> {
					(0, D.POWER.INTRO),
					(4, D.POWER.BATTERY),
					(8, D.POWER.OVERLOAD),
					(13, D.POWER.OVERLAY),
					(19, D.POWER.IDEA),
					(22, D.POWER.REBUILD),
					(26, D.POWER.RESULT),
				},
				["Spaced_Out_Intro"] = new List<(double, string)> {
					(0, D.SPACED_OUT_INTRO.COLONY),
					(4, D.SPACED_OUT_INTRO.ROCKET),
					(8, D.SPACED_OUT_INTRO.CELEBRATION),
					(13, D.SPACED_OUT_INTRO.GOODBYE),
					(20, D.SPACED_OUT_INTRO.DUPLICATION),
					(25, D.SPACED_OUT_INTRO.CONFUSION),
					(32, D.SPACED_OUT_INTRO.PORTALS),
					(42, D.SPACED_OUT_INTRO.GRID),
					(51, D.SPACED_OUT_INTRO.SPLIT),
					(63, D.SPACED_OUT_INTRO.CRASH),
					(72, D.SPACED_OUT_INTRO.FLASH),
					(78, D.SPACED_OUT_INTRO.CAVERN),
					(89, D.SPACED_OUT_INTRO.SURFACE),
					(93, D.SPACED_OUT_INTRO.PEEK),
					(103, D.SPACED_OUT_INTRO.STRANDED),
				},
				["Stay"] = new List<(double, string)> {
					(0, D.STAY.CHEER),
					(2, D.STAY.WIDER),
					(5, D.STAY.PARTY),
				},
				["Stay_loop"] = new List<(double, string)> {
					(0, D.STAY_LOOP.MESS),
					(5, D.STAY_LOOP.LADDER),
				},
				["Geothermal_loop"] = new List<(double, string)> {
					(0, D.GEOTHERMAL_LOOP.DUPES),
					(4, D.GEOTHERMAL_LOOP.FILLING),
					(8, D.GEOTHERMAL_LOOP.CYCLING),
				},
			};

		public static List<(double time, string text)> GetDescriptions(string clipName) {
			Descriptions.TryGetValue(clipName, out var list);
			return list;
		}
	}
}
