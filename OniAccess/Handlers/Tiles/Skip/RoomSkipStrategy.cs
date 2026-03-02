namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips until the room (cavity) changes. Two adjacent rooms of
	/// the same type are distinct CavityInfo instances, so the skip
	/// stops at every room boundary. Null cavity (no room) compares
	/// equal across cells via object.Equals.
	/// </summary>
	public class RoomSkipStrategy: ISkipStrategy {
		public object GetSignature(int cell) {
			return Game.Instance.roomProber.GetCavityForCell(cell);
		}
	}
}
