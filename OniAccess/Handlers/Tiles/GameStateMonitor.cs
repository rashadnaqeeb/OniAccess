using OniAccess.Speech;

namespace OniAccess.Handlers.Tiles {
	public class GameStateMonitor {
		private bool _firstTick = true;
		private bool _wasPaused;
		private int _lastSpeed;
		private int _lastCycle;

		public void Tick() {
			var speedScreen = SpeedControlScreen.Instance;
			bool paused = speedScreen.IsPaused;
			int speed = speedScreen.GetSpeed();
			int cycle = GameClock.Instance.GetCycle();

			if (_firstTick) {
				_firstTick = false;
				_wasPaused = paused;
				_lastSpeed = speed;
				_lastCycle = cycle;
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
		}

		public void SpeakCycleStatus() {
			int cycle = GameClock.Instance.GetCycle();
			int block = ScheduleManager.GetCurrentHour();
			SpeechPipeline.SpeakInterrupt(
				string.Format((string)STRINGS.ONIACCESS.GAME_STATE.CYCLE_STATUS, cycle, block));
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
