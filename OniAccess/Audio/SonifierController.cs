namespace OniAccess.Audio {
	public class SonifierController {
		public static SonifierController Instance { get; private set; }

		private enum SonifierMode { None, Conduit, Power }

		private int _activeCell = Grid.InvalidCell;
		private SonifierMode _activeMode = SonifierMode.None;
		private ConduitType _activeConduitType = ConduitType.None;

		public SonifierController() {
			Instance = this;
		}

		public void OnCursorMoved(int cell, HashedString overlayMode) {
			if (!ConfigManager.Config.FlowSonification) {
				Deactivate();
				return;
			}

			if (!Grid.IsValidCell(cell)) {
				Deactivate();
				return;
			}

			if (overlayMode == OverlayModes.Power.ID) {
				if (Game.Instance.electricalConduitSystem.GetNetworkForCell(cell) == null) {
					Deactivate();
					return;
				}
				_activeCell = cell;
				_activeMode = SonifierMode.Power;
				_activeConduitType = ConduitType.None;
				SampleAndUpdate();
				return;
			}

			var conduitType = GetConduitType(overlayMode);
			if (conduitType == ConduitType.None
				|| !GetConduitFlow(conduitType).HasConduit(cell)) {
				Deactivate();
				return;
			}

			_activeCell = cell;
			_activeMode = SonifierMode.Conduit;
			_activeConduitType = conduitType;
			SampleAndUpdate();
		}

		public void OnOverlayChanged(HashedString newMode) {
			if (newMode != OverlayModes.Power.ID && GetConduitType(newMode) == ConduitType.None)
				Deactivate();
		}

		public void Tick() {
			if (_activeMode == SonifierMode.None) return;
			SampleAndUpdate();
		}

		public void Stop() {
			Deactivate();
		}

		private void Deactivate() {
			_activeCell = Grid.InvalidCell;
			_activeMode = SonifierMode.None;
			_activeConduitType = ConduitType.None;
			Sonifier.Instance.Stop();
		}

		private void SampleAndUpdate() {
			if (_activeMode == SonifierMode.Power) {
				SamplePower();
				return;
			}
			var flow = GetConduitFlow(_activeConduitType);
			var contents = flow.GetContents(_activeCell);
			float maxMass = GetMaxMass(_activeConduitType);
			float fillRatio = UnityEngine.Mathf.Clamp01(contents.mass / maxMass);
			bool hasContents = contents.mass > 0f;
			Sonifier.Instance.UpdateTone(fillRatio, hasContents);
		}

		private void SamplePower() {
			ushort circuitID = Game.Instance.circuitManager.GetCircuitID(_activeCell);
			if (circuitID == ushort.MaxValue) {
				Sonifier.Instance.UpdateTone(0f, false);
				return;
			}
			float wattsUsed = Game.Instance.circuitManager.GetWattsUsedByCircuit(circuitID);
			float maxWatts = Game.Instance.circuitManager.GetMaxSafeWattageForCircuit(circuitID);
			float fillRatio = maxWatts > 0f ? UnityEngine.Mathf.Clamp01(wattsUsed / maxWatts) : 0f;
			Sonifier.Instance.UpdateTone(fillRatio, wattsUsed > 0f);
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
