using System.Collections.Generic;

namespace OniAccess.ConduitTracking {
	/// <summary>
	/// Captures flow data from ConduitBridge transfers so bridge input
	/// cells can report element and direction in FlowTracker. Bridge
	/// callbacks run inside Sim200ms before our postfix samples.
	/// Also maintains a persistent input→output cell mapping for the
	/// flow sonifier.
	/// </summary>
	public static class BridgeFlowCapture {
		private struct Transfer {
			public SimHashes Element;
			public int Direction;
		}

		private struct BridgeEndpoints {
			public int OutputCell;
			public ConduitType Type;
		}

		private static readonly Dictionary<int, Transfer> _transfers
			= new Dictionary<int, Transfer>();

		private static readonly Dictionary<int, BridgeEndpoints> _endpoints
			= new Dictionary<int, BridgeEndpoints>();

		public static void Record(int inputCell, int outputCell,
				ConduitType type, SimHashes element, int direction) {
			_transfers[inputCell] = new Transfer {
				Element = element,
				Direction = direction
			};
			_endpoints[inputCell] = new BridgeEndpoints {
				OutputCell = outputCell,
				Type = type
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

		public static bool TryGetOutputCell(int inputCell,
				out int outputCell, out ConduitType type) {
			if (_endpoints.TryGetValue(inputCell, out BridgeEndpoints ep)) {
				outputCell = ep.OutputCell;
				type = ep.Type;
				return true;
			}
			outputCell = 0;
			type = ConduitType.None;
			return false;
		}

		public static void Clear() {
			_transfers.Clear();
		}
	}
}
