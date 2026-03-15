using System.Collections.Generic;

namespace OniAccess.Handlers.Screens {
	public static class VideoDescriptions {
		private static readonly Dictionary<string, List<(double, string)>> Descriptions =
			new Dictionary<string, List<(double, string)>> {
				["Digging"] = new List<(double, string)> {
					(0, STRINGS.ONIACCESS.VIDEO.DESCRIPTIONS.DIGGING.INTRO),
					(5, STRINGS.ONIACCESS.VIDEO.DESCRIPTIONS.DIGGING.GAP),
					(14, STRINGS.ONIACCESS.VIDEO.DESCRIPTIONS.DIGGING.WALL),
					(17, STRINGS.ONIACCESS.VIDEO.DESCRIPTIONS.DIGGING.CLIMB),
					(24, STRINGS.ONIACCESS.VIDEO.DESCRIPTIONS.DIGGING.CEILING),
					(28, STRINGS.ONIACCESS.VIDEO.DESCRIPTIONS.DIGGING.BLOCKED),
					(34, STRINGS.ONIACCESS.VIDEO.DESCRIPTIONS.DIGGING.HUG),
				},
			};

		public static List<(double time, string text)> GetDescriptions(string clipName) {
			Descriptions.TryGetValue(clipName, out var list);
			return list;
		}
	}
}
