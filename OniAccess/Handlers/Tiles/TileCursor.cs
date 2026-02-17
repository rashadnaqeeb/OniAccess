using UnityEngine;

namespace OniAccess.Handlers.Tiles {
	public enum Direction { Up, Down, Left, Right }

	public enum CoordinateMode { Off, Append, Prepend }

	/// <summary>
	/// Owns a cell index for tile-by-tile world navigation.
	/// Arrow key movement, world bounds clamping, KInputManager mouse lock,
	/// camera follow, coordinate reading, and proof-of-life element speech.
	/// </summary>
	public class TileCursor {
		private int _cell;
		private bool _wasPanning;

		public CoordinateMode Mode { get; private set; } = ConfigManager.Config.CoordinateMode;

		public int Cell => _cell;

		/// <summary>
		/// Initialize cursor at the Printing Pod on the active world.
		/// Falls back to world center if no telepad exists.
		/// </summary>
		public void Initialize() {
			var world = ClusterManager.Instance.activeWorld;
			int cell = FindTelepadCell(world);
			if (cell == Grid.InvalidCell)
				cell = FindWorldCenter(world);
			_cell = cell;
			LockMouseToCell(_cell);
			SnapCameraToCell(_cell);
		}

		/// <summary>
		/// Move one cell in the given direction. Returns the speech string
		/// for the new cell, or null if the move was blocked by world bounds.
		/// </summary>
		public string Move(Direction direction) {
			int candidate = GetNeighbor(_cell, direction);
			if (candidate == Grid.InvalidCell || !IsInWorldBounds(candidate)) {
				PlayBoundarySound();
				return null;
			}
			_cell = candidate;
			LockMouseToCell(_cell);
			SnapCameraToCell(_cell);
			return BuildCellSpeech();
		}

		/// <summary>
		/// Return coordinates for the current cell, relative to the
		/// active world's origin (so bottom-left of the world is 0, 0).
		/// </summary>
		public string ReadCoordinates() {
			var world = ClusterManager.Instance.activeWorld;
			int x = Grid.CellColumn(_cell) - (int)world.minimumBounds.x;
			int y = Grid.CellRow(_cell) - (int)world.minimumBounds.y;
			return string.Format((string)STRINGS.ONIACCESS.TILE_CURSOR.COORDS, x, y);
		}

		/// <summary>
		/// Cycle Off -> Append -> Prepend -> Off.
		/// Returns the spoken name of the new mode.
		/// </summary>
		public string CycleMode() {
			string spoken;
			switch (Mode) {
				case CoordinateMode.Off:
					Mode = CoordinateMode.Append;
					spoken = (string)STRINGS.ONIACCESS.TILE_CURSOR.COORD_APPEND;
					break;
				case CoordinateMode.Append:
					Mode = CoordinateMode.Prepend;
					spoken = (string)STRINGS.ONIACCESS.TILE_CURSOR.COORD_PREPEND;
					break;
				default:
					Mode = CoordinateMode.Off;
					spoken = (string)STRINGS.ONIACCESS.TILE_CURSOR.COORD_OFF;
					break;
			}
			ConfigManager.Config.CoordinateMode = Mode;
			ConfigManager.Save();
			return spoken;
		}

		/// <summary>
		/// Re-sync cursor to the camera's center cell. Called every frame
		/// so the cursor follows game-initiated camera movement (alerts,
		/// follow-cam, etc.) and the mouse lock stays correct.
		/// Returns tile speech when the camera finishes a pan, null otherwise.
		/// </summary>
		public string SyncToCamera() {
			if (Camera.main == null) return null;
			Vector3 center = Camera.main.transform.position;
			int cell = Grid.PosToCell(center);
			if (Grid.IsValidCell(cell) && cell != _cell && IsInWorldBounds(cell))
				_cell = cell;
			LockMouseToCell(_cell);

			bool panning = CameraController.Instance != null
				&& CameraController.Instance.isTargetPosSet;
			if (_wasPanning && !panning) {
				_wasPanning = false;
				return BuildCellSpeech();
			}
			_wasPanning = panning;
			return null;
		}

		// ========================================
		// PRIVATE
		// ========================================

		private string BuildCellSpeech() {
			if (!Grid.IsVisible(_cell))
				return AttachCoordinates((string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED);

			string content = Grid.Element[_cell].name;
			return AttachCoordinates(content);
		}

		private int FindTelepadCell(WorldContainer world) {
			try {
				var telepads = Components.Telepads.GetWorldItems(world.id);
				if (telepads != null && telepads.Count > 0)
					return Grid.PosToCell(telepads[0].transform.GetPosition());
			} catch (System.Exception ex) {
				Util.Log.Warn($"TileCursor.FindTelepadCell: {ex.Message}");
			}
			return Grid.InvalidCell;
		}

		private int FindWorldCenter(WorldContainer world) {
			int x = (int)((world.minimumBounds.x + world.maximumBounds.x) / 2f);
			int y = (int)((world.minimumBounds.y + world.maximumBounds.y) / 2f);
			int cell = Grid.XYToCell(x, y);
			if (Grid.IsValidCell(cell))
				return cell;
			Util.Log.Warn("TileCursor.FindWorldCenter: center cell invalid, using cell 0");
			return 0;
		}

		private static int GetNeighbor(int cell, Direction direction) {
			switch (direction) {
				case Direction.Up: return Grid.CellAbove(cell);
				case Direction.Down: return Grid.CellBelow(cell);
				case Direction.Left: return Grid.CellLeft(cell);
				case Direction.Right: return Grid.CellRight(cell);
				default: return Grid.InvalidCell;
			}
		}

		private bool IsInWorldBounds(int cell) {
			if (!Grid.IsValidCell(cell))
				return false;
			var world = ClusterManager.Instance.activeWorld;
			if (Grid.WorldIdx[cell] != world.id)
				return false;
			int x = Grid.CellColumn(cell);
			int y = Grid.CellRow(cell);
			return x >= (int)world.minimumBounds.x
				&& x <= (int)world.maximumBounds.x
				&& y >= (int)world.minimumBounds.y
				&& y <= (int)world.maximumBounds.y;
		}

		private static void LockMouseToCell(int cell) {
			if (Camera.main == null) {
				Util.Log.Warn("TileCursor.LockMouseToCell: Camera.main is null");
				return;
			}
			Vector3 worldPos = Grid.CellToPosCCC(cell, Grid.SceneLayer.Move);
			Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
			KInputManager.isMousePosLocked = true;
			KInputManager.lockedMousePos = screenPos;
		}

		private static void SnapCameraToCell(int cell) {
			if (CameraController.Instance == null) {
				Util.Log.Warn("TileCursor.SnapCameraToCell: CameraController.Instance is null");
				return;
			}
			Vector3 worldPos = Grid.CellToPosCCC(cell, Grid.SceneLayer.Move);
			CameraController.Instance.SnapTo(worldPos);
		}

		private string AttachCoordinates(string content) {
			switch (Mode) {
				case CoordinateMode.Append:
					return content + ", " + ReadCoordinates();
				case CoordinateMode.Prepend:
					return ReadCoordinates() + ", " + content;
				default:
					return content;
			}
		}

		private static void PlayBoundarySound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Negative"));
			} catch (System.Exception ex) {
				Util.Log.Error($"TileCursor.PlayBoundarySound: {ex.Message}");
			}
		}
	}
}
