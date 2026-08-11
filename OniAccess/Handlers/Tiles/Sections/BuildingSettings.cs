using System.Collections.Generic;
using UnityEngine;

using OniAccess.Util;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Speaks user-configurable building settings on the tile cursor, gated by the
	/// active overlay so each setting surfaces where the player is already reasoning
	/// about that subsystem. A building can belong to several overlays: an Atmo
	/// Sensor reads under Logic, a conduit temperature sensor under Logic, its pipe
	/// view and Temperature. Settings whose value cannot be stated in one line
	/// (storage filters, cluster location lists, access permissions) are deliberately
	/// absent. Off by default behind <see cref="ModConfig.BuildingSettingsReadout"/>.
	/// </summary>
	public static class BuildingSettings {
		public static void Append(GameObject go, List<string> tokens) {
			if (!ConfigManager.Config.BuildingSettingsReadout) return;
			if (OverlayScreen.Instance == null) return;

			var mode = OverlayScreen.Instance.GetMode();
			if (mode == OverlayModes.None.ID) return;

			ReadThreshold(go, mode, tokens);
			ReadFilter(go, mode, tokens);
			ReadActivationRange(go, mode, tokens);
			ReadSlider(go, mode, tokens);
			ReadValve(go, mode, tokens);
			ReadLimitValve(go, mode, tokens);
			ReadFuelCapacity(go, mode, tokens);
			ReadCounter(go, mode, tokens);
			ReadTimer(go, mode, tokens);
			ReadTimeRange(go, mode, tokens);
			ReadCritterCounting(go, mode, tokens);
			ReadRibbonBits(go, mode, tokens);
			ReadAlarm(go, mode, tokens);
			ReadBroadcastChannel(go, mode, tokens);
			ReadRedirector(go, mode, tokens);
			ReadAutomationOnly(go, mode, tokens);
			ReadReceptacle(go, mode, tokens);
			ReadLure(go, mode, tokens);
		}

		// --- Overlay routing helpers ---

		private static HashedString ConduitOverlay(ConduitType type) {
			switch (type) {
				case ConduitType.Gas: return OverlayModes.GasConduits.ID;
				case ConduitType.Liquid: return OverlayModes.LiquidConduits.ID;
				case ConduitType.Solid: return OverlayModes.SolidConveyor.ID;
				default: return OverlayModes.None.ID;
			}
		}

		private static HashedString FilterOverlay(Filterable.ElementState state) {
			switch (state) {
				case Filterable.ElementState.Gas: return OverlayModes.GasConduits.ID;
				case Filterable.ElementState.Liquid: return OverlayModes.LiquidConduits.ID;
				case Filterable.ElementState.Solid: return OverlayModes.SolidConveyor.ID;
				default: return OverlayModes.None.ID;
			}
		}

		/// <summary>
		/// The conduit view a building belongs to, taken from its primary input port.
		/// Used where a single component backs both a gas and a liquid building
		/// (SmartReservoir) and carries no discriminator of its own.
		/// </summary>
		private static HashedString BuildingConduitOverlay(GameObject go) {
			var building = go.GetComponent<Building>();
			if (building == null) return OverlayModes.None.ID;
			return ConduitOverlay(building.Def.InputConduitType);
		}

		// --- Readers ---

		/// <summary>
		/// Threshold sensors and switches. Always readable under Logic since they all
		/// drive an automation signal, plus the view of whatever they measure.
		/// The gas and liquid variants of LogicPressureSensor and PressureSwitch share
		/// one component with no discriminator, so they stay Logic-only rather than
		/// risk claiming the wrong conduit view.
		/// </summary>
		private static void ReadThreshold(
				GameObject go, HashedString mode, List<string> tokens) {
			var threshold = go.GetComponent<IThresholdSwitch>();
			if (threshold == null || !ThresholdApplies(threshold, mode)) return;

			string direction = threshold.ActivateAboveThreshold
				? (string)STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.ABOVE_BUTTON
				: (string)STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.BELOW_BUTTON;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_THRESHOLD,
				(string)threshold.ThresholdValueName,
				direction,
				threshold.Format(threshold.Threshold, true)));
		}

		private static bool ThresholdApplies(IThresholdSwitch threshold, HashedString mode) {
			if (mode == OverlayModes.Logic.ID) return true;

			var conduit = threshold as ConduitSensor;
			if (conduit != null && ConduitOverlay(conduit.conduitType) == mode)
				return true;

			if (mode == OverlayModes.Temperature.ID
				&& (threshold is LogicTemperatureSensor
					|| threshold is TemperatureControlledSwitch
					|| threshold is ConduitTemperatureSensor))
				return true;
			if (mode == OverlayModes.Disease.ID
				&& (threshold is LogicDiseaseSensor || threshold is ConduitDiseaseSensor))
				return true;
			if (mode == OverlayModes.Radiation.ID
				&& (threshold is LogicRadiationSensor || threshold is LogicHEPSensor))
				return true;
			if (mode == OverlayModes.Power.ID && threshold is LogicWattageSensor)
				return true;
			if (mode == OverlayModes.Light.ID && threshold is LogicLightSensor)
				return true;
			return false;
		}

		/// <summary>
		/// Element filters and element sensors. The component's own element state picks
		/// the conduit view; sensors additionally read under Logic because their
		/// selection is what drives the signal.
		/// </summary>
		private static void ReadFilter(
				GameObject go, HashedString mode, List<string> tokens) {
			var filterable = go.GetComponent<Filterable>();
			if (filterable == null) return;

			bool isSensor = go.GetComponent<LogicElementSensor>() != null
				|| go.GetComponent<ConduitElementSensor>() != null;
			bool applies = FilterOverlay(filterable.filterElementState) == mode
				|| (mode == OverlayModes.Logic.ID && isSensor);
			if (!applies) return;

			var tag = filterable.SelectedTag;
			if (!tag.IsValid || tag == GameTags.Void) {
				tokens.Add((string)STRINGS.ONIACCESS.GLANCE.SETTING_FILTER_NONE);
				return;
			}
			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_FILTER, tag.ProperName()));
		}

		/// <summary>
		/// Two-ended activation ranges: the Smart Battery's charge window under Power
		/// and the smart reservoirs' fill window under their conduit view.
		/// </summary>
		private static void ReadActivationRange(
				GameObject go, HashedString mode, List<string> tokens) {
			var range = go.GetComponent<IActivationRangeTarget>();
			if (range == null) return;

			bool applies =
				(mode == OverlayModes.Power.ID && range is BatterySmart)
				|| (range is SmartReservoir && BuildingConduitOverlay(go) == mode);
			if (!applies) return;

			string format = range.UseWholeNumbers ? "F0" : "F1";
			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_ACTIVATION_RANGE,
				range.ActivateSliderLabelText,
				range.ActivateValue.ToString(format),
				range.DeactivateSliderLabelText,
				range.DeactivateValue.ToString(format)));
		}

		/// <summary>
		/// Single-slider settings, routed by the subsystem each one belongs to. Sliders
		/// on buildings with no overlay home (medical cot, oil well cap) are skipped.
		/// </summary>
		private static void ReadSlider(
				GameObject go, HashedString mode, List<string> tokens) {
			var slider = go.GetComponent<ISingleSliderControl>();
			if (slider == null || !SliderApplies(slider, mode)) return;

			StringEntry title;
			if (!Strings.TryGet(slider.SliderTitleKey, out title)) {
				Log.Warn("BuildingSettings: missing slider title key "
					+ slider.SliderTitleKey);
				return;
			}

			string value = slider.GetSliderValue(0)
				.ToString("F" + slider.SliderDecimalPlaces(0));
			if (!string.IsNullOrEmpty(slider.SliderUnits))
				value = value + " " + slider.SliderUnits;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_SLIDER, title.String, value));
		}

		private static bool SliderApplies(ISingleSliderControl slider, HashedString mode) {
			if (mode == OverlayModes.Temperature.ID && slider is SpaceHeater)
				return true;
			if (mode == OverlayModes.Logic.ID
				&& (slider is LogicGateBuffer || slider is LogicGateFilter))
				return true;
			if (mode == OverlayModes.Radiation.ID
				&& (slider is HEPBattery || slider is HighEnergyParticleSpawner))
				return true;
			if (mode == OverlayModes.Power.ID
				&& (slider is ManualGenerator || slider is EnergyGenerator))
				return true;
			return false;
		}

		/// <summary>
		/// Valve throughput. DesiredFlow is the player's setting; MaxFlow is the
		/// building's fixed ceiling and would misreport every unadjusted valve.
		/// </summary>
		private static void ReadValve(
				GameObject go, HashedString mode, List<string> tokens) {
			var valve = go.GetComponent<Valve>();
			if (valve == null) return;

			var valveBase = go.GetComponent<ValveBase>();
			if (valveBase == null || ConduitOverlay(valveBase.conduitType) != mode)
				return;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_FLOW,
				GameUtil.GetFormattedMass(valve.DesiredFlow,
					GameUtil.TimeSlice.PerSecond, GameUtil.MetricMassFormat.Gram)));
		}

		private static void ReadLimitValve(
				GameObject go, HashedString mode, List<string> tokens) {
			var limit = go.GetComponent<LimitValve>();
			if (limit == null || ConduitOverlay(limit.conduitType) != mode) return;

			string amount = limit.displayUnitsInsteadOfMass
				? GameUtil.GetFormattedUnits(limit.Limit)
				: GameUtil.GetFormattedMass(limit.Limit);
			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_LIMIT, amount));
		}

		/// <summary>
		/// Radbolt fuel tank capacity. Other IUserControlledCapacity holders are
		/// storage buildings with no overlay to surface under.
		/// </summary>
		private static void ReadFuelCapacity(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Radiation.ID) return;

			var capacity = go.GetComponent<HEPFuelTank>();
			if (capacity == null) return;

			float max = capacity.UserMaxCapacity;
			if (float.IsPositiveInfinity(max)) return;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_CAPACITY,
				max.ToString("F0") + " " + (string)capacity.CapacityUnits));
		}

		private static void ReadCounter(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Logic.ID) return;

			var counter = go.GetComponent<LogicCounter>();
			if (counter == null) return;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_COUNT, counter.maxCount));
			if (counter.advancedMode)
				tokens.Add((string)STRINGS.ONIACCESS.GLANCE.SETTING_COUNT_ADVANCED);
		}

		private static void ReadTimer(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Logic.ID) return;

			var timer = go.GetComponent<LogicTimerSensor>();
			if (timer == null) return;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_TIMER,
				FormatDuration(timer.onDuration, timer.displayCyclesMode),
				FormatDuration(timer.offDuration, timer.displayCyclesMode)));
		}

		private static string FormatDuration(float seconds, bool cyclesMode) {
			return cyclesMode
				? GameUtil.GetFormattedCycles(seconds, "F2")
				: GameUtil.GetFormattedTime(seconds);
		}

		/// <summary>
		/// Time of day sensor. Both fields are normalized fractions of a cycle, so
		/// they read as percentages rather than being converted to a clock time.
		/// </summary>
		private static void ReadTimeRange(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Logic.ID) return;

			var sensor = go.GetComponent<LogicTimeOfDaySensor>();
			if (sensor == null) return;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_TIME_RANGE,
				GameUtil.GetFormattedPercent(sensor.startTime * 100f),
				GameUtil.GetFormattedPercent(sensor.duration * 100f)));
		}

		private static void ReadCritterCounting(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Logic.ID) return;

			var sensor = go.GetComponent<LogicCritterCountSensor>();
			if (sensor == null) return;

			if (sensor.countCritters && sensor.countEggs)
				tokens.Add((string)STRINGS.ONIACCESS.GLANCE.SETTING_CRITTERS_AND_EGGS);
			else if (sensor.countCritters)
				tokens.Add((string)STRINGS.ONIACCESS.GLANCE.SETTING_CRITTERS);
			else if (sensor.countEggs)
				tokens.Add((string)STRINGS.ONIACCESS.GLANCE.SETTING_EGGS);
		}

		private static void ReadRibbonBits(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Logic.ID) return;

			var selector = go.GetComponent<ILogicRibbonBitSelector>();
			if (selector == null) return;

			var active = new List<string>();
			int depth = selector.GetBitDepth();
			for (int bit = 0; bit < depth; bit++)
				if (selector.IsBitActive(bit))
					active.Add((bit + 1).ToString());

			tokens.Add(active.Count == 0
				? (string)STRINGS.ONIACCESS.GLANCE.SETTING_BITS_NONE
				: string.Format((string)STRINGS.ONIACCESS.GLANCE.SETTING_BITS,
					string.Join(", ", active.ToArray())));
		}

		private static void ReadAlarm(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Logic.ID) return;

			var alarm = go.GetComponent<LogicAlarm>();
			if (alarm == null || string.IsNullOrEmpty(alarm.notificationName)) return;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_ALARM, alarm.notificationName));
		}

		private static void ReadBroadcastChannel(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Logic.ID) return;

			var receiver = go.GetComponent<LogicBroadcastReceiver>();
			if (receiver == null) return;

			var channel = receiver.GetChannel();
			if (channel == null) return;

			var selectable = channel.GetComponent<KSelectable>();
			if (selectable == null) return;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_CHANNEL,
				selectable.GetName()));
		}

		private static void ReadRedirector(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Radiation.ID) return;

			var redirector = go.GetComponent<HighEnergyParticleRedirector>();
			if (redirector == null || !redirector.directionControllable) return;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_AIMED,
				BuildingSection.EightDirectionToString(redirector.Direction)));
		}

		private static void ReadAutomationOnly(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Logic.ID && mode != OverlayModes.SolidConveyor.ID)
				return;

			var automatable = go.GetComponent<Automatable>();
			if (automatable == null || !automatable.GetAutomationOnly()) return;

			tokens.Add((string)STRINGS.ONIACCESS.GLANCE.SETTING_AUTOMATION_ONLY);
		}

		/// <summary>
		/// The seed a planter is set to grow, or the egg an incubator is set to hatch.
		/// </summary>
		private static void ReadReceptacle(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Crop.ID) return;

			var receptacle = go.GetComponent<SingleEntityReceptacle>();
			if (receptacle == null) return;

			var tag = receptacle.requestedEntityTag;
			tokens.Add(tag.IsValid
				? string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.SETTING_SELECTED, tag.ProperName())
				: (string)STRINGS.ONIACCESS.GLANCE.SETTING_SELECTED_NONE);
		}

		private static void ReadLure(
				GameObject go, HashedString mode, List<string> tokens) {
			if (mode != OverlayModes.Crop.ID) return;

			var lure = go.GetComponent<CreatureLure>();
			if (lure == null || !lure.activeBaitSetting.IsValid) return;

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.SETTING_BAIT,
				lure.activeBaitSetting.ProperName()));
		}
	}
}
