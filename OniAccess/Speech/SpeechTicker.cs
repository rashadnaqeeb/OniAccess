namespace OniAccess.Speech {
	/// <summary>
	/// Per-frame tick for backends that pace speech themselves (the Mac system
	/// voice). LateUpdate runs after every Update in the frame, so the frame's
	/// announcements are all queued before the backend collects and schedules.
	/// </summary>
	public class SpeechTicker: UnityEngine.MonoBehaviour {
		private void LateUpdate() {
			SpeechEngine.Update();
		}
	}
}
