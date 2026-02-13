namespace OniAccess.Input {
	/// <summary>
	/// Simple data class for help list entries.
	/// Each handler provides its own list of these via IAccessHandler.HelpEntries.
	/// Displayed in the F12 navigable help list.
	/// </summary>
	public sealed class HelpEntry {
		public string KeyName { get; }
		public string Description { get; }

		public HelpEntry(string keyName, string description) {
			KeyName = keyName;
			Description = description;
		}

		public override string ToString() => $"{KeyName}: {Description}";
	}
}
