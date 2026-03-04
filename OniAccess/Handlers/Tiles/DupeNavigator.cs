using System.Collections.Generic;
using Klei.AI;
using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Owns dupe cycle navigation for [ / ] and \ keys.
	/// All dupe data is re-queried live on each call; _dupeIndex is the
	/// only mod-side state.
	/// </summary>
	public class DupeNavigator {
		private int _dupeIndex;

		public void CycleDupe(int direction) {
			try {
				var dupes = GetWorldDupes();
				if (dupes.Count == 0) {
					BaseScreenHandler.PlaySound("Negative");
					SpeechPipeline.SpeakInterrupt(
						(string)STRINGS.ONIACCESS.DUPES.NO_DUPLICANTS);
					return;
				}
				_dupeIndex = ((_dupeIndex + direction) % dupes.Count + dupes.Count) % dupes.Count;
				SpeechPipeline.SpeakInterrupt(BuildAnnouncement(dupes[_dupeIndex]));
			} catch (System.Exception ex) {
				Log.Error($"DupeNavigator.CycleDupe: {ex}");
			}
		}

		public void JumpOrSelect() {
			try {
				var dupes = GetWorldDupes();
				if (dupes.Count == 0) {
					BaseScreenHandler.PlaySound("Negative");
					SpeechPipeline.SpeakInterrupt(
						(string)STRINGS.ONIACCESS.DUPES.NO_DUPLICANTS);
					return;
				}
				if (_dupeIndex >= dupes.Count)
					_dupeIndex = dupes.Count - 1;
				var mi = dupes[_dupeIndex];
				int dupeCell = Grid.PosToCell(mi);
				if (TileCursor.Instance.Cell == dupeCell) {
					if (!(PlayerController.Instance.ActiveTool is SelectTool))
						SelectTool.Instance.Activate();
					var selectable = mi.GetComponent<KSelectable>();
					SelectTool.Instance.Select(null);
					SelectTool.Instance.Select(selectable);
				} else {
					string speech = TileCursor.Instance.JumpTo(dupeCell);
					if (speech != null)
						SpeechPipeline.SpeakInterrupt(speech);
				}
			} catch (System.Exception ex) {
				Log.Error($"DupeNavigator.JumpOrSelect: {ex}");
			}
		}

		private static IList<MinionIdentity> GetWorldDupes() {
			int worldId = ClusterManager.Instance.activeWorldId;
			return Components.LiveMinionIdentities.GetWorldItems(worldId);
		}

		private static string BuildAnnouncement(MinionIdentity mi) {
			string name = mi.GetProperName();
			string task = BuildTaskPart(mi);
			string statuses = BuildStatusPart(mi);
			if (statuses != null)
				return $"{name}, {task}, {statuses}";
			return $"{name}, {task}";
		}

		private static string BuildTaskPart(MinionIdentity mi) {
			try {
				var chore = mi.GetComponent<ChoreDriver>().GetCurrentChore();
				if (chore == null)
					return (string)STRINGS.ONIACCESS.DUPES.IDLE;
				return GameUtil.GetChoreName(chore, null);
			} catch (System.Exception ex) {
				Log.Warn($"DupeNavigator.BuildTaskPart: {ex}");
				return (string)STRINGS.ONIACCESS.DUPES.IDLE;
			}
		}

		// Each entry maps a check to a spoken label. Add/remove/reorder
		// entries here to change which statuses are announced.
		private static StatusCheck[] _statusChecks;

		private static StatusCheck[] GetStatusChecks() {
			if (_statusChecks != null)
				return _statusChecks;
			var dupeItems = Db.Get().DuplicantStatusItems;
			var creatureItems = Db.Get().CreatureStatusItems;
			_statusChecks = new StatusCheck[] {
				new StatusCheck(
					mi => mi.GetComponent<Health>().State == Health.HealthState.Incapacitated,
					(string)STRINGS.ONIACCESS.DUPES.INCAPACITATED),
				new StatusCheck(
					mi => mi.GetComponent<Health>().State == Health.HealthState.Critical,
					(string)STRINGS.ONIACCESS.DUPES.HEALTH_CRITICAL),
				new StatusCheck(
					mi => mi.GetComponent<Health>().State == Health.HealthState.Injured,
					(string)STRINGS.ONIACCESS.DUPES.HEALTH_INJURED),
				new StatusCheck(dupeItems.SevereWounds,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.SEVEREWOUNDS.NAME),
				new StatusCheck(dupeItems.Suffocating,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.SUFFOCATING.NAME),
				new StatusCheck(dupeItems.HoldingBreath,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.HOLDINGBREATH.NAME),
				new StatusCheck(dupeItems.NervousBreakdown,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.NERVOUSBREAKDOWN.NAME),
				new StatusCheck(dupeItems.Stressed,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.STRESSED.NAME),
				new StatusCheck(creatureItems.Scalding,
					(string)STRINGS.CREATURES.STATUSITEMS.SCALDING.NAME),
				new StatusCheck(dupeItems.ExitingCold,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.EXITINGCOLD.NAME),
				new StatusCheck(
					mi => mi.GetComponent<MinionModifiers>().sicknesses.IsInfected(),
					(string)STRINGS.ONIACCESS.DUPES.SICK),
				new StatusCheck(dupeItems.Starving,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.STARVING.NAME),
				new StatusCheck(dupeItems.EntombedChore,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.ENTOMBEDCHORE.NAME),
				new StatusCheck(dupeItems.Fleeing,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.FLEEING.NAME),
				new StatusCheck(dupeItems.BionicCriticalBattery,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.BIONICCRITICALBATTERY.NAME),
				new StatusCheck(dupeItems.BionicOfflineIncapacitated,
					(string)STRINGS.DUPLICANTS.STATUSITEMS.BIONICOFFLINEINCAPACITATED.NAME),
			};
			return _statusChecks;
		}

		private static string BuildStatusPart(MinionIdentity mi) {
			var results = new List<string>();
			var selectable = mi.GetComponent<KSelectable>();
			try {
				foreach (var check in GetStatusChecks()) {
					if (check.Test(mi, selectable))
						results.Add(check.Label);
				}
			} catch (System.Exception ex) {
				Log.Warn($"DupeNavigator.BuildStatusPart: {ex}");
			}
			return results.Count > 0 ? string.Join(", ", results) : null;
		}

		private struct StatusCheck {
			private readonly StatusItem _statusItem;
			private readonly System.Func<MinionIdentity, bool> _predicate;
			public readonly string Label;

			public StatusCheck(StatusItem statusItem, string label) {
				_statusItem = statusItem;
				_predicate = null;
				Label = label;
			}

			public StatusCheck(System.Func<MinionIdentity, bool> predicate, string label) {
				_statusItem = null;
				_predicate = predicate;
				Label = label;
			}

			public bool Test(MinionIdentity mi, KSelectable selectable) {
				if (_predicate != null)
					return _predicate(mi);
				return selectable.HasStatusItem(_statusItem);
			}
		}
	}
}
