using System.Collections.Generic;
using Klei.AI;
using OniAccess.Speech;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Handlers.Tiles {
	public class GameStateMonitor {
		private bool _firstTick = true;
		private bool _wasPaused;
		private int _lastSpeed;
		private int _lastCycle;
		private bool _wasRedAlert;
		private bool _wasYellowAlert;

		public void Tick() {
			var speedScreen = SpeedControlScreen.Instance;
			bool paused = speedScreen.IsPaused;
			int speed = speedScreen.GetSpeed();
			int cycle = GameClock.Instance.GetCycle();

			var world = ClusterManager.Instance.activeWorld;
			bool red = world.IsRedAlert();
			bool yellow = world.IsYellowAlert();

			if (_firstTick) {
				_firstTick = false;
				_wasPaused = paused;
				_lastSpeed = speed;
				_lastCycle = cycle;
				_wasRedAlert = red;
				_wasYellowAlert = yellow;
				return;
			}

			if (paused != _wasPaused) {
				_wasPaused = paused;
				if (paused)
					SpeechPipeline.SpeakInterrupt((string)STRINGS.UI.TOOLTIPS.PAUSEBUTTON);
				else
					SpeechPipeline.SpeakInterrupt(
						string.Format((string)STRINGS.ONIACCESS.GAME_STATE.UNPAUSED, SpeedName(speed)));
			} else if (speed != _lastSpeed) {
				SpeechPipeline.SpeakInterrupt(SpeedName(speed));
			}
			_lastSpeed = speed;

			if (cycle != _lastCycle) {
				_lastCycle = cycle;
				SpeechPipeline.SpeakInterrupt(
					string.Format((string)STRINGS.ONIACCESS.GAME_STATE.CYCLE, cycle));
			}

			if (red != _wasRedAlert) {
				if (red)
					SpeechPipeline.SpeakInterrupt((string)STRINGS.MISC.NOTIFICATIONS.REDALERT.NAME);
				else
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.GAME_STATE.RED_ALERT_OFF);
			} else if (yellow != _wasYellowAlert) {
				if (yellow)
					SpeechPipeline.SpeakInterrupt((string)STRINGS.MISC.NOTIFICATIONS.YELLOWALERT.NAME);
				else
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.GAME_STATE.YELLOW_ALERT_OFF);
			}
			_wasRedAlert = red;
			_wasYellowAlert = yellow;
		}

		public void CycleSpeed() {
			var scs = SpeedControlScreen.Instance;
			if (scs == null) return;
			int newSpeed = (scs.GetSpeed() + 1) % 3;
			PlaySpeedChangeSound(newSpeed + 1);
			scs.SetSpeed(newSpeed);
			scs.OnSpeedChange();
		}

		public void SpeakCycleStatus() {
			int cycle = GameClock.Instance.GetCycle();
			int block = ScheduleManager.GetCurrentHour();
			string msg = string.Format((string)STRINGS.ONIACCESS.GAME_STATE.CYCLE_STATUS, cycle, block);
			var world = ClusterManager.Instance.activeWorld;
			if (world.IsRedAlert())
				msg += ", " + (string)STRINGS.MISC.NOTIFICATIONS.REDALERT.NAME;
			else if (world.IsYellowAlert())
				msg += ", " + (string)STRINGS.MISC.NOTIFICATIONS.YELLOWALERT.NAME;
			SpeechPipeline.SpeakInterrupt(msg);
		}

		public void ToggleRedAlert() {
			var world = ClusterManager.Instance.activeWorld;
			world.AlertManager.ToggleRedAlert(!world.AlertManager.IsRedAlertToggledOn());
		}

		public void SpeakTimePlayed() {
			float hours = GameClock.Instance.GetTimePlayedInSeconds() / 3600f;
			SpeechPipeline.SpeakInterrupt(
				string.Format((string)STRINGS.UI.ASTEROIDCLOCK.TIME_PLAYED, hours.ToString("0.00")));
		}

		public void SpeakColonyStatus() {
			var parts = new List<string>();
			int worldId = ClusterManager.Instance.activeWorldId;
			var inventory = ClusterManager.Instance.activeWorld.worldInventory;

			// Dupes
			try {
				int local = Components.LiveMinionIdentities.GetWorldItems(worldId).Count;
				if (DlcManager.FeatureClusterSpaceEnabled()) {
					int total = Components.LiveMinionIdentities.Count;
					parts.Add(string.Format(
						(string)STRINGS.ONIACCESS.GAME_STATE.DUPES_CLUSTER, local, total));
				} else {
					parts.Add(string.Format(
						(string)STRINGS.ONIACCESS.GAME_STATE.DUPES, local));
				}
			} catch (System.Exception ex) {
				Log.Error($"SpeakColonyStatus dupes: {ex}");
			}

			// Sick
			try {
				int sick = 0;
				var minions = Components.LiveMinionIdentities.GetWorldItems(worldId);
				foreach (var minion in minions) {
					if (!minion.IsNullOrDestroyed()
						&& minion.GetComponent<MinionModifiers>().sicknesses.IsInfected())
						sick++;
				}
				if (sick > 0)
					parts.Add(string.Format(
						(string)STRINGS.ONIACCESS.GAME_STATE.SICK, sick));
			} catch (System.Exception ex) {
				Log.Error($"SpeakColonyStatus sick: {ex}");
			}

			// Rations
			try {
				float kcal = WorldResourceAmountTracker<RationTracker>.Get()
					.CountAmount(null, inventory);
				string formatted = GameUtil.GetFormattedCalories(kcal);
				string rations = string.Format(
					(string)STRINGS.ONIACCESS.GAME_STATE.RATIONS, formatted);
				string trend = GetTrend(
					TrackerTool.Instance.GetWorldTracker<KCalTracker>(worldId));
				if (trend != null)
					rations += ", " + trend;
				parts.Add(rations);
			} catch (System.Exception ex) {
				Log.Error($"SpeakColonyStatus rations: {ex}");
			}

			// Stress
			try {
				float stress = Mathf.Round(GameUtil.GetMaxStressInActiveWorld());
				string stressStr = string.Format(
					(string)STRINGS.ONIACCESS.GAME_STATE.STRESS, (int)stress);
				string trend = GetTrend(
					TrackerTool.Instance.GetWorldTracker<StressTracker>(worldId));
				if (trend != null)
					stressStr += ", " + trend;
				parts.Add(stressStr);
			} catch (System.Exception ex) {
				Log.Error($"SpeakColonyStatus stress: {ex}");
			}

			// Electrobanks
			try {
				if (Game.IsDlcActiveForCurrentSave("DLC3_ID")
					&& WorldResourceAmountTracker<ElectrobankTracker>.Get() != null) {
					bool hasBionics = false;
					var minions = Components.LiveMinionIdentities.GetWorldItems(worldId);
					foreach (var minion in minions) {
						if (!minion.IsNullOrDestroyed()
							&& minion.model == BionicMinionConfig.MODEL) {
							hasBionics = true;
							break;
						}
					}
					if (hasBionics) {
						float totalUnits;
						float joules = WorldResourceAmountTracker<ElectrobankTracker>.Get()
							.CountAmount(null, out totalUnits, inventory, true);
						string formatted = GameUtil.GetFormattedJoules(joules);
						string ebank = string.Format(
							(string)STRINGS.ONIACCESS.GAME_STATE.ELECTROBANKS, formatted);
						string trend = GetTrend(
							TrackerTool.Instance.GetWorldTracker<ElectrobankJoulesTracker>(worldId));
						if (trend != null)
							ebank += ", " + trend;
						parts.Add(ebank);
					}
				}
			} catch (System.Exception ex) {
				Log.Error($"SpeakColonyStatus electrobanks: {ex}");
			}

			if (parts.Count > 0)
				SpeechPipeline.SpeakInterrupt(string.Join(", ", parts));
		}

		private static string GetTrend(WorldTracker tracker) {
			if (tracker == null) return null;
			float delta = tracker.GetDelta(600f);
			if (delta > 0.5f)
				return (string)STRINGS.ONIACCESS.RESOURCES.RISING;
			if (delta < -0.5f)
				return (string)STRINGS.ONIACCESS.RESOURCES.FALLING;
			return null;
		}

		private static void PlaySpeedChangeSound(float speed) {
			string sound = GlobalAssets.GetSound("Speed_Change");
			if (sound != null) {
				var instance = SoundEvent.BeginOneShot(sound, UnityEngine.Vector3.zero);
				instance.setParameterByName("Speed", speed);
				SoundEvent.EndOneShot(instance);
			}
		}

		private static string SpeedName(int speed) {
			switch (speed) {
				case 0: return (string)STRINGS.UI.SPEED_SLOW;
				case 1: return (string)STRINGS.UI.SPEED_MEDIUM;
				case 2: return (string)STRINGS.UI.SPEED_FAST;
				default: return (string)STRINGS.UI.SPEED_SLOW;
			}
		}
	}
}
