using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Util;

namespace OniAccess.Input.Handlers {
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
	public class KleiItemDropHandler : BaseMenuHandler {
		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.ITEM_DROP;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		// State tracking to detect transitions and avoid repeat announcements
		private bool _announcedItemInfo;
		private bool _announcedError;
		private string _lastAcceptButtonText;

		public KleiItemDropHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override void OnActivate() {
			_announcedItemInfo = false;
			_announcedError = false;
			_lastAcceptButtonText = null;
			base.OnActivate();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			var t = Traverse.Create(screen);

			// acceptButton — visible when acceptButtonRect is active and CanvasGroup alpha > 0.5
			try {
				var acceptButtonRect = t.Field<UnityEngine.RectTransform>("acceptButtonRect").Value;
				if (acceptButtonRect != null && acceptButtonRect.gameObject.activeInHierarchy) {
					var cg = acceptButtonRect.GetComponent<UnityEngine.CanvasGroup>();
					if (cg == null || cg.alpha > 0.5f) {
						var acceptButton = t.Field<KButton>("acceptButton").Value;
						if (acceptButton != null) {
							string label = GetButtonLabel(acceptButton, "Accept");
							_widgets.Add(new WidgetInfo {
								Label = label,
								Component = acceptButton,
								Type = WidgetType.Button,
								GameObject = acceptButton.gameObject
							});
						}
					}
				}
			} catch (System.Exception ex) {
				Log.Debug($"KleiItemDropHandler: acceptButton discovery failed: {ex.Message}");
			}

			// acknowledgeButton — visible when active and parent itemTextContainer has alpha > 0.5
			try {
				var acknowledgeButton = t.Field<KButton>("acknowledgeButton").Value;
				if (acknowledgeButton != null && acknowledgeButton.gameObject.activeInHierarchy) {
					var itemTextContainer = t.Field<UnityEngine.RectTransform>("itemTextContainer").Value;
					bool visible = true;
					if (itemTextContainer != null) {
						var cg = itemTextContainer.GetComponent<UnityEngine.CanvasGroup>();
						if (cg != null && cg.alpha <= 0.5f) visible = false;
					}
					if (visible) {
						string label = GetButtonLabel(acknowledgeButton, "OK");
						_widgets.Add(new WidgetInfo {
							Label = label,
							Component = acknowledgeButton,
							Type = WidgetType.Button,
							GameObject = acknowledgeButton.gameObject
						});
					}
				}
			} catch (System.Exception ex) {
				Log.Debug($"KleiItemDropHandler: acknowledgeButton discovery failed: {ex.Message}");
			}

			// closeButton — uses plain SetActive, no alpha fade
			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeButton", null, _widgets);

			Log.Debug($"KleiItemDropHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}

		public override void Tick() {
			var t = Traverse.Create(_screen);

			// Re-discover widgets (buttons appear/disappear via coroutines)
			int prevCount = _widgets.Count;
			DiscoverWidgets(_screen);

			// Clamp cursor index if widget count changed
			if (_widgets.Count > 0 && _currentIndex >= _widgets.Count) {
				_currentIndex = _widgets.Count - 1;
			}

			// Detect item reveal: itemNameLabel goes from empty to populated
			try {
				var itemNameLabel = t.Field<LocText>("itemNameLabel").Value;
				if (itemNameLabel != null) {
					string nameText = itemNameLabel.text;
					if (!string.IsNullOrEmpty(nameText)) {
						if (!_announcedItemInfo) {
							_announcedItemInfo = true;
							var itemRarityLabel = t.Field<LocText>("itemRarityLabel").Value;
							var itemCategoryLabel = t.Field<LocText>("itemCategoryLabel").Value;
							var itemDescriptionLabel = t.Field<LocText>("itemDescriptionLabel").Value;

							string rarity = itemRarityLabel != null ? itemRarityLabel.text : "";
							string category = itemCategoryLabel != null ? itemCategoryLabel.text : "";
							string description = itemDescriptionLabel != null ? itemDescriptionLabel.text : "";

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
				Log.Debug($"KleiItemDropHandler: item info detection failed: {ex.Message}");
			}

			// Detect error: errorMessage activated
			try {
				var errorMessage = t.Field<LocText>("errorMessage").Value;
				if (errorMessage != null) {
					if (errorMessage.gameObject.activeSelf) {
						if (!_announcedError) {
							_announcedError = true;
							string errorText = errorMessage.text;
							if (!string.IsNullOrEmpty(errorText)) {
								Speech.SpeechPipeline.SpeakQueued(errorText);
							}
						}
					} else {
						_announcedError = false;
					}
				}
			} catch (System.Exception ex) {
				Log.Debug($"KleiItemDropHandler: error detection failed: {ex.Message}");
			}

			// Detect new button availability: accept button text changes or widgets appear from zero
			try {
				var acceptButton = t.Field<KButton>("acceptButton").Value;
				if (acceptButton != null) {
					var locText = acceptButton.GetComponentInChildren<LocText>();
					string currentText = locText != null ? locText.text : null;
					if (currentText != _lastAcceptButtonText && !string.IsNullOrEmpty(currentText)) {
						// Only announce if the button is actually visible
						if (_widgets.Count > 0 && _widgets[0].Component == acceptButton) {
							Speech.SpeechPipeline.SpeakQueued(currentText);
						}
					}
					_lastAcceptButtonText = currentText;
				}
			} catch (System.Exception ex) {
				Log.Debug($"KleiItemDropHandler: accept button detection failed: {ex.Message}");
			}

			base.Tick();
		}

		/// <summary>
		/// Base check + CanvasGroup alpha > 0.5 check on widget or parent.
		/// </summary>
		protected override bool IsWidgetValid(WidgetInfo widget) {
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
