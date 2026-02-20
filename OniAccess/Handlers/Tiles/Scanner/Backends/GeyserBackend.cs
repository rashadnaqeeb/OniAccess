using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Handlers.Tiles.Scanner.Backends {
	/// <summary>
	/// Backend for Buildings > Geysers. Iterates Components.Geysers and
	/// Components.GeothermalVents. Each geyser is one instance.
	/// </summary>
	public class GeyserBackend : IScannerBackend {

		public IEnumerable<ScanEntry> Scan(int worldId) {
			foreach (var geyser in Components.Geysers.GetItems(worldId)) {
				var go = geyser.gameObject;
				yield return MakeEntry(go);
			}

			foreach (var vent in Components.GeothermalVents.GetItems(worldId)) {
				var go = vent.gameObject;
				yield return MakeEntry(go);
			}
		}

		public bool ValidateEntry(ScanEntry entry, int cursorCell) {
			var go = (GameObject)entry.BackendData;
			return go != null && !go.IsNullOrDestroyed();
		}

		public string FormatName(ScanEntry entry) {
			var go = (GameObject)entry.BackendData;
			return GetGeyserName(go) ?? entry.ItemName;
		}

		private ScanEntry MakeEntry(GameObject go) {
			string name = GetGeyserName(go) ?? go.name;
			return new ScanEntry {
				Cell = Grid.PosToCell(go.transform.GetPosition()),
				Backend = this,
				BackendData = go,
				Category = ScannerTaxonomy.Categories.Buildings,
				Subcategory = ScannerTaxonomy.Subcategories.Geysers,
				ItemName = name,
			};
		}

		private static string GetGeyserName(GameObject go) {
			var userNameable = go.GetComponent<UserNameable>();
			if (userNameable != null && !string.IsNullOrEmpty(userNameable.savedName))
				return userNameable.savedName;
			return go.GetComponent<KSelectable>()?.GetName();
		}
	}
}
