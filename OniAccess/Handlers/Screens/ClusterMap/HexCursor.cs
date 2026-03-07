namespace OniAccess.Handlers.Screens.ClusterMap {
	/// <summary>
	/// Stateless hex cursor movement utility. Maps 6-direction keys and
	/// 4-direction arrow keys to hex neighbor movement with bounds checking.
	/// </summary>
	public static class HexCursor {
		/// <summary>
		/// Try to move from current position in the given hex direction.
		/// Returns true if the destination is a valid cell.
		/// </summary>
		public static bool TryMove(AxialI from, AxialI direction, out AxialI result) {
			result = from + direction;
			return ClusterGrid.Instance.IsValidCell(result);
		}

		/// <summary>
		/// Map an arrow key direction to a hex direction, using row parity
		/// for Up/Down to alternate between NE/NW and SE/SW.
		/// Left=West, Right=East always.
		///
		/// Pointy-top hex layout (ONI's cluster map):
		///   Even q: Up=NE, Down=SE (first step shifts right)
		///   Odd q:  Up=NW, Down=SW (first step shifts left)
		/// This makes Up zigzag NE/NW and Down retrace the same hexes.
		/// </summary>
		public static AxialI ArrowToHexDirection(AxialI current, Direction arrowDir) {
			bool evenQ = (current.q & 1) == 0;
			switch (arrowDir) {
				case Direction.Left:
					return AxialI.WEST;
				case Direction.Right:
					return AxialI.EAST;
				case Direction.Up:
					return evenQ ? AxialI.NORTHEAST : AxialI.NORTHWEST;
				case Direction.Down:
					return evenQ ? AxialI.SOUTHEAST : AxialI.SOUTHWEST;
				default:
					return AxialI.ZERO;
			}
		}

		public enum Direction { Up, Down, Left, Right }
	}
}
