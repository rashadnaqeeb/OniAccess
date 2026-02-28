using System.Collections.Generic;

using OniAccess.Handlers.Screens;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Notifications {
	/// <summary>
	/// Handler for MessageDialogFrame — the modal dialog that opens when a
	/// Messages-type notification is clicked. Exposes title, body text,
	/// Close, Next Message, and Don't Show Again controls.
	///
	/// MessageDialogFrame extends KScreen and overrides OnActivate, so the
	/// generic KScreen_Activate_Patch fires naturally.
	/// </summary>
	internal sealed class MessageDialogFrameHandler : BaseWidgetHandler {
		private LocText _titleLocText;

		internal MessageDialogFrameHandler(KScreen screen) : base(screen) { }

		public override string DisplayName {
			get {
				if (_titleLocText != null) {
					string text = _titleLocText.text;
					if (!string.IsNullOrEmpty(text)) return text;
				}
				return (string)STRINGS.ONIACCESS.NOTIFICATIONS.MESSAGE_DIALOG;
			}
		}

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry> {
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
				new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
			}.AsReadOnly();

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			try {
				var traverse = HarmonyLib.Traverse.Create(screen);

				// Title
				_titleLocText = traverse.Field<LocText>("title").Value;

				// Body: find LocText and VideoWidget components in the body RectTransform
				var body = traverse.Field<UnityEngine.RectTransform>("body").Value;
				if (body != null) {
					var bodyTexts = body.GetComponentsInChildren<LocText>(false);
					if (bodyTexts != null) {
						for (int i = 0; i < bodyTexts.Length; i++) {
							var locText = bodyTexts[i];
							string text = locText.GetParsedText();
							if (string.IsNullOrEmpty(text)) text = locText.text;
							if (string.IsNullOrEmpty(text)) continue;
							_widgets.Add(new Widget {
								Label = text,
								Component = locText,
								GameObject = locText.gameObject,
								SpeechFunc = () => {
									string t = locText.GetParsedText();
									return !string.IsNullOrEmpty(t) ? t : locText.text;
								}
							});
						}
					}

					// TutorialMessageDialog embeds a VideoWidget with a play button
					var videoWidgets = body.GetComponentsInChildren<VideoWidget>(false);
					if (videoWidgets != null) {
						for (int i = 0; i < videoWidgets.Length; i++) {
							var videoButton = HarmonyLib.Traverse.Create(videoWidgets[i])
								.Field<KButton>("button").Value;
							if (videoButton != null) {
								_widgets.Add(new ButtonWidget {
									Label = (string)STRINGS.ONIACCESS.NOTIFICATIONS.PLAY_VIDEO,
									Component = videoButton,
									GameObject = videoButton.gameObject,
									SuppressTooltip = true
								});
							}
						}
					}
				}

				// Close button
				var closeButton = traverse.Field<KButton>("closeButton").Value;
				if (closeButton != null) {
					_widgets.Add(new ButtonWidget {
						Label = (string)STRINGS.UI.CONFIRMDIALOG.OK,
						Component = closeButton,
						GameObject = closeButton.gameObject,
						SuppressTooltip = true
					});
				}

				// Next Message button (only if visible — hidden when no more messages)
				var nextMessageButton = traverse.Field<KToggle>("nextMessageButton").Value;
				if (nextMessageButton != null && nextMessageButton.gameObject.activeInHierarchy) {
					_widgets.Add(new ButtonWidget {
						Label = (string)STRINGS.ONIACCESS.NOTIFICATIONS.NEXT_MESSAGE,
						Component = nextMessageButton,
						GameObject = nextMessageButton.gameObject,
						SuppressTooltip = true
					});
				}

				// Don't Show Again toggle (only if visible)
				var dontShowElement = traverse.Field<UnityEngine.GameObject>("dontShowAgainElement").Value;
				var dontShowButton = traverse.Field<MultiToggle>("dontShowAgainButton").Value;
				if (dontShowElement != null && dontShowElement.activeInHierarchy && dontShowButton != null) {
					_widgets.Add(new ToggleWidget {
						Label = (string)STRINGS.ONIACCESS.NOTIFICATIONS.DONT_SHOW_AGAIN,
						Component = dontShowButton,
						GameObject = dontShowButton.gameObject,
						SuppressTooltip = true
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"MessageDialogFrameHandler.DiscoverWidgets failed: {ex}");
			}

			return _widgets.Count > 0;
		}
	}
}
