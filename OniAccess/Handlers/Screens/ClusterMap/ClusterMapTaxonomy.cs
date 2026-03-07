namespace OniAccess.Handlers.Screens.ClusterMap {
	public static class ClusterMapTaxonomy {
		public static class Categories {
			public const string All = "All";
			public const string Asteroids = "Asteroids";
			public const string Rockets = "Rockets";
			public const string POIs = "POIs";
			public const string Meteors = "Meteors";
			public const string Unknown = "Unknown";
		}

		private static readonly string[] _categoryOrder = {
			Categories.All,
			Categories.Asteroids,
			Categories.Rockets,
			Categories.POIs,
			Categories.Meteors,
			Categories.Unknown,
		};

		public static int CategorySortIndex(string name) {
			for (int i = 0; i < _categoryOrder.Length; i++) {
				if (_categoryOrder[i] == name) return i;
			}
			return _categoryOrder.Length;
		}
	}
}
