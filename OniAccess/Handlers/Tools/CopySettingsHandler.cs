using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Handlers.Tiles;
using OniAccess.Input;
using OniAccess.Speech;
using UnityEngine;

namespace OniAccess.Handlers.Tools {
	/// <summary>
	/// Handler for CopySettingsTool. Sits on top of TileCursorHandler.
	/// CapturesAllInput = false so arrow keys fall through to TileCursorHandler.
	/// Space applies settings and stays in tool, Enter applies and exits,
	/// Escape cancels.
	/// </summary>
	public class CopySettingsHandler: IAccessHandler {
		public string DisplayName => BuildActivationName();
		public bool CapturesAllInput => false;

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.Space),
			new ConsumedKey(KKeyCode.Return),
		};
		public IReadOnlyList<ConsumedKey> ConsumedKeys => _consumedKeys;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Space", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.APPLY_SETTINGS),
			new HelpEntry("Enter", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.APPLY_AND_EXIT),
			new HelpEntry("Escape", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.CANCEL_TOOL),
		}.AsReadOnly();
		public IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public void OnActivate() {
			if (Game.Instance != null) {
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
				Game.Instance.Subscribe(1174281782, OnActiveToolChanged);
			}
			SpeechPipeline.SpeakInterrupt(DisplayName);
		}

		public void OnDeactivate() {
			if (Game.Instance != null)
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
		}

		private void OnActiveToolChanged(object data) {
			if (data is SelectTool) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.CANCELED);
				BaseScreenHandler.PlaySound("Tile_Cancel");
				HandlerStack.Pop();
			}
		}

		public bool Tick() {
			if (InputUtil.AnyModifierHeld()) return false;
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space)) {
				TryApply(exitAfter: false);
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
				TryApply(exitAfter: true);
				return true;
			}
			return false;
		}

		public bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.CANCELED);
				BaseScreenHandler.PlaySound("Tile_Cancel");
				ExitTool();
				return true;
			}
			return false;
		}

		private void TryApply(bool exitAfter) {
			var tool = CopySettingsTool.Instance;
			if (tool == null) {
				ExitTool();
				return;
			}

			int cell = TileCursor.Instance.Cell;
			var sourceGO = Traverse.Create(tool)
				.Field("sourceGameObject").GetValue<GameObject>();

			bool success = CopyBuildingSettings.ApplyCopy(cell, sourceGO);
			if (success) {
				var targetGO = GetTargetGO(cell, sourceGO);
				if (FarmCopyFailed(sourceGO, targetGO)) {
					BaseScreenHandler.PlaySound("Negative");
					SpeechPipeline.SpeakInterrupt(
						(string)STRINGS.ONIACCESS.TOOLS.COPY_SETTINGS_FAILED);
				} else {
					BaseScreenHandler.PlaySound("HUD_Click");
					string targetName = targetGO?.GetComponent<KSelectable>()?.GetName();
					string appliedText = targetName != null
						? targetName + ", " + (string)STRINGS.UI.COPIED_SETTINGS
						: (string)STRINGS.UI.COPIED_SETTINGS;
					SpeechPipeline.SpeakInterrupt(appliedText);
				}
			} else {
				BaseScreenHandler.PlaySound("Negative");
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.TOOLS.COPY_SETTINGS_NO_TARGET);
			}

			if (exitAfter) {
				SpeechPipeline.SpeakQueued(
					(string)STRINGS.ONIACCESS.TOOLS.DONE);
				ExitTool();
			}
		}

		private static void ExitTool() {
			Screens.DetailsScreenHandler.SuppressNextActivation = true;
			HandlerStack.Pop();
			SelectTool.Instance.Activate();
		}

		private static string BuildActivationName() {
			var tool = CopySettingsTool.Instance;
			if (tool == null)
				return (string)STRINGS.UI.USERMENUACTIONS.COPY_BUILDING_SETTINGS.NAME;

			var sourceGO = Traverse.Create(tool)
				.Field("sourceGameObject").GetValue<GameObject>();
			if (sourceGO == null)
				return (string)STRINGS.UI.USERMENUACTIONS.COPY_BUILDING_SETTINGS.NAME;

			var sel = sourceGO.GetComponent<KSelectable>();
			string name = sel?.GetName();
			if (string.IsNullOrEmpty(name))
				return (string)STRINGS.UI.USERMENUACTIONS.COPY_BUILDING_SETTINGS.NAME;

			return string.Format(
				(string)STRINGS.ONIACCESS.TOOLS.COPY_SETTINGS_ACTIVATION, name);
		}

		private static GameObject GetTargetGO(int cell, GameObject sourceGO) {
			ObjectLayer layer = ObjectLayer.Building;
			if (sourceGO.GetComponent<MoverLayerOccupier>() != null)
				layer = ObjectLayer.Mover;
			var building = sourceGO.GetComponent<BuildingComplete>();
			if (building != null)
				layer = building.Def.ObjectLayer;
			return Grid.Objects[cell, (int)layer];
		}

		private static bool FarmCopyFailed(GameObject sourceGO, GameObject targetGO) {
			if (targetGO == null) return false;
			var destPlot = targetGO.GetComponent<PlantablePlot>();
			if (destPlot == null) return false;
			var sourcePlot = sourceGO.GetComponent<PlantablePlot>();
			if (sourcePlot == null) return false;
			return GetEffectiveSeedTag(sourcePlot) != GetEffectiveSeedTag(destPlot);
		}

		private static Tag GetEffectiveSeedTag(PlantablePlot plot) {
			if (plot.Occupant != null) {
				var seedProducer = plot.Occupant.GetComponent<SeedProducer>();
				if (seedProducer != null)
					return TagManager.Create(seedProducer.seedInfo.seedId);
			}
			return plot.requestedEntityTag;
		}
	}
}
