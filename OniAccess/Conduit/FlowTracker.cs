namespace OniAccess.ConduitTracking {
	/// <summary>
	/// Tracks per-conduit flow direction in a circular buffer.
	/// One instance per conduit type (gas, liquid, solid).
	/// Records are sampled once per game-second from Sim200ms postfixes.
	/// </summary>
	public class FlowTracker {
		public const int BufferSize = 20;

		/// <summary>
		/// Normalized direction values shared across ConduitFlow.FlowDirections
		/// (flags byte) and SolidConduitFlow.FlowDirection (plain enum).
		/// </summary>
		public const int DirNone = 0;
		public const int DirUp = 1;
		public const int DirDown = 2;
		public const int DirLeft = 3;
		public const int DirRight = 4;

		private int[] _buffer;
		private int _writePos;
		private int _conduitCount;
		private int _samplesRecorded;

		public static FlowTracker Gas { get; private set; }
		public static FlowTracker Liquid { get; private set; }
		public static FlowTracker Solid { get; private set; }

		public static void Initialize() {
			Gas = new FlowTracker();
			Liquid = new FlowTracker();
			Solid = new FlowTracker();
		}

		public void Clear() {
			_buffer = null;
			_writePos = 0;
			_conduitCount = 0;
			_samplesRecorded = 0;
		}

		public void RecordFluid(ConduitFlow flow) {
			int count = flow.soaInfo.NumEntries;
			if (count == 0) return;
			EnsureCapacity(count);
			int baseIdx = _writePos * _conduitCount;
			for (int i = 0; i < count; i++) {
				var info = flow.soaInfo.GetLastFlowInfo(i);
				_buffer[baseIdx + i] = NormalizeFluidDirection(info.direction);
			}
			AdvanceWritePos();
		}

		public void RecordSolid(SolidConduitFlow flow) {
			var soa = flow.GetSOAInfo();
			int count = soa.NumEntries;
			if (count == 0) return;
			EnsureCapacity(count);
			int baseIdx = _writePos * _conduitCount;
			for (int i = 0; i < count; i++) {
				var info = soa.GetLastFlowInfo(i);
				_buffer[baseIdx + i] = NormalizeSolidDirection(info.direction);
			}
			AdvanceWritePos();
		}

		/// <summary>
		/// Returns direction percentages for the given conduit index.
		/// Counts are filled into the provided array indexed by DirNone..DirRight.
		/// Returns the number of samples used as the denominator.
		/// </summary>
		public int GetDirectionCounts(int conduitIdx, int[] counts) {
			counts[DirNone] = 0;
			counts[DirUp] = 0;
			counts[DirDown] = 0;
			counts[DirLeft] = 0;
			counts[DirRight] = 0;

			if (_buffer == null || conduitIdx < 0 || conduitIdx >= _conduitCount)
				return 0;

			int samples = _samplesRecorded < BufferSize
				? _samplesRecorded : BufferSize;
			if (samples == 0) return 0;

			int startSlot = _samplesRecorded < BufferSize
				? 0 : _writePos;
			for (int i = 0; i < samples; i++) {
				int slot = (startSlot + i) % BufferSize;
				int dir = _buffer[slot * _conduitCount + conduitIdx];
				counts[dir]++;
			}
			return samples;
		}

		private void EnsureCapacity(int conduitCount) {
			if (_conduitCount == conduitCount && _buffer != null) return;
			_buffer = new int[conduitCount * BufferSize];
			_conduitCount = conduitCount;
			_writePos = 0;
			_samplesRecorded = 0;
		}

		private void AdvanceWritePos() {
			_writePos = (_writePos + 1) % BufferSize;
			if (_samplesRecorded < BufferSize)
				_samplesRecorded++;
		}

		private static int NormalizeFluidDirection(ConduitFlow.FlowDirections dir) {
			switch (dir) {
				case ConduitFlow.FlowDirections.Up: return DirUp;
				case ConduitFlow.FlowDirections.Down: return DirDown;
				case ConduitFlow.FlowDirections.Left: return DirLeft;
				case ConduitFlow.FlowDirections.Right: return DirRight;
				default: return DirNone;
			}
		}

		private static int NormalizeSolidDirection(SolidConduitFlow.FlowDirection dir) {
			switch (dir) {
				case SolidConduitFlow.FlowDirection.Up: return DirUp;
				case SolidConduitFlow.FlowDirection.Down: return DirDown;
				case SolidConduitFlow.FlowDirection.Left: return DirLeft;
				case SolidConduitFlow.FlowDirection.Right: return DirRight;
				default: return DirNone;
			}
		}
	}
}
