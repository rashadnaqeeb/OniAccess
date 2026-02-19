using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Speech;

namespace OniAccess.Handlers.Tools {
	/// <summary>
	/// Modal menu for selecting a tool filter/mode. Two use cases:
	/// 1. Opened from ToolHandler (F key) to change filter for the active tool.
	/// 2. Opened from ToolPickerHandler for tools that require mode-pick before
	///    activation (e.g., Harvest: pick "when ready" vs "do not harvest" first).
	/// </summary>
	public class ToolFilterHandler : BaseMenuHandler {
		private readonly ToolHandler _owner;
		private readonly ModToolInfo _pendingTool;
		private List<string> _filterKeys;
		private List<string> _filterNames;

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

		public override int ItemCount => _filterKeys != null ? _filterKeys.Count : 0;

		public override string GetItemLabel(int index) {
			if (_filterNames == null || index < 0 || index >= _filterNames.Count) return null;
			return _filterNames[index];
		}

		public override void SpeakCurrentItem() {
			if (_filterNames != null && _currentIndex >= 0 && _currentIndex < _filterNames.Count)
				SpeechPipeline.SpeakInterrupt(_filterNames[_currentIndex]);
		}

		public override void OnActivate() {
			PlaySound("HUD_Click_Open");
			_filterKeys = new List<string>();
			_filterNames = new List<string>();
			_currentIndex = 0;
			_search.Clear();

			var menuTraverse = Traverse.Create(ToolMenu.Instance.toolParameterMenu);
			var parameters = menuTraverse
				.Field<Dictionary<string, ToolParameterMenu.ToggleState>>("currentParameters")
				.Value;

			if (parameters == null && _pendingTool != null
				&& _pendingTool.ToolType == typeof(HarvestTool)) {
				_filterKeys.Add("HARVEST_WHEN_READY");
				_filterKeys.Add("DO_NOT_HARVEST");
				for (int i = 0; i < _filterKeys.Count; i++)
					_filterNames.Add(Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS." + _filterKeys[i] + ".NAME"));
			} else if (parameters != null) {
				int onIndex = 0;
				int idx = 0;
				foreach (var kv in parameters) {
					if (kv.Value == ToolParameterMenu.ToggleState.Disabled)
						continue;
					_filterKeys.Add(kv.Key);
					_filterNames.Add(Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS." + kv.Key + ".NAME"));
					if (kv.Value == ToolParameterMenu.ToggleState.On)
						onIndex = idx;
					idx++;
				}
				_currentIndex = onIndex;
			}

			if (_filterNames.Count > 0) {
				SpeechPipeline.SpeakInterrupt(_filterNames[_currentIndex]);
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

		private static System.Reflection.MethodInfo _changeToSettingMethod;
		private static System.Reflection.MethodInfo _onChangeMethod;

		protected override void ActivateCurrentItem() {
			if (_filterKeys == null || _currentIndex < 0 || _currentIndex >= _filterKeys.Count)
				return;

			if (_pendingTool != null)
				ToolPickerHandler.ActivateTool(_pendingTool);

			try {
				var menu = ToolMenu.Instance.toolParameterMenu;
				if (_changeToSettingMethod == null)
					_changeToSettingMethod = AccessTools.Method(typeof(ToolParameterMenu), "ChangeToSetting");
				if (_onChangeMethod == null)
					_onChangeMethod = AccessTools.Method(typeof(ToolParameterMenu), "OnChange");
				_changeToSettingMethod.Invoke(menu, new object[] { _filterKeys[_currentIndex] });
				_onChangeMethod.Invoke(menu, null);
			} catch (System.Exception ex) {
				Util.Log.Error($"ToolFilterHandler.ActivateCurrentItem: filter apply failed: {ex}");
			}

			if (_owner != null) {
				bool hadSelection = _owner.HasSelection;
				_owner.ClearSelection();
				string announcement = _filterNames[_currentIndex];
				if (hadSelection)
					announcement += ", " + (string)STRINGS.ONIACCESS.TOOLS.SELECTION_CLEARED;
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

		private static void PlaySound(string name) {
			ToolPickerHandler.PlaySound(name);
		}
	}
}
