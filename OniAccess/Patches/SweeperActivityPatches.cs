using HarmonyLib;
using UnityEngine;

using OniAccess.Handlers.Tiles;
using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Patches {
	/// <summary>
	/// Narrates what an Auto-Sweeper's arm is doing while that sweeper is the
	/// open details-screen target: one line per completed pickup, one per
	/// completed delivery.
	///
	/// The arm's active/idle state produces no status item and no game-side
	/// readout of any kind - SolidTransferArm's state machine only swaps
	/// animations - so a sighted player learns what it is moving by watching
	/// the arm swing. These two hooks are the speech equivalent, and
	/// <see cref="SweeperActivity"/> answers the same question on demand for
	/// the tile cursor.
	///
	/// Both fire at the instant the work completes, and both read every value
	/// they speak from the live objects at that instant. Nothing is stored
	/// between calls, and there is no attach/detach lifecycle: whether to
	/// speak is decided by asking the details screen what it is showing right
	/// now, so deselecting the sweeper silences the readout immediately.
	/// </summary>
	internal static class SweeperNarration {
		/// <summary>
		/// Whether this object is an Auto-Sweeper the player currently has
		/// open in the details screen. Ordered cheapest test first: the
		/// pickup hook runs for every pickup in the colony, and the target
		/// reference compare rejects all of them but the selected one before
		/// any component lookup happens.
		/// </summary>
		internal static bool IsWatched(GameObject go) {
			if (!ModToggle.IsEnabled) return false;
			if (!ConfigManager.Config.SweeperActivityReadout) return false;
			if (!LoadGate.IsReady) return false;
			var screen = DetailsScreen.Instance;
			if (screen == null || screen.target != go) return false;
			if (!screen.gameObject.activeInHierarchy) return false;
			return go.GetComponent<SolidTransferArm>() != null;
		}
	}

	/// <summary>
	/// Pickup half of the sweeper readout. Patched as a prefix because the
	/// source is only readable before the body runs: Take() moves the item
	/// into the arm's own storage, after which its position is the arm's and
	/// Pickupable.storage no longer names where it came from.
	/// </summary>
	[HarmonyPatch(typeof(Pickupable), "OnCompleteWork")]
	internal static class Pickupable_OnCompleteWork_Patch {
		private static void Prefix(Pickupable __instance, WorkerBase worker) {
			if (!SweeperNarration.IsWatched(worker.gameObject)) return;
			try {
				var info = worker.GetStartWorkInfo()
					as Pickupable.PickupableStartWorkInfo;
				if (info == null) {
					Log.Warn("Pickupable_OnCompleteWork_Patch: "
						+ "sweeper pickup had no PickupableStartWorkInfo");
					return;
				}
				SpeechPipeline.SpeakQueued(string.Format(
					(string)STRINGS.ONIACCESS.DETAILS.SWEEPER_PICKED_UP,
					SweeperActivity.DescribeItem(__instance, info.amount),
					SweeperActivity.DescribeSource(__instance)));
			} catch (System.Exception ex) {
				Log.Warn($"Pickupable_OnCompleteWork_Patch: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Delivery half of the sweeper readout. A prefix again, because
	/// DeliverComplete's body hands the item to the destination and clears
	/// the delivery, taking both halves of the sentence with it. The guards
	/// mirror the body's own success path so a delivery that is about to
	/// fail is never announced as one that happened.
	/// </summary>
	[HarmonyPatch(typeof(FetchAreaChore.StatesInstance),
		nameof(FetchAreaChore.StatesInstance.DeliverComplete))]
	internal static class FetchAreaChore_DeliverComplete_Patch {
		private static void Prefix(FetchAreaChore.StatesInstance __instance) {
			if (!SweeperNarration.IsWatched(__instance.sm.fetcher.Get(__instance)))
				return;
			try {
				if (__instance.deliveries.Count == 0) return;
				var item = __instance.sm.deliveryObject.Get<Pickupable>(__instance);
				if (item == null || item.FetchTotalAmount <= 0f) return;
				var delivery = __instance.deliveries[0];
				var destination = delivery.destination;
				if (destination == null) return;

				// Complete() stores up to the chore's requested amount out of
				// what the arm carries, so the smaller of the two is what
				// actually lands in the destination.
				float amount = Mathf.Min(item.FetchTotalAmount, delivery.amount);
				SpeechPipeline.SpeakQueued(string.Format(
					(string)STRINGS.ONIACCESS.DETAILS.SWEEPER_DROPPED,
					SweeperActivity.DescribeItem(item, amount),
					SweeperActivity.DescribeBuilding(destination.gameObject)));
			} catch (System.Exception ex) {
				Log.Warn($"FetchAreaChore_DeliverComplete_Patch: {ex.Message}");
			}
		}
	}
}
