using System.Collections.Generic;

using OniAccess.Handlers.Screens;
using OniAccess.Handlers.Screens.Codex;
using OniAccess.Speech;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Notifications {
	/// <summary>
	/// Handler for MessageDialogFrame — the modal dialog that opens when a
	/// Messages-type notification is clicked. Exposes title, body text,
	/// Close, Next Message, and Don't Show Again controls.
	///
	/// Body text widgets with inline codex links (FormatAsLink) support
	/// Enter to follow the link (single) or open a link chooser (multiple).
	///
	/// MessageDialogFrame extends KScreen and overrides OnActivate, so the
	/// generic KScreen_Activate_Patch fires naturally.
	/// </summary>
	internal sealed class MessageDialogFrameHandler : BaseWidgetHandler {
		private LocText _titleLocText;
		private readonly Dictionary<int, List<(string id, string text)>> _widgetLinks
			= new Dictionary<int, List<(string id, string text)>>();

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
			_widgetLinks.Clear();

			try {
				var traverse = HarmonyLib.Traverse.Create(screen);

				// Title
				_titleLocText = traverse.Field<LocText>("title").Value;

				// Body: find LocText components in the body RectTransform
				var body = traverse.Field<UnityEngine.RectTransform>("body").Value;
				if (body != null) {
					var bodyTexts = body.GetComponentsInChildren<LocText>(false);
					if (bodyTexts != null) {
						for (int i = 0; i < bodyTexts.Length; i++) {
							var locText = bodyTexts[i];
							string text = locText.GetParsedText();
							if (string.IsNullOrEmpty(text)) text = locText.text;
							if (string.IsNullOrEmpty(text)) continue;

							int widgetIndex = _widgets.Count;
							_widgets.Add(new Widget {
								Label = text,
								Component = locText,
								GameObject = locText.gameObject,
								SpeechFunc = () => {
									string t = locText.GetParsedText();
									return !string.IsNullOrEmpty(t) ? t : locText.text;
								}
							});

							// Extract inline codex links from the raw markup
							string rawText = locText.text;
							var links = CodexHelper.ExtractTextLinks(rawText);
							if (links.Count > 0)
								_widgetLinks[widgetIndex] = links;
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

		protected override void ActivateCurrentItem() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;

			// Text widgets with codex links: follow link or open chooser
			if (_widgetLinks.TryGetValue(_currentIndex, out var links)) {
				if (links.Count == 1) {
					FollowLink(links[0].id);
					return;
				}
				HandlerStack.Push(new MessageLinkMenuHandler(links));
				return;
			}

			// Buttons and toggles: default dispatch
			base.ActivateCurrentItem();
		}

		private static void FollowLink(string entryId) {
			if (ManagementMenu.Instance == null) return;
			PlayOpenSound();
			ManagementMenu.Instance.OpenCodexToEntry(entryId);
		}

		/// <summary>
		/// Popup menu for choosing between multiple inline codex links.
		/// </summary>
		private sealed class MessageLinkMenuHandler : BaseMenuHandler {
			private readonly List<(string id, string text)> _links;

			internal MessageLinkMenuHandler(List<(string id, string text)> links) : base(screen: null) {
				_links = links;
			}

			public override string DisplayName => string.Format(
				STRINGS.ONIACCESS.CODEX.LINK_MENU, _links.Count);

			public override IReadOnlyList<HelpEntry> HelpEntries { get; }
				= new List<HelpEntry> {
					new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
					new HelpEntry("Enter", STRINGS.ONIACCESS.CODEX.FOLLOW_LINK_HELP),
				}.AsReadOnly();

			public override int ItemCount => _links.Count;

			public override string GetItemLabel(int index) {
				if (index < 0 || index >= _links.Count) return null;
				return _links[index].text;
			}

			public override void SpeakCurrentItem(string parentContext = null) {
				if (_currentIndex < 0 || _currentIndex >= _links.Count) return;
				string text = _links[_currentIndex].text;
				if (!string.IsNullOrEmpty(parentContext))
					text = parentContext + ", " + text;
				SpeechPipeline.SpeakInterrupt(text);
			}

			protected override void ActivateCurrentItem() {
				if (_currentIndex < 0 || _currentIndex >= _links.Count) return;
				string linkId = _links[_currentIndex].id;
				HandlerStack.Pop();
				FollowLink(linkId);
			}

			public override bool HandleKeyDown(KButtonEvent e) {
				if (e.TryConsume(Action.Escape)) {
					HandlerStack.Pop();
					SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.TOOLTIP.CLOSED);
					return true;
				}
				return base.HandleKeyDown(e);
			}
		}
	}
}
