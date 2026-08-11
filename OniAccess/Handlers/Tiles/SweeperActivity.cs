using System.Collections.Generic;
using UnityEngine;

using OniAccess.Util;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Describes what an Auto-Sweeper's arm is doing, on demand.
	///
	/// SolidTransferArm publishes nothing about its own work: its state
	/// machine only swaps animations, and Operational.IsActive - the one flag
	/// that tracks busy versus idle - drives no status item, so the game has
	/// no readout of the arm to borrow. Every value here is pulled from the
	/// live chore, storage, and grid at the moment it is asked for.
	///
	/// The formatters are shared with the running narration in
	/// <see cref="Patches.Pickupable_OnCompleteWork_Patch"/> so a pickup
	/// announced while the sweeper is open reads the same as one described
	/// here.
	/// </summary>
	internal static class SweeperActivity {
		/// <summary>
		/// The arm state of a sweeper occupying this cell, or null when the
		/// cell holds no sweeper.
		/// </summary>
		internal static string DescribeAtCell(int cell) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Building];
			if (go == null) return null;
			return DescribeState(go);
		}

		/// <summary>
		/// One line for the arm's current work: off, idle, reaching for
		/// something, or carrying something to a destination. Null when the
		/// object is not a sweeper.
		/// </summary>
		internal static string DescribeState(GameObject armGo) {
			if (armGo.GetComponent<SolidTransferArm>() == null) return null;
			try {
				// Off and idle are different facts: an unpowered arm will not
				// work at all, an idle one is working and has nothing to do.
				if (!armGo.GetComponent<Operational>().IsOperational)
					return (string)STRINGS.ONIACCESS.GLANCE.ARM_OFF;

				var chore = armGo.GetComponent<ChoreDriver>().GetCurrentChore()
					as FetchAreaChore;
				if (chore == null)
					return (string)STRINGS.ONIACCESS.GLANCE.ARM_IDLE;

				return chore.IsDelivering
					? DescribeDelivering(armGo, chore)
					: DescribeCollecting(chore);
			} catch (System.Exception ex) {
				Log.Warn($"SweeperActivity.DescribeState: {ex.Message}");
				return null;
			}
		}

		private static string DescribeDelivering(
				GameObject armGo, FetchAreaChore chore) {
			var destination = chore.smi.sm.deliveryDestination.Get(chore.smi);
			if (destination == null)
				return (string)STRINGS.ONIACCESS.GLANCE.ARM_IDLE;

			string carried = DescribeCarried(armGo);
			if (carried == null)
				return string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.ARM_DELIVERING,
					DescribeBuilding(destination));
			return string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.ARM_HOLDING,
				carried, DescribeBuilding(destination));
		}

		private static string DescribeCollecting(FetchAreaChore chore) {
			var target = chore.GetFetchTarget;
			if (target == null)
				return (string)STRINGS.ONIACCESS.GLANCE.ARM_IDLE;
			var item = target.GetComponent<Pickupable>();
			if (item == null)
				return (string)STRINGS.ONIACCESS.GLANCE.ARM_IDLE;

			// The reservation amount is the honest figure once the fetch is
			// set up; before that it is still zero, and the whole pile is a
			// truer answer than "0 kg".
			float amount = chore.smi.sm.fetchAmount.Get(chore.smi);
			if (amount <= 0f) amount = item.FetchTotalAmount;

			return string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.ARM_COLLECTING,
				DescribeItem(item, amount), DescribeSource(item));
		}

		/// <summary>
		/// What the arm physically holds right now, read from its own
		/// storage rather than from the chore, so the answer is what is in
		/// the claw and not what the chore intends to move. Null when empty.
		/// </summary>
		private static string DescribeCarried(GameObject armGo) {
			var storage = armGo.GetComponent<Storage>();
			var parts = new List<string>();
			foreach (var stored in storage.items) {
				if (stored == null) continue;
				var item = stored.GetComponent<Pickupable>();
				if (item == null) continue;
				parts.Add(DescribeItem(item, item.FetchTotalAmount));
			}
			return parts.Count > 0 ? string.Join(", ", parts) : null;
		}

		/// <summary>
		/// "Copper Ore, 200 kg" - the item plus how much of it is in play.
		/// The amount arrives as mass because that is what Pickupable.Take
		/// consumes; countable items (rations, artifacts) are converted back
		/// to units so they read the way the game shows them. MassPerUnit is
		/// guarded so a zero can never send an infinity to the speech engine.
		/// </summary>
		internal static string DescribeItem(Pickupable item, float massAmount) {
			float massPerUnit = item.PrimaryElement.MassPerUnit;
			string amount =
				Assets.IsTagCountable(item.KPrefabID.PrefabTag) && massPerUnit > 0f
					? GameUtil.GetFormattedUnits(massAmount / massPerUnit)
					: GameUtil.GetFormattedMass(massAmount);
			return $"{item.gameObject.GetProperName()}, {amount}";
		}

		/// <summary>
		/// Where an item is being taken from: the building whose storage
		/// holds it, or the coordinates it is lying at as loose debris.
		/// </summary>
		internal static string DescribeSource(Pickupable item) {
			var storage = item.storage;
			if (storage != null)
				return DescribeBuilding(storage.gameObject);
			return GridCoordinates.Format(
				Grid.PosToCell(item.transform.GetPosition()));
		}

		/// <summary>
		/// "Conveyor Loader, 5, -4" - coordinates included because a sweeper
		/// commonly reaches two of the same building and the name alone
		/// would not say which one.
		/// </summary>
		internal static string DescribeBuilding(GameObject go) {
			string name = go.GetComponent<KSelectable>().GetName();
			return $"{name}, {GridCoordinates.Format(Grid.PosToCell(go))}";
		}
	}
}
