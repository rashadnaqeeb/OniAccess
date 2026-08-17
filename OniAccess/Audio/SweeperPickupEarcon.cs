using UnityEngine;

using OniAccess.Handlers.Tiles;
using OniAccess.Util;

namespace OniAccess.Audio {
	/// <summary>
	/// Sounds the moment an Auto-Sweeper under the tile cursor closes its claw
	/// on something.
	///
	/// The arm swings all cycle and none of that motion reaches the player, so
	/// a sweeper that has just taken an item reads exactly like one that has
	/// been idle for hours. The earcon marks the one moment that changes: from
	/// here the info key has something to report about what is in the claw.
	/// Only pickups fire it - deliveries and empty swings stay silent, so the
	/// sound always means the same thing.
	/// </summary>
	internal static class SweeperPickupEarcon {
		private const string ClipName = "auto-sweeper";

		/// <summary>
		/// Runs for every completed pickup in the colony, duplicants included,
		/// so the tests are ordered cheapest first: the config flag and the
		/// grid lookup reject all of them but an arm standing under the cursor
		/// before any component lookup happens.
		/// </summary>
		internal static void OnPickupComplete(GameObject workerGo) {
			if (!ModToggle.IsEnabled) return;
			if (!ConfigManager.Config.SweeperPickupEarcons) return;
			if (!LoadGate.IsReady) return;

			int cell = TileCursor.Instance?.Cell ?? Grid.InvalidCell;
			if (!Grid.IsValidCell(cell)) return;

			// A sweeper is 3x1 and claims every cell it covers on the building
			// layer, so this matches wherever along the arm the cursor sits.
			if (Grid.Objects[cell, (int)ObjectLayer.Building] != workerGo) return;
			if (workerGo.GetComponent<SolidTransferArm>() == null) return;

			// The caller is a prefix on the game's own pickup completion, so a
			// throw from here would take the pickup down with it.
			try {
				EarconScheduler.Instance.PlayOneShot(
					ClipName, ConfigManager.Config.SweeperPickupVolume);
			} catch (System.Exception ex) {
				Log.Warn($"SweeperPickupEarcon.OnPickupComplete: {ex.Message}");
			}
		}
	}
}
