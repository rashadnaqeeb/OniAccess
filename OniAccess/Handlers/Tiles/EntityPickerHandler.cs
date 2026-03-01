using System.Collections.Generic;
using OniAccess.Speech;
using OniAccess.Util;

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

		/// <summary>
		/// Collects all selectable entities at a cell using the same collision
		/// layer the game's SelectTool queries on mouse click.
		/// </summary>
		public static List<KSelectable> CollectSelectables(int cell) {
			var result = new List<KSelectable>();
			int x, y;
			Grid.CellToXY(cell, out x, out y);
			var cellCenter = Grid.CellToPosCCC(cell, Grid.SceneLayer.Move);

			var entries = ListPool<ScenePartitionerEntry, EntityPickerHandler>.Allocate();
			GameScenePartitioner.Instance.GatherEntries(
				x, y, 1, 1,
				GameScenePartitioner.Instance.collisionLayer,
				entries);

			var seen = new HashSet<UnityEngine.GameObject>();
			foreach (var entry in entries) {
				var collider = entry.obj as KCollider2D;
				if (collider == null) continue;
				if (!collider.Intersects(new UnityEngine.Vector2(cellCenter.x, cellCenter.y)))
					continue;
				var ks = collider.GetComponent<KSelectable>();
				if (ks == null)
					ks = collider.GetComponentInParent<KSelectable>();
				if (ks == null || !ks.isActiveAndEnabled || !ks.IsSelectable) continue;
				if (!seen.Add(ks.gameObject)) continue;
				var cso = ks.GetComponent<CellSelectionObject>();
				if (cso != null && cso.alternateSelectionObject != null
					&& seen.Contains(cso.alternateSelectionObject.gameObject))
					continue;
				result.Add(ks);
			}

			entries.Recycle();

			var pickGo = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (pickGo != null) {
				var pick = pickGo.GetComponent<Pickupable>();
				if (pick != null) {
					var item = pick.objectLayerListItem;
					while (item != null) {
						var ks = item.gameObject.GetComponent<KSelectable>();
						if (ks != null && ks.isActiveAndEnabled && ks.IsSelectable
							&& seen.Add(ks.gameObject))
							result.Add(ks);
						item = item.nextItem;
					}
				}
			}

			result.Sort((a, b) => EntitySortKey(a).CompareTo(EntitySortKey(b)));
			return result;
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
