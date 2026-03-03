namespace OniAccess.Widgets {
	public class TreeItemWidget : Widget {
		public KTreeItem TreeItem { get; set; }

		public override bool Activate() {
			TreeItem.checkboxChecked = !TreeItem.checkboxChecked;
			TreeItem.ToggleChecked();
			return true;
		}

		public override bool IsValid() {
			if (GameObject != null && !GameObject.activeInHierarchy) return false;
			return TreeItem != null;
		}

		public override string GetSpeechText() {
			if (SpeechFunc != null) {
				string result = SpeechFunc()?.Trim();
				if (!string.IsNullOrEmpty(result)) return result;
			}
			return Label;
		}
	}
}
