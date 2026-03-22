using System.Collections.Generic;

namespace OniAccess.ConduitTracking {
	/// <summary>
	/// Formats a FlowTracker's direction counts as a speech string.
	/// </summary>
	public static class FlowSpeech {
		public static string Format(FlowTracker tracker, int conduitIdx,
				bool currentlyEmpty) {
			var elementCounts = new Dictionary<SimHashes, int[]>();
			int samples = tracker.GetElementDirectionCounts(
				conduitIdx, elementCounts);
			if (samples == 0) return null;

			if (elementCounts.Count == 0)
				return currentlyEmpty
					? STRINGS.ONIACCESS.GLANCE.FLOW_EMPTY
					: STRINGS.ONIACCESS.GLANCE.FLOW_NOT_FLOWING;

			var elements = new List<ElementGroup>(elementCounts.Count);
			foreach (var kvp in elementCounts) {
				int total = 0;
				var dirs = new List<DirectionPercent>(4);
				for (int d = FlowTracker.DirUp;
						d <= FlowTracker.DirRight; d++) {
					if (kvp.Value[d] > 0) {
						int pct = kvp.Value[d] * 100 / samples;
						dirs.Add(new DirectionPercent(d, pct));
						total += kvp.Value[d];
					}
				}
				dirs.Sort((a, b) => b.Percent - a.Percent);
				var element = ElementLoader.FindElementByHash(kvp.Key);
				string name = element != null ? element.name
					: kvp.Key.ToString();
				elements.Add(new ElementGroup(name, total, dirs));
			}
			elements.Sort((a, b) => b.Total - a.Total);

			var tokens = new List<string>(elements.Count);
			foreach (var eg in elements) {
				var dirTokens = new List<string>(eg.Dirs.Count);
				foreach (var d in eg.Dirs)
					dirTokens.Add(string.Format(
						STRINGS.ONIACCESS.GLANCE.FLOW_DIRECTION_PERCENT,
						d.Percent, DirectionName(d.Dir)));
				tokens.Add(string.Format(
					STRINGS.ONIACCESS.GLANCE.FLOW_ELEMENT_DIRECTIONS,
					eg.Name, string.Join(" ", dirTokens)));
			}
			return string.Join(", ", tokens);
		}

		private static string DirectionName(int dir) {
			switch (dir) {
				case FlowTracker.DirUp:
					return STRINGS.ONIACCESS.SCANNER.DIRECTION_UP;
				case FlowTracker.DirDown:
					return STRINGS.ONIACCESS.SCANNER.DIRECTION_DOWN;
				case FlowTracker.DirLeft:
					return STRINGS.ONIACCESS.SCANNER.DIRECTION_LEFT;
				case FlowTracker.DirRight:
					return STRINGS.ONIACCESS.SCANNER.DIRECTION_RIGHT;
				default: return "";
			}
		}

		private struct DirectionPercent {
			public int Dir;
			public int Percent;
			public DirectionPercent(int dir, int percent) {
				Dir = dir;
				Percent = percent;
			}
		}

		private struct ElementGroup {
			public string Name;
			public int Total;
			public List<DirectionPercent> Dirs;
			public ElementGroup(string name, int total,
					List<DirectionPercent> dirs) {
				Name = name;
				Total = total;
				Dirs = dirs;
			}
		}
	}
}
