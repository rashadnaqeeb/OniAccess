using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Util;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for KleiItemDropScreen: cosmetic item reveal triggered from Supply Closet.
	///
	/// This is a coroutine-driven sequential presentation — an animated pod reveals items
	/// one at a time, with buttons fading in/out via CanvasGroup alpha at each stage.
	/// At any moment there are at most 2-3 buttons visible.
	///
	/// Tick() polls for state changes (item reveal, error, button availability) since the
	/// coroutines animate elements asynchronously.
	///
	/// Lifecycle note: Like LockerMenuScreen, OnActivate() calls Show(false) during prefab
	/// init, so a Harmony patch on KleiItemDropScreen.Show pushes/pops this handler.
	/// </summary>
	public class KleiItemDropHandler: BaseWidgetHandler {
		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.ITEM_DROP;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		// State tracking to detect transitions and avoid repeat announcements
		private bool _announcedItemInfo;
		private bool _announcedError;
		private string _lastAcceptButtonText;

		// Cached component references resolved once in OnActivate via Traverse.
		// These are live Unity component refs (allowed by caching rules);
		// visibility/content is always read fresh from the components themselves.
		private UnityEngine.RectTransform _acceptButtonRect;
		private KButton _acceptButton;
		private KButton _acknowledgeButton;
		private UnityEngine.RectTransform _itemTextContainer;
		private LocText _itemNameLabel;
		private LocText _itemRarityLabel;
		private LocText _itemCategoryLabel;
		private LocText _itemDescriptionLabel;
		private LocText _errorMessage;
		private KButton _closeButton;

		public KleiItemDropHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override void OnActivate() {
			_announcedItemInfo = false;
			_announcedError = false;
			_lastAcceptButtonText = null;

			var t = Traverse.Create(_screen);
			_acceptButtonRect = t.Field<UnityEngine.RectTransform>("acceptButtonRect").Value;
			_acceptButton = t.Field<KButton>("acceptButton").Value;
			_acknowledgeButton = t.Field<KButton>("acknowledgeButton").Value;
			_itemTextContainer = t.Field<UnityEngine.RectTransform>("itemTextContainer").Value;
			_itemNameLabel = t.Field<LocText>("itemNameLabel").Value;
			_itemRarityLabel = t.Field<LocText>("itemRarityLabel").Value;
			_itemCategoryLabel = t.Field<LocText>("itemCategoryLabel").Value;
			_itemDescriptionLabel = t.Field<LocText>("itemDescriptionLabel").Value;
			_errorMessage = t.Field<LocText>("errorMessage").Value;
			_closeButton = t.Field<KButton>("closeButton").Value;

			base.OnActivate();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			// acceptButton — visible when acceptButtonRect is active and CanvasGroup alpha > 0.5
			try {
				if (_acceptButtonRect != null && _acceptButtonRect.gameObject.activeInHierarchy) {
					var cg = _acceptButtonRect.GetComponent<UnityEngine.CanvasGroup>();
					if (cg == null || cg.alpha > 0.5f) {
						if (_acceptButton != null) {
							string label = GetButtonLabel(_acceptButton, (string)STRINGS.ONIACCESS.BUTTONS.ACCEPT);
							_widgets.Add(new ButtonWidget {
								Label = label,
								Component = _acceptButton,
								GameObject = _acceptButton.gameObject
							});
						}
					}
				}
			} catch (System.Exception ex) {
				Log.Error($"KleiItemDropHandler: acceptButton discovery failed: {ex.Message}");
			}

			// acknowledgeButton — visible when active and parent itemTextContainer has alpha > 0.5
			try {
				if (_acknowledgeButton != null && _acknowledgeButton.gameObject.activeInHierarchy) {
					bool visible = true;
					if (_itemTextContainer != null) {
						var cg = _itemTextContainer.GetComponent<UnityEngine.CanvasGroup>();
						if (cg != null && cg.alpha <= 0.5f) visible = false;
					}
					if (visible) {
						string label = GetButtonLabel(_acknowledgeButton, (string)STRINGS.UI.CONFIRMDIALOG.OK);
						_widgets.Add(new ButtonWidget {
							Label = label,
							Component = _acknowledgeButton,
							GameObject = _acknowledgeButton.gameObject
						});
					}
				}
			} catch (System.Exception ex) {
				Log.Error($"KleiItemDropHandler: acknowledgeButton discovery failed: {ex.Message}");
			}

			// closeButton — uses plain SetActive, no alpha fade
			if (_closeButton != null && _closeButton.gameObject.activeInHierarchy) {
				string label = null;
				var locText = _closeButton.GetComponentInChildren<LocText>();
				if (locText != null && !string.IsNullOrEmpty(locText.text))
					label = locText.text;
				_widgets.Add(new ButtonWidget {
					Label = label,
					Component = _closeButton,
					GameObject = _closeButton.gameObject
				});
			}

			Log.Debug($"KleiItemDropHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}

		public override bool Tick() {
			// Re-discover widgets (buttons appear/disappear via coroutines)
			int prevCount = _widgets.Count;
			DiscoverWidgets(_screen);

			// Clamp cursor index if widget count changed
			if (_widgets.Count > 0 && _currentIndex >= _widgets.Count) {
				_currentIndex = _widgets.Count - 1;
			}

			// Detect item reveal: itemNameLabel goes from empty to populated
			try {
				if (_itemNameLabel != null) {
					string nameText = _itemNameLabel.text;
					if (!string.IsNullOrEmpty(nameText)) {
						if (!_announcedItemInfo) {
							_announcedItemInfo = true;

							string rarity = _itemRarityLabel != null ? _itemRarityLabel.text : "";
							string category = _itemCategoryLabel != null ? _itemCategoryLabel.text : "";
							string description = _itemDescriptionLabel != null ? _itemDescriptionLabel.text : "";

							var parts = new List<string>();
							if (!string.IsNullOrEmpty(rarity)) parts.Add(rarity);
							if (!string.IsNullOrEmpty(category)) parts.Add(category);
							parts.Add(nameText);
							if (!string.IsNullOrEmpty(description)) parts.Add(description);

							Speech.SpeechPipeline.SpeakQueued(string.Join(", ", parts.ToArray()));
						}
					} else {
						// Labels cleared — reset for next item reveal
						_announcedItemInfo = false;
					}
				}
			} catch (System.Exception ex) {
				Log.Error($"KleiItemDropHandler: item info detection failed: {ex.Message}");
			}

			// Detect error: errorMessage activated
			try {
				if (_errorMessage != null) {
					if (_errorMessage.gameObject.activeSelf) {
						if (!_announcedError) {
							_announcedError = true;
							string errorText = _errorMessage.text;
							if (!string.IsNullOrEmpty(errorText)) {
								Speech.SpeechPipeline.SpeakQueued(errorText);
							}
						}
					} else {
						_announcedError = false;
					}
				}
			} catch (System.Exception ex) {
				Log.Error($"KleiItemDropHandler: error detection failed: {ex.Message}");
			}

			// Detect new button availability: accept button text changes or widgets appear from zero
			try {
				if (_acceptButton != null) {
					var locText = _acceptButton.GetComponentInChildren<LocText>();
					string currentText = locText != null ? locText.text : null;
					if (currentText != _lastAcceptButtonText && !string.IsNullOrEmpty(currentText)) {
						// Only announce if the button is actually visible
						if (_widgets.Count > 0 && _widgets[0].Component == _acceptButton) {
							Speech.SpeechPipeline.SpeakQueued(currentText);
						}
					}
					_lastAcceptButtonText = currentText;
				}
			} catch (System.Exception ex) {
				Log.Error($"KleiItemDropHandler: accept button detection failed: {ex.Message}");
			}

			return base.Tick();
		}

		/// <summary>
		/// Base check + CanvasGroup alpha > 0.5 check on widget or parent.
		/// </summary>
		protected override bool IsWidgetValid(Widget widget) {
			if (widget == null || widget.GameObject == null) return false;
			if (!widget.GameObject.activeInHierarchy) return false;

			// Check CanvasGroup alpha on the widget or its parent
			var cg = widget.GameObject.GetComponent<UnityEngine.CanvasGroup>();
			if (cg != null && cg.alpha <= 0.5f) return false;

			var parentCg = widget.GameObject.GetComponentInParent<UnityEngine.CanvasGroup>();
			if (parentCg != null && parentCg.alpha <= 0.5f) return false;

			return base.IsWidgetValid(widget);
		}
	}
}
