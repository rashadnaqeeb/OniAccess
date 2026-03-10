namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Invisible cross-shaped alignment guide. One ruler at a time,
	/// extending infinitely along the row and column of the placement cell.
	/// Plays proximity audio as the cursor approaches, and acts as a
	/// hard stop boundary for cursor skip.
	/// </summary>
	public class CursorRuler {
		public static CursorRuler Instance { get; private set; }

		private int _rulerCell = Grid.InvalidCell;
		private int _lastSoundCell = Grid.InvalidCell;

		private enum ProximityZone { None, Near, OnLine, Intersection }

		public static CursorRuler Create() {
			Instance = new CursorRuler();
			return Instance;
		}

		public static void Destroy() {
			Instance = null;
		}

		public string PlaceAt(int cell) {
			_rulerCell = cell;
			_lastSoundCell = Grid.InvalidCell;
			return (string)STRINGS.ONIACCESS.RULER.PLACED;
		}

		public string Clear() {
			_rulerCell = Grid.InvalidCell;
			_lastSoundCell = Grid.InvalidCell;
			return (string)STRINGS.ONIACCESS.RULER.CLEARED;
		}

		public bool IsOnRulerLine(int cell) {
			if (_rulerCell == Grid.InvalidCell) return false;
			return Grid.CellRow(cell) == Grid.CellRow(_rulerCell)
				|| Grid.CellColumn(cell) == Grid.CellColumn(_rulerCell);
		}

		public void OnCursorMoved(int cursorCell) {
			if (_rulerCell == Grid.InvalidCell) return;
			if (cursorCell == _lastSoundCell) return;
			_lastSoundCell = cursorCell;

			switch (ClassifyZone(cursorCell)) {
				case ProximityZone.Intersection:
					PlayClickSound();
					break;
				case ProximityZone.OnLine:
					PlayPluckSound(12f);
					break;
				case ProximityZone.Near:
					PlayPluckSound(4f);
					break;
			}
		}

		private ProximityZone ClassifyZone(int cell) {
			int rulerRow = Grid.CellRow(_rulerCell);
			int rulerCol = Grid.CellColumn(_rulerCell);
			int curRow = Grid.CellRow(cell);
			int curCol = Grid.CellColumn(cell);

			bool onRow = curRow == rulerRow;
			bool onCol = curCol == rulerCol;
			if (onRow && onCol) return ProximityZone.Intersection;
			if (onRow || onCol) return ProximityZone.OnLine;

			int rowDist = System.Math.Abs(curRow - rulerRow);
			int colDist = System.Math.Abs(curCol - rulerCol);
			if (rowDist <= 1 || colDist <= 1) return ProximityZone.Near;
			return ProximityZone.None;
		}

		private static void PlayClickSound() {
			try {
				string sound = GlobalAssets.GetSound("HUD_Click");
				if (sound == null) return;
				KFMOD.PlayUISound(sound);
			} catch (System.Exception ex) {
				Util.Log.Warn($"CursorRuler.PlayClickSound: {ex.Message}");
			}
		}

		private static void PlayPluckSound(float dragCount) {
			try {
				var pos = SoundListenerController.Instance.transform.GetPosition();
				string sound = GlobalAssets.GetSound("ScheduleMenu_Select");
				if (sound == null) return;
				var instance = SoundEvent.BeginOneShot(sound, pos);
				instance.setParameterByName("Drag_Count", dragCount);
				SoundEvent.EndOneShot(instance);
			} catch (System.Exception ex) {
				Util.Log.Warn($"CursorRuler.PlayPluckSound: {ex.Message}");
			}
		}
	}
}
