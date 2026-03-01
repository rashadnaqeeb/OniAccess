using System.Collections.Generic;
using OniAccess.Handlers.Tiles.Sections;
using OniAccess.Speech;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Modal picker for selecting one of multiple KSelectable entities at a tile.
	/// Pushed by TileCursorHandler when Enter is pressed on a cell with 2+ entities.
	/// Selecting an item calls SelectTool.Instance.Select() to open the details screen.
	/// </summary>
	public class EntityPickerHandler: BaseMenuHandler {
		private readonly IReadOnlyList<KSelectable> _selectables;

		public override string DisplayName =>
			(string)STRINGS.ONIACCESS.HANDLERS.ENTITY_PICKER;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= OniAccess.Handlers.Tools.ToolPickerHandler.ModalMenuHelp;

		public EntityPickerHandler(IReadOnlyList<KSelectable> selectables) {
			_selectables = selectables;
		}

		public override int ItemCount => _selectables.Count;

		public override string GetItemLabel(int index) {
			if (index < 0 || index >= _selectables.Count) return null;
			return DebrisNameHelper.GetDisplayName(_selectables[index].gameObject);
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			if (_currentIndex >= 0 && _currentIndex < _selectables.Count)
				SpeechPipeline.SpeakInterrupt(
					DebrisNameHelper.GetDisplayName(_selectables[_currentIndex].gameObject));
		}

		public override void OnActivate() {
			PlaySound("HUD_Click_Open");
			_currentIndex = 0;
			_search.Clear();
			SpeechPipeline.SpeakQueued(
				(string)STRINGS.ONIACCESS.TILE_CURSOR.SELECT_OBJECT);
			if (_selectables.Count > 0)
				SpeechPipeline.SpeakQueued(
					DebrisNameHelper.GetDisplayName(_selectables[0].gameObject));
		}

		public override void OnDeactivate() {
			PlaySound("HUD_Click_Close");
			base.OnDeactivate();
		}

		protected override void ActivateCurrentItem() {
			if (_currentIndex < 0 || _currentIndex >= _selectables.Count)
				return;
			var entity = _selectables[_currentIndex];
			// Pop before Select: Select() synchronously triggers DetailsScreen.OnShow
			// which pushes DetailsScreenHandler. If we pop after, we'd pop that instead.
			HandlerStack.Pop();
			if (!(PlayerController.Instance.ActiveTool is SelectTool))
				SelectTool.Instance.Activate();
			SelectTool.Instance.Select(entity);
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e))
				return true;
			if (e.TryConsume(Action.Escape)) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.TOOLTIP.CLOSED);
				HandlerStack.Pop();
				return true;
			}
			return false;
		}

		private static readonly int[] PowerLayers = {
			(int)ObjectLayer.Wire, (int)ObjectLayer.WireConnectors };
		private static readonly int[] LiquidLayers = {
			(int)ObjectLayer.LiquidConduit, (int)ObjectLayer.LiquidConduitConnection };
		private static readonly int[] GasLayers = {
			(int)ObjectLayer.GasConduit, (int)ObjectLayer.GasConduitConnection };
		private static readonly int[] SolidLayers = {
			(int)ObjectLayer.SolidConduit, (int)ObjectLayer.SolidConduitConnection };
		private static readonly int[] AutomationLayers = {
			(int)ObjectLayer.LogicWire, (int)ObjectLayer.LogicGate };

		/// <summary>
		/// Collects all selectable entities at a cell using Grid.Objects layer
		/// lookups, matching what the tile cursor sections report.
		/// </summary>
		public static List<KSelectable> CollectSelectables(int cell) {
			var result = new List<KSelectable>();
			var seen = new HashSet<GameObject>();

			CollectFromLayer(cell, (int)ObjectLayer.Building, seen, result);
			CollectFromLayer(cell, (int)ObjectLayer.FoundationTile, seen, result);
			if (!IsOverlayFocused())
				CollectFromLayer(cell, (int)ObjectLayer.Backwall, seen, result);
			CollectFromLayer(cell, (int)ObjectLayer.Minion, seen, result);
			CollectOverlayLayers(cell, seen, result);
			CollectPickupables(cell, seen, result);
			RemoveDuplicateSelectionObjects(result, seen);

			result.Sort((a, b) => EntitySortKey(a).CompareTo(EntitySortKey(b)));
			return result;
		}

		private static void CollectFromLayer(int cell, int layer,
				HashSet<GameObject> seen, List<KSelectable> result) {
			var go = Grid.Objects[cell, layer];
			if (go == null) return;
			var ks = go.GetComponent<KSelectable>();
			if (ks != null && ks.isActiveAndEnabled && ks.IsSelectable
				&& seen.Add(go))
				result.Add(ks);
		}

		private static void CollectOverlayLayers(int cell,
				HashSet<GameObject> seen, List<KSelectable> result) {
			var layers = GetOverlayLayers();
			if (layers == null) return;
			foreach (int layer in layers) {
				var go = Grid.Objects[cell, layer];
				if (go == null || !seen.Add(go)) continue;
				if (ConduitSection.IsPortRegistration(go, layer)) continue;
				var ks = go.GetComponent<KSelectable>();
				if (ks != null && ks.isActiveAndEnabled && ks.IsSelectable)
					result.Add(ks);
			}
		}

		private static int[] GetOverlayLayers() {
			if (OverlayScreen.Instance == null) return null;
			var mode = OverlayScreen.Instance.GetMode();
			if (mode == OverlayModes.Power.ID) return PowerLayers;
			if (mode == OverlayModes.LiquidConduits.ID) return LiquidLayers;
			if (mode == OverlayModes.GasConduits.ID) return GasLayers;
			if (mode == OverlayModes.SolidConveyor.ID) return SolidLayers;
			if (mode == OverlayModes.Logic.ID) return AutomationLayers;
			return null;
		}

		private static void CollectPickupables(int cell,
				HashSet<GameObject> seen, List<KSelectable> result) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (go == null) return;
			var pick = go.GetComponent<Pickupable>();
			if (pick == null) return;
			var item = pick.objectLayerListItem;
			while (item != null) {
				var ks = item.gameObject.GetComponent<KSelectable>();
				if (ks != null && ks.isActiveAndEnabled && ks.IsSelectable
					&& seen.Add(item.gameObject))
					result.Add(ks);
				item = item.nextItem;
			}
		}

		private static void RemoveDuplicateSelectionObjects(
				List<KSelectable> result, HashSet<GameObject> seen) {
			for (int i = result.Count - 1; i >= 0; i--) {
				var cso = result[i].GetComponent<CellSelectionObject>();
				if (cso != null && cso.alternateSelectionObject != null
					&& seen.Contains(cso.alternateSelectionObject.gameObject))
					result.RemoveAt(i);
			}
		}

		private static bool IsOverlayFocused() {
			return OverlayScreen.Instance != null
				&& StatusFilter.IsOverlayFocused(OverlayScreen.Instance.GetMode());
		}

		private static int EntitySortKey(KSelectable ks) {
			var building = ks.GetComponent<Building>();
			if (building != null)
				return building.Def.ObjectLayer == ObjectLayer.Building ? 0 : 1;
			if (ks.GetComponent<CellSelectionObject>() != null) return 3;
			return 2;
		}

		private static void PlaySound(string name) {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound(name));
			} catch (System.Exception ex) {
				OniAccess.Util.Log.Error(
					$"EntityPickerHandler.PlaySound failed: {ex.Message}");
			}
		}
	}
}
