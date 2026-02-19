using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Util;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for VideoScreen (KModalScreen): victory cinematics and intro videos.
	///
	/// Two phases during victory sequences:
	/// Phase 1: Unskippable video plays. No interactive elements. Announces "Video playing".
	/// Phase 2: Victory loop. closeButton + proceedButton activate, overlay text appears.
	///          Re-discovers widgets and speaks the first one.
	///
	/// For skippable intro videos, closeButton starts active so Phase 2 triggers
	/// immediately after the Phase 1 announcement — DiscoverWidgets finds just the
	/// close button, which is the correct behavior.
	///
	/// Lifecycle: OnActivate calls Show(false) during prefab init, so a Harmony patch
	/// on VideoScreen.OnShow pushes/pops this handler.
	///
	/// Widget discovery is gated on _inVictoryLoop. During Phase 1 there are no
	/// interactive elements, and the handler is pushed mid-PlayVideo before button
	/// states are configured (Show() fires at line 151, SetActive at line 172),
	/// so discovering during Phase 1 would pick up stale prefab state. Phase 2
	/// transition is detected in Tick() by polling closeButton.activeSelf.
	/// </summary>
	public class VideoScreenHandler: BaseWidgetHandler {
		private bool _announcedPlaying;
		private bool _inVictoryLoop;

		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.VIDEO;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public VideoScreenHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override void OnActivate() {
			_announcedPlaying = false;
			_inVictoryLoop = false;
			base.OnActivate();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (!_inVictoryLoop) return true;

			var t = Traverse.Create(screen);

			// Overlay text from victory loop (victoryLoopMessage rendered via VideoOverlay prefab)
			try {
				var overlayContainer = t.Field<UnityEngine.RectTransform>("overlayContainer").Value;
				if (overlayContainer != null && overlayContainer.gameObject.activeInHierarchy) {
					var locTexts = overlayContainer.GetComponentsInChildren<LocText>();
					var parts = new List<string>();
					foreach (var lt in locTexts) {
						if (lt != null && !string.IsNullOrEmpty(lt.text))
							parts.Add(lt.text);
					}
					if (parts.Count > 0) {
						_widgets.Add(new WidgetInfo {
							Label = string.Join(". ", parts.ToArray()),
							Component = null,
							Type = WidgetType.Label,
							GameObject = overlayContainer.gameObject
						});
					}
				}
			} catch (System.Exception ex) {
				Log.Error($"VideoScreenHandler: overlay text discovery failed: {ex.Message}");
			}

			// closeButton — only when active
			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeButton", null, _widgets);

			// proceedButton — only when active
			WidgetDiscoveryUtil.TryAddButtonField(screen, "proceedButton", null, _widgets);

			Log.Debug($"VideoScreenHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}

		public override void Tick() {
			if (!_announcedPlaying) {
				_announcedPlaying = true;
				Speech.SpeechPipeline.SpeakQueued((string)STRINGS.ONIACCESS.VIDEO.PLAYING);
			}

			if (!_inVictoryLoop) {
				try {
					var closeButton = Traverse.Create(_screen).Field<KButton>("closeButton").Value;
					if (closeButton != null && closeButton.gameObject.activeSelf) {
						_inVictoryLoop = true;
						_pendingRediscovery = false;
						DiscoverWidgets(_screen);
						_currentIndex = 0;
						if (_widgets.Count > 0) {
							Speech.SpeechPipeline.SpeakQueued(_widgets[0].Label);
						}
					}
				} catch (System.Exception ex) {
					Log.Error($"VideoScreenHandler: victory loop detection failed: {ex.Message}");
				}
			} else {
				DiscoverWidgets(_screen);
				if (_widgets.Count > 0 && _currentIndex >= _widgets.Count) {
					_currentIndex = _widgets.Count - 1;
				}
			}

			base.Tick();
		}
	}
}
