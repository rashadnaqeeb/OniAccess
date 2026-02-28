using System.Collections.Generic;


namespace OniAccess.Handlers.Notifications {
	/// <summary>
	/// Replicates NotificationScreen.OnClick behavior for a single notification.
	/// Used by both NotificationMenuHandler and NotificationSubmenuHandler to
	/// activate a notification without coupling to NotificationScreen internals.
	/// </summary>
	internal static class NotificationActivator {
		/// <summary>
		/// Activate a notification: trigger its click behavior (custom callback,
		/// camera focus, entity selection, or message dialog).
		/// </summary>
		internal static void Activate(Notification notification) {
			try {
				if (notification.customClickCallback != null) {
					notification.customClickCallback(notification.customClickData);
				} else {
					if (notification.clickFocus != null) {
						FocusCamera(notification);
					} else if (notification.Notifier != null) {
						var selectable = notification.Notifier.GetComponent<KSelectable>();
						if (selectable != null)
							SelectTool.Instance.Select(selectable);
					}
					if (notification.Type == NotificationType.Messages
						&& notification is MessageNotification mn) {
						ShowMessage(mn);
					}
				}
				if (notification.clearOnClick)
					notification.Clear();
			} catch (System.Exception ex) {
				Util.Log.Error($"NotificationActivator.Activate failed: {ex}");
			}
		}

		private static void FocusCamera(Notification notification) {
			var transform = notification.clickFocus;
			var position = transform.GetPosition();
			position.z = -40f;

			int worldId = transform.gameObject.GetMyWorldId();
			if (worldId != -1) {
				GameUtil.FocusCameraOnWorld(worldId, position);
			} else if (DlcManager.FeatureClusterSpaceEnabled()) {
				var clusterEntity = transform.GetComponent<ClusterGridEntity>();
				if (clusterEntity != null && clusterEntity.IsVisible) {
					ManagementMenu.Instance.OpenClusterMap();
					ClusterMapScreen.Instance.SetTargetFocusPosition(clusterEntity.Location);
				}
			}

			var selectable = transform.GetComponent<KSelectable>();
			if (selectable != null) {
				if (DlcManager.FeatureClusterSpaceEnabled()) {
					var clusterEntity = transform.GetComponent<ClusterGridEntity>();
					if (clusterEntity != null && clusterEntity.IsVisible)
						ClusterMapSelectTool.Instance.Select(selectable);
					else
						SelectTool.Instance.Select(selectable);
				} else {
					SelectTool.Instance.Select(selectable);
				}
			}
		}

		/// <summary>
		/// Open the message dialog for a MessageNotification, replicating
		/// NotificationScreen.ShowMessage behavior.
		/// </summary>
		private static void ShowMessage(MessageNotification mn) {
			mn.message.OnClick();
			if (!mn.message.ShowDialog()) {
				Messenger.Instance.RemoveMessage(mn.message);
				mn.Clear();
				return;
			}

			// Read dialogPrefabs from NotificationScreen to find a matching dialog
			var screen = NotificationScreen.Instance;
			if (screen == null) {
				Util.Log.Warn("NotificationActivator.ShowMessage: NotificationScreen.Instance is null");
				return;
			}

			List<MessageDialog> dialogPrefabs;
			try {
				dialogPrefabs = HarmonyLib.Traverse.Create(screen)
					.Field<List<MessageDialog>>("dialogPrefabs").Value;
			} catch (System.Exception ex) {
				Util.Log.Error($"NotificationActivator.ShowMessage: failed to read dialogPrefabs: {ex}");
				return;
			}

			if (dialogPrefabs == null) {
				Util.Log.Warn("NotificationActivator.ShowMessage: dialogPrefabs is null");
				return;
			}

			for (int i = 0; i < dialogPrefabs.Count; i++) {
				if (dialogPrefabs[i].CanDisplay(mn.message)) {
					// Destroy any existing message dialog
					var existingDialog = HarmonyLib.Traverse.Create(screen)
						.Field<MessageDialogFrame>("messageDialog").Value;
					if (existingDialog != null)
						UnityEngine.Object.Destroy(existingDialog.gameObject);

					var frame = global::Util.KInstantiateUI<MessageDialogFrame>(
						ScreenPrefabs.Instance.MessageDialogFrame.gameObject,
						GameScreenManager.Instance.ssOverlayCanvas.gameObject);
					var dialog = global::Util.KInstantiateUI<MessageDialog>(
						dialogPrefabs[i].gameObject,
						GameScreenManager.Instance.ssOverlayCanvas.gameObject);
					frame.SetMessage(dialog, mn.message);
					frame.Show();

					// Store the new dialog reference back on NotificationScreen
					HarmonyLib.Traverse.Create(screen)
						.Field<MessageDialogFrame>("messageDialog").Value = frame;
					break;
				}
			}

			Messenger.Instance.RemoveMessage(mn.message);
			mn.Clear();
		}

		/// <summary>
		/// Dismiss an entire notification group, matching NotificationScreen's
		/// dismiss button behavior. Iterates backward for safe removal.
		/// For MessageNotification, also removes from Messenger.
		/// </summary>
		internal static void DismissGroup(NotificationGroup group) {
			// Copy the members list since Clear() will mutate the tracker's list
			var members = new List<Notification>(group.Members);
			for (int i = members.Count - 1; i >= 0; i--) {
				var n = members[i];
				if (n is MessageNotification msgNotif)
					Messenger.Instance.RemoveMessage(msgNotif.message);
				n.Clear();
			}
		}
	}
}
