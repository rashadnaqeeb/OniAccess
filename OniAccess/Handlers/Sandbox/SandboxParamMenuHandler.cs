using System.Collections.Generic;
using OniAccess.Handlers.Screens;
using OniAccess.Speech;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Sandbox {
	/// <summary>
	/// Modal handler for the sandbox parameter menu. Pushed by F key
	/// from SandboxToolHandler. Lists visible sliders and selectors
	/// from SandboxToolParameterMenu. Sliders adjust with Left/Right;
	/// selectors open a SandboxSelectorHandler on Enter.
	/// </summary>
	public class SandboxParamMenuHandler : BaseWidgetHandler {
		public override string DisplayName => (string)STRINGS.ONIACCESS.SANDBOX.PARAM_MENU;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries;

		static SandboxParamMenuHandler() {
			var list = new List<HelpEntry>();
			list.AddRange(MenuHelpEntries);
			list.AddRange(ListNavHelpEntries);
			list.Add(new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE));
			_helpEntries = list.AsReadOnly();
		}

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public SandboxParamMenuHandler() : base(null) { }

		// ========================================
		// WIDGET DISCOVERY
		// ========================================

		/// <summary>
		/// All parameters in spawn order (matching the game's OnSpawn layout).
		/// Selectors and sliders interleaved as they appear in the UI.
		/// </summary>
		private struct ParamEntry {
			public bool IsSelector;
			public SandboxToolParameterMenu.SelectorValue Selector;
			public SandboxToolParameterMenu.SliderValue Slider;
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();
			var menu = SandboxToolParameterMenu.instance;
			if (menu == null) return false;

			// Enumerate in the game's spawn order (OnSpawn lines 405-417)
			var entries = new ParamEntry[] {
				Sel(menu.entitySelector),
				Sel(menu.elementSelector),
				Sel(menu.storySelector),
				Sld(menu.brushRadiusSlider),
				Sld(menu.noiseScaleSlider),
				Sld(menu.noiseDensitySlider),
				Sld(menu.massSlider),
				Sld(menu.temperatureSlider),
				Sld(menu.temperatureAdditiveSlider),
				Sld(menu.stressAdditiveSlider),
				Sel(menu.diseaseSelector),
				Sld(menu.diseaseCountSlider),
				Sld(menu.moraleSlider),
			};

			foreach (var e in entries) {
				if (e.IsSelector) {
					if (e.Selector.row == null || !e.Selector.row.activeSelf) continue;
					_widgets.Add(new ButtonWidget {
						Label = e.Selector.labelText,
						Component = e.Selector.button,
						GameObject = e.Selector.row,
						Tag = e.Selector,
						SpeechFunc = () => ReadSelectorValue(e.Selector),
					});
				} else {
					if (e.Slider.row == null || !e.Slider.row.activeSelf) continue;
					if (e.Slider.slider == null) continue;
					_widgets.Add(new SliderWidget {
						Label = e.Slider.labelText,
						Component = e.Slider.slider,
						GameObject = e.Slider.row,
						SuppressTooltip = true,
					});
				}
			}

			return _widgets.Count > 0;
		}

		private static ParamEntry Sel(SandboxToolParameterMenu.SelectorValue sel) =>
			new ParamEntry { IsSelector = true, Selector = sel };

		private static ParamEntry Sld(SandboxToolParameterMenu.SliderValue sld) =>
			new ParamEntry { IsSelector = false, Slider = sld };

		/// <summary>
		/// Read the current selection text from the selector button's child LocText.
		/// </summary>
		private static string ReadSelectorValue(SandboxToolParameterMenu.SelectorValue sel) {
			if (sel.button == null) return sel.labelText;
			var locText = sel.button.GetComponentInChildren<LocText>();
			if (locText == null) return sel.labelText;
			string value = TextFilter.FilterForSpeech(locText.text);
			if (string.IsNullOrEmpty(value)) return sel.labelText;
			return $"{sel.labelText}, {value}";
		}

		// ========================================
		// INTERACTION
		// ========================================

		protected override void ActivateCurrentItem() {
			if (CurrentIndex < 0 || CurrentIndex >= _widgets.Count) return;
			var widget = _widgets[CurrentIndex];

			// Selector widgets: push the selector handler
			if (widget.Tag is SandboxToolParameterMenu.SelectorValue sel) {
				HandlerStack.Push(new SandboxSelectorHandler(sel));
				return;
			}

			// Sliders: no Enter action
		}

		// ========================================
		// ESCAPE
		// ========================================

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e))
				return true;
			if (e.TryConsume(Action.Escape)) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLTIP.CLOSED);
				HandlerStack.Pop();
				return true;
			}
			return false;
		}

		// ========================================
		// REDISCOVERY
		// ========================================

		/// <summary>
		/// Called from Harmony postfix on SandboxToolParameterMenu.RefreshDisplay
		/// to trigger widget rediscovery when the active tool changes.
		/// </summary>
		internal static void OnRefreshDisplay() {
			if (HandlerStack.ActiveHandler is SandboxParamMenuHandler handler)
				handler._pendingRediscovery = true;
		}
	}
}
