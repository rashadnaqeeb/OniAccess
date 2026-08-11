using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Speech;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Tools {
	/// <summary>
	/// Modal menu for selecting a tool filter/mode. Two use cases:
	/// 1. Opened from ToolHandler (F key) to change filter for the active tool.
	/// 2. Opened from ToolPickerHandler for tools that require mode-pick before
	///    activation (e.g., Harvest: pick "when ready" vs "do not harvest" first).
	/// </summary>
	public class ToolFilterHandler: BaseMenuHandler {
		internal const string HarvestWhenReadyKey = "HARVEST_WHEN_READY";
		internal const string DoNotHarvestKey = "DO_NOT_HARVEST";

		private readonly ToolHandler _owner;
		private readonly ModToolInfo _pendingTool;
		private List<string> _filterNames;
		// Live ToggleData objects owned by the tool, parallel to _filterKeys.
		private List<ToolParameterMenu.ToggleData> _toggles;

		public override string DisplayName => (string)STRINGS.ONIACCESS.TOOLS.FILTER_NAME;

		public override IReadOnlyList<HelpEntry> HelpEntries => ToolPickerHandler.ModalMenuHelp;

		/// <summary>
		/// Change filter for an active tool (F key in tool mode).
		/// </summary>
		public ToolFilterHandler(ToolHandler owner) {
			_owner = owner;
			_pendingTool = null;
		}

		/// <summary>
		/// Pick mode before activating a tool (e.g., Harvest from tool picker).
		/// </summary>
		public ToolFilterHandler(ModToolInfo pendingTool) {
			_owner = null;
			_pendingTool = pendingTool;
		}

		public override int ItemCount => _filterNames != null ? _filterNames.Count : 0;

		public override string GetItemLabel(int index) {
			if (_filterNames == null || index < 0 || index >= _filterNames.Count) return null;
			return _filterNames[index];
		}

		protected override string GetReviewItemText() {
			if (_filterNames == null || CurrentIndex < 0 || CurrentIndex >= _filterNames.Count)
				return null;
			return ItemSpeech(CurrentIndex);
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			if (_filterNames != null && CurrentIndex >= 0 && CurrentIndex < _filterNames.Count)
				SpeechPipeline.SpeakInterrupt(ComposeItem(GetReviewItemText(), CurrentIndex, RoleForItem(CurrentIndex)));
		}

		// Checkbox (inclusive) filters are on/off toggles; the others are a
		// one-of-several selection spoken as radio buttons.
		private string RoleForItem(int index) {
			return _toggles[index].isToggleInclusive ? Widgets.NavRoles.Toggle : Widgets.NavRoles.Radio;
		}

		/// <summary>
		/// Spoken form of an item: checkbox filters (dig tool) append their
		/// on/off state; radio filters are just the name.
		/// </summary>
		private string ItemSpeech(int index) {
			string name = _filterNames[index];
			var toggle = _toggles[index];
			// Radio filters: mark the one currently chosen. Spoken in both modes; in
			// verbose mode it precedes the appended "radio button" role since it is part
			// of the body.
			if (!toggle.isToggleInclusive)
				return toggle.IsOn ? name + ", " + (string)STRINGS.ONIACCESS.STATES.SELECTED : name;
			string state = toggle.IsOn
				? (string)STRINGS.ONIACCESS.STATES.ON
				: (string)STRINGS.ONIACCESS.STATES.OFF;
			return name + ", " + state;
		}

		private static ToolParameterMenu.ToggleData[] ReadMenuToggles() {
			var toggles = Traverse.Create(ToolMenu.Instance.toolParameterMenu)
				.Field<ToolParameterMenu.ToggleData[]>("currentTogglesData").Value;
			return toggles != null && toggles.Length > 0 ? toggles : null;
		}

		/// <summary>
		/// Toggles owned by a tool that has not been activated yet. The shared
		/// parameter menu still holds whichever tool ran last (ClearMenu leaves
		/// currentTogglesData behind), so it cannot be trusted before activation.
		/// </summary>
		private ToolParameterMenu.ToggleData[] ReadPendingToolToggles() {
			if (_pendingTool.ToolType != typeof(HarvestTool)) {
				Util.Log.Warn($"ToolFilterHandler: no mode source for pending tool {_pendingTool.ToolName}");
				return null;
			}
			return Traverse.Create(HarvestTool.Instance)
				.Field<ToolParameterMenu.ToggleData[]>("options").Value;
		}

		public override void OnActivate() {
			PlaySound("HUD_Click_Open");
			_filterNames = new List<string>();
			_toggles = null;
			CurrentIndex = 0;
			_search.Clear();

			var toggles = _pendingTool != null ? ReadPendingToolToggles() : ReadMenuToggles();

			if (toggles != null) {
				_toggles = new List<ToolParameterMenu.ToggleData>();
				int onIndex = 0;
				int idx = 0;
				foreach (var toggle in toggles) {
					if (toggle.state == ToolParameterMenu.ToggleState.Disabled)
						continue;
					_toggles.Add(toggle);
					_filterNames.Add(Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS." + toggle.name + ".NAME"));
					if (toggle.state == ToolParameterMenu.ToggleState.On)
						onIndex = idx;
					idx++;
				}
				CurrentIndex = onIndex;
			}

			if (_filterNames.Count > 0) {
				SpeechPipeline.SpeakInterrupt(ComposeItem(ItemSpeech(CurrentIndex), CurrentIndex, RoleForItem(CurrentIndex)));
			} else {
				Util.Log.Warn("ToolFilterHandler.OnActivate: no filter parameters available");
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLTIP.CLOSED);
				HandlerStack.Pop();
			}
		}

		public override void OnDeactivate() {
			PlaySound("HUD_Click_Close");
			base.OnDeactivate();
		}

		private static System.Reflection.MethodInfo _onChangeMethod;

		protected override void ActivateCurrentItem() {
			if (_filterNames == null || CurrentIndex < 0 || CurrentIndex >= _filterNames.Count)
				return;

			// Activating a pending tool pops this menu off the handler stack, and
			// deactivation resets CurrentIndex, so resolve the choice up front.
			int index = CurrentIndex;
			var clicked = _toggles[index];

			if (_pendingTool != null)
				ToolPickerHandler.ActivateTool(_pendingTool);

			// Mirrors the game's ToolParameterMenu.ChangeToSetting, which now
			// takes a private Widget and can't be invoked with a filter key.
			// The ToggleData objects are shared with the tool's currentFilters,
			// so mutating them and firing OnChange updates both the menu
			// visuals and the tool.
			if (clicked.isToggleInclusive) {
				clicked.state = clicked.IsOn
					? ToolParameterMenu.ToggleState.Off
					: ToolParameterMenu.ToggleState.On;
			} else {
				foreach (var toggle in _toggles)
					toggle.state = ToolParameterMenu.ToggleState.Off;
				clicked.state = ToolParameterMenu.ToggleState.On;
			}

			try {
				if (_onChangeMethod == null)
					_onChangeMethod = AccessTools.Method(typeof(ToolParameterMenu), "OnChange");
				_onChangeMethod.Invoke(ToolMenu.Instance.toolParameterMenu, null);
			} catch (System.Exception ex) {
				Util.Log.Error($"ToolFilterHandler.ActivateCurrentItem: filter apply failed: {ex}");
			}

			bool hadSelection = _owner != null && _owner.HasSelection;
			if (_owner != null)
				_owner.ClearSelection();

			bool isInclusive = clicked.isToggleInclusive;
			string announcement = isInclusive ? ItemSpeech(index) : _filterNames[index];
			if (hadSelection)
				announcement += ", " + (string)STRINGS.ONIACCESS.TOOLS.SELECTION_CLEARED;

			if (isInclusive) {
				// Checkbox menu: stay open so more filters can be toggled
				SpeechPipeline.SpeakInterrupt(announcement);
			} else if (_owner != null) {
				SpeechPipeline.SpeakInterrupt(announcement);
				HandlerStack.Pop();
			} else {
				HandlerStack.Replace(new ToolHandler());
			}
		}

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
	}
}
