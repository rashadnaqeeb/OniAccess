namespace OniAccess.Handlers.Screens.ClusterMap {
	/// <summary>
	/// Hex distance and compass direction formatting.
	/// Uses atan2 on world-space positions bucketed into 8 compass directions.
	/// </summary>
	public static class HexCoordinates {
		private static readonly LocString[] CompassStrings = {
			STRINGS.ONIACCESS.CLUSTER_MAP.COMPASS.NORTH,
			STRINGS.ONIACCESS.CLUSTER_MAP.COMPASS.NORTHEAST,
			STRINGS.ONIACCESS.CLUSTER_MAP.COMPASS.EAST,
			STRINGS.ONIACCESS.CLUSTER_MAP.COMPASS.SOUTHEAST,
			STRINGS.ONIACCESS.CLUSTER_MAP.COMPASS.SOUTH,
			STRINGS.ONIACCESS.CLUSTER_MAP.COMPASS.SOUTHWEST,
			STRINGS.ONIACCESS.CLUSTER_MAP.COMPASS.WEST,
			STRINGS.ONIACCESS.CLUSTER_MAP.COMPASS.NORTHWEST,
		};

		/// <summary>
		/// Format hex distance and compass direction from origin to target.
		/// Returns "center" if same hex. Format: "5 northeast".
		/// </summary>
		public static string Format(AxialI origin, AxialI target) {
			int distance = AxialUtil.GetDistance(origin, target);
			if (distance == 0)
				return (string)STRINGS.ONIACCESS.CLUSTER_MAP.AT_CENTER;
			string direction = GetCompassDirection(origin, target);
			return string.Format(
				(string)STRINGS.ONIACCESS.CLUSTER_MAP.HEX_COORDINATES,
				distance, direction);
		}

		/// <summary>
		/// Returns one of 8 compass direction strings based on atan2
		/// of the world-space vector from origin to target.
		/// </summary>
		public static string GetCompassDirection(AxialI origin, AxialI target) {
			var fromWorld = AxialUtil.AxialToWorld(origin.r, origin.q);
			var toWorld = AxialUtil.AxialToWorld(target.r, target.q);
			float dx = toWorld.x - fromWorld.x;
			float dy = toWorld.y - fromWorld.y;
			// atan2 returns radians, convert to degrees [0, 360)
			float angle = UnityEngine.Mathf.Atan2(dy, dx) * UnityEngine.Mathf.Rad2Deg;
			if (angle < 0f) angle += 360f;
			// Bucket into 8 directions (each 45 degrees, offset by 22.5)
			// 0=East(67.5-112.5), but we want North=0. Rotate: North is +Y = 90 degrees
			// Mapping: 0=N(67.5..112.5), 1=NE(22.5..67.5), 2=E(-22.5..22.5), etc.
			// Simpler: shift angle so North is at 0
			float shifted = (450f - angle) % 360f;
			int index = (int)((shifted + 22.5f) / 45f) % 8;
			return (string)CompassStrings[index];
		}
	}
}
