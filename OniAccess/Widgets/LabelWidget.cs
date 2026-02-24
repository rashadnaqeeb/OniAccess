namespace OniAccess.Widgets {
	/// <summary>
	/// A read-only label widget (LocText, status info, section headers).
	/// Always valid as long as it exists.
	/// </summary>
	public class LabelWidget : Widget {
		public override bool IsValid() {
			if (GameObject != null && !GameObject.activeInHierarchy) return false;
			return true;
		}
	}
}
