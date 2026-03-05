using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.AreaScan {
	public class RoomsAreaScanner : IAreaScanner {
		public string Scan(int[] cells, int totalCells, int unexploredCount) {
			try {
				var tokens = new List<string>();
				AreaScanUtil.AddUnexploredToken(tokens, totalCells, unexploredCount);

				var roomNames = new Dictionary<string, int>();
				int uncategorized = 0;
				var seenCavities = new HashSet<CavityInfo>();

				for (int i = 0; i < cells.Length; i++) {
					var cavity = Game.Instance.roomProber.GetCavityForCell(cells[i]);
					if (cavity == null) continue;
					if (!seenCavities.Add(cavity)) continue;

					if (cavity.room == null
						|| cavity.room.roomType == Db.Get().RoomTypes.Neutral) {
						uncategorized++;
					} else {
						string name = cavity.room.roomType.Name;
						if (roomNames.ContainsKey(name))
							roomNames[name]++;
						else
							roomNames[name] = 1;
					}
				}

				var parts = new List<string>();
				foreach (var kv in roomNames) {
					if (kv.Value > 1)
						parts.Add($"{kv.Value} {kv.Key}");
					else
						parts.Add(kv.Key);
				}
				if (uncategorized > 0)
					parts.Add(string.Format(
						STRINGS.ONIACCESS.BIG_CURSOR.UNCATEGORIZED_ROOMS,
						uncategorized));

				if (parts.Count == 0) {
					tokens.Add((string)STRINGS.ONIACCESS.BIG_CURSOR.NO_ROOMS);
				} else {
					tokens.Add(string.Join(", ", parts));
				}

				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"RoomsAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}
	}
}
