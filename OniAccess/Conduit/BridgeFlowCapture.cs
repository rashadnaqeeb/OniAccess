using System.Collections.Generic;

namespace OniAccess.ConduitTracking {
	/// <summary>
	/// Captures flow data from ConduitBridge transfers so bridge input
	/// cells can report element and direction in FlowTracker. Bridge
	/// callbacks run inside Sim200ms before our postfix samples.
	/// </summary>
	public static class BridgeFlowCapture {
		private struct Transfer {
			public SimHashes Element;
			public int Direction;
		}

		private static readonly Dictionary<int, Transfer> _transfers
			= new Dictionary<int, Transfer>();

		public static void Record(int cell, SimHashes element,
				int direction) {
			_transfers[cell] = new Transfer {
				Element = element,
				Direction = direction
			};
		}

		public static bool TryGet(int cell, out SimHashes element,
				out int direction) {
			if (_transfers.TryGetValue(cell, out Transfer t)) {
				element = t.Element;
				direction = t.Direction;
				return true;
			}
			element = SimHashes.Vacuum;
			direction = FlowTracker.DirNone;
			return false;
		}

		public static void Clear() {
			_transfers.Clear();
		}
	}
}
