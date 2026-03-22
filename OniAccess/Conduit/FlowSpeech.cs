using System.Collections.Generic;

namespace OniAccess.ConduitTracking {
	/// <summary>
	/// Formats a FlowTracker's direction counts as a speech string.
	/// </summary>
	public static class FlowSpeech {
		public static string Format(FlowTracker tracker, int conduitIdx) {
			var counts = new int[5];
			int samples = tracker.GetDirectionCounts(conduitIdx, counts);
			if (samples == 0) return null;

			bool hasFlow = false;
			for (int d = FlowTracker.DirUp; d <= FlowTracker.DirRight; d++) {
				if (counts[d] > 0) { hasFlow = true; break; }
			}
			if (!hasFlow)
				return STRINGS.ONIACCESS.GLANCE.FLOW_NOT_FLOWING;

			var parts = new List<DirectionPercent>(4);
			for (int d = FlowTracker.DirUp; d <= FlowTracker.DirRight; d++) {
				if (counts[d] > 0)
					parts.Add(new DirectionPercent(d,
						counts[d] * 100 / samples));
			}
			parts.Sort((a, b) => b.Percent - a.Percent);

			var tokens = new List<string>(parts.Count);
			foreach (var p in parts)
				tokens.Add(string.Format(
					STRINGS.ONIACCESS.GLANCE.FLOW_DIRECTION_PERCENT,
					p.Percent, DirectionName(p.Dir)));
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
	}
}
