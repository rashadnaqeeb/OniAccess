using System.Collections;
using System.Reflection;
using OniAccess.Handlers.Tiles.Scanner;
using UnityEngine;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Reads the game's UserNavigation bookmark data via reflection and
	/// provides jump-to-bookmark, orient-to-bookmark, and jump-home operations
	/// that move the tile cursor instead of panning the camera.
	/// </summary>
	public class CursorBookmarks {
		private readonly FieldInfo _hotkeyNavPointsField;
		private readonly FieldInfo _posField;
		private readonly FieldInfo _orthoSizeField;

		public CursorBookmarks() {
			var userNavType = typeof(UserNavigation);
			_hotkeyNavPointsField = userNavType.GetField(
				"hotkeyNavPoints",
				BindingFlags.Instance | BindingFlags.NonPublic);
			if (_hotkeyNavPointsField == null) {
				Util.Log.Warn("CursorBookmarks: hotkeyNavPoints field not found");
				return;
			}

			// NavPoint is a private nested struct — find it and cache its fields
			var navPointType = userNavType.GetNestedType(
				"NavPoint", BindingFlags.NonPublic);
			if (navPointType == null) {
				Util.Log.Warn("CursorBookmarks: NavPoint type not found");
				return;
			}
			_posField = navPointType.GetField("pos", BindingFlags.Public | BindingFlags.Instance);
			_orthoSizeField = navPointType.GetField("orthoSize", BindingFlags.Public | BindingFlags.Instance);
			if (_posField == null || _orthoSizeField == null)
				Util.Log.Warn("CursorBookmarks: NavPoint fields not found");
		}

		public string Goto(int index) {
			if (!TryReadBookmark(index, out Vector3 pos))
				return (string)STRINGS.ONIACCESS.BOOKMARKS.NO_BOOKMARK;
			int cell = Grid.PosToCell(pos);
			string speech = TileCursor.Instance.JumpTo(cell);
			if (speech != null)
				PlayRecallSound(index);
			return speech ?? (string)STRINGS.ONIACCESS.BOOKMARKS.NO_BOOKMARK;
		}

		public string Orient(int index) {
			if (!TryReadBookmark(index, out Vector3 pos))
				return (string)STRINGS.ONIACCESS.BOOKMARKS.NO_BOOKMARK;
			int targetCell = Grid.PosToCell(pos);
			int cursorCell = TileCursor.Instance.Cell;
			string distance = AnnouncementFormatter.FormatDistance(cursorCell, targetCell);
			if (string.IsNullOrEmpty(distance))
				return (string)STRINGS.ONIACCESS.BOOKMARKS.AT_BOOKMARK;
			return distance;
		}

		public static string JumpHome() {
			var telepad = GameUtil.GetActiveTelepad();
			if (telepad == null)
				return (string)STRINGS.ONIACCESS.BOOKMARKS.NO_HOME;
			int cell = Grid.PosToCell(telepad.transform.GetPosition());
			string speech = TileCursor.Instance.JumpTo(cell);
			if (speech != null)
				KMonoBehaviour.PlaySound(GlobalAssets.GetSound("Click_Notification"));
			return speech ?? (string)STRINGS.ONIACCESS.BOOKMARKS.NO_HOME;
		}

		public static int DigitKeyToIndex(KeyCode key) {
			if (key >= KeyCode.Alpha1 && key <= KeyCode.Alpha9)
				return (int)(key - KeyCode.Alpha1);
			if (key == KeyCode.Alpha0)
				return 9;
			return -1;
		}

		private static void PlayRecallSound(int index) {
			string sound = GlobalAssets.GetSound("UserNavPoint_recall");
			if (sound != null) {
				var instance = KFMOD.BeginOneShot(sound, Vector3.zero);
				instance.setParameterByName("userNavPoint_ID", index);
				KFMOD.EndOneShot(instance);
			}
		}

		private bool TryReadBookmark(int index, out Vector3 pos) {
			pos = Vector3.zero;
			if (_hotkeyNavPointsField == null || _posField == null || _orthoSizeField == null)
				return false;
			var userNav = SaveGame.Instance.GetComponent<UserNavigation>();
			var list = _hotkeyNavPointsField.GetValue(userNav) as IList;
			if (list == null || index < 0 || index >= list.Count)
				return false;
			object navPoint = list[index];
			float orthoSize = (float)_orthoSizeField.GetValue(navPoint);
			if (orthoSize == 0f)
				return false;
			pos = (Vector3)_posField.GetValue(navPoint);
			return true;
		}
	}
}
