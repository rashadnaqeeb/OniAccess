namespace OniAccess.Audio {
	public class SonifierController {
		public static SonifierController Instance { get; private set; }

		private int _activeCell = Grid.InvalidCell;
		private ConduitType _activeConduitType = ConduitType.None;

		public SonifierController() {
			Instance = this;
		}

		public void OnCursorMoved(int cell, HashedString overlayMode) {
			if (!ConfigManager.Config.FlowSonification) {
				Deactivate();
				return;
			}

			var conduitType = GetConduitType(overlayMode);
			if (conduitType == ConduitType.None
				|| !Grid.IsValidCell(cell)
				|| !GetConduitFlow(conduitType).HasConduit(cell)) {
				Deactivate();
				return;
			}

			_activeCell = cell;
			_activeConduitType = conduitType;
			SampleAndUpdate();
		}

		public void OnOverlayChanged(HashedString newMode) {
			if (GetConduitType(newMode) == ConduitType.None)
				Deactivate();
		}

		public void Tick() {
			if (_activeConduitType == ConduitType.None) return;
			SampleAndUpdate();
		}

		public void Stop() {
			Deactivate();
		}

		private void Deactivate() {
			_activeCell = Grid.InvalidCell;
			_activeConduitType = ConduitType.None;
			Sonifier.Instance.Stop();
		}

		private void SampleAndUpdate() {
			var flow = GetConduitFlow(_activeConduitType);
			var contents = flow.GetContents(_activeCell);
			float maxMass = GetMaxMass(_activeConduitType);
			float fillRatio = UnityEngine.Mathf.Clamp01(contents.mass / maxMass);
			bool hasContents = contents.mass > 0f;
			Sonifier.Instance.UpdateTone(fillRatio, hasContents);
		}

		private static ConduitType GetConduitType(HashedString overlayMode) {
			if (overlayMode == OverlayModes.LiquidConduits.ID)
				return ConduitType.Liquid;
			if (overlayMode == OverlayModes.GasConduits.ID)
				return ConduitType.Gas;
			return ConduitType.None;
		}

		private static ConduitFlow GetConduitFlow(ConduitType type) {
			switch (type) {
				case ConduitType.Liquid: return Game.Instance.liquidConduitFlow;
				case ConduitType.Gas: return Game.Instance.gasConduitFlow;
				default: return null;
			}
		}

		private static float GetMaxMass(ConduitType type) {
			switch (type) {
				case ConduitType.Liquid: return ConduitFlow.MAX_LIQUID_MASS;
				case ConduitType.Gas: return ConduitFlow.MAX_GAS_MASS;
				default: return 1f;
			}
		}
	}
}
