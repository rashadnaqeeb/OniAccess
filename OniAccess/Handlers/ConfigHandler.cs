using System.Collections.Generic;
using OniAccess.Config;
using OniAccess.Handlers.Tiles;
using OniAccess.Handlers.Tiles.Scanner;
using OniAccess.Input;

namespace OniAccess.Handlers {
	public class ConfigHandler : BaseMenuHandler {
		private readonly List<ConfigItem> _items;

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.CONFIG;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry> {
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
				new HelpEntry("Enter/Left/Right", STRINGS.ONIACCESS.HELP.TOGGLE_OPTION),
				new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
				new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			}.AsReadOnly();

		public ConfigHandler() {
			_items = BuildItems();
		}

		public override int ItemCount => _items.Count;

		public override string GetItemLabel(int index) {
			if (index < 0 || index >= _items.Count) return null;
			var item = _items[index];
			return item.Label + ", " + item.GetDisplayValue();
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			if (CurrentIndex >= 0 && CurrentIndex < _items.Count)
				Speech.SpeechPipeline.SpeakInterrupt(GetItemLabel(CurrentIndex));
		}

		public override void OnActivate() {
			PlaySound("HUD_Click_Open");
			base.OnActivate();
			if (_items.Count > 0)
				Speech.SpeechPipeline.SpeakQueued(GetItemLabel(CurrentIndex));
		}

		protected override void ActivateCurrentItem() {
			CycleCurrentItem(1);
		}

		protected override void AdjustCurrentItem(int direction, int stepLevel) {
			CycleCurrentItem(direction);
		}

		private void CycleCurrentItem(int direction) {
			if (CurrentIndex < 0 || CurrentIndex >= _items.Count) return;
			var item = _items[CurrentIndex];
			item.Cycle(direction);
			PlaySound("HUD_Click");
			SpeakCurrentItem();
		}

		public override bool Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F12)
				&& !InputUtil.ShiftHeld() && !InputUtil.CtrlHeld() && !InputUtil.AltHeld()) {
				Close();
				return true;
			}
			return base.Tick();
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e))
				return true;
			if (e.TryConsume(Action.Escape)) {
				Close();
				return true;
			}
			return false;
		}

		private void Close() {
			Speech.SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.TOOLTIP.CLOSED);
			PlaySound("HUD_Click_Close");
			HandlerStack.Pop();
		}

		private static List<ConfigItem> BuildItems() {
			var items = new List<ConfigItem>();

			items.Add(new EnumConfigItem<CoordinateMode>(
				(string)STRINGS.ONIACCESS.CONFIG.COORDINATE_MODE,
				() => ConfigManager.Config.CoordinateMode,
				value => {
					ConfigManager.Config.CoordinateMode = value;
					if (TileCursor.Instance != null)
						TileCursor.Instance.Mode = value;
				},
				new[] { CoordinateMode.Off, CoordinateMode.Append, CoordinateMode.Prepend },
				mode => {
					switch (mode) {
						case CoordinateMode.Off: return (string)STRINGS.ONIACCESS.TILE_CURSOR.COORD_OFF;
						case CoordinateMode.Append: return (string)STRINGS.ONIACCESS.TILE_CURSOR.COORD_APPEND;
						case CoordinateMode.Prepend: return (string)STRINGS.ONIACCESS.TILE_CURSOR.COORD_PREPEND;
						default: return mode.ToString();
					}
				}
			));

			items.Add(new BoolConfigItem(
				(string)STRINGS.ONIACCESS.CONFIG.AUTO_MOVE_CURSOR,
				() => ConfigManager.Config.AutoMoveCursor,
				value => {
					ConfigManager.Config.AutoMoveCursor = value;
					if (ScannerNavigator.Instance != null)
						ScannerNavigator.Instance.SetAutoMove(value);
				}
			));

			items.Add(new BoolConfigItem(
				(string)STRINGS.ONIACCESS.CONFIG.LOCK_ZOOM,
				() => ConfigManager.Config.LockZoom,
				value => ConfigManager.Config.LockZoom = value
			));

			items.Add(new BoolConfigItem(
				(string)STRINGS.ONIACCESS.CONFIG.UTILITY_PRESENCE_EARCONS,
				() => ConfigManager.Config.UtilityPresenceEarcons,
				value => ConfigManager.Config.UtilityPresenceEarcons = value
			));

			items.Add(new BoolConfigItem(
				(string)STRINGS.ONIACCESS.CONFIG.PIPE_SHAPE_EARCONS,
				() => ConfigManager.Config.PipeShapeEarcons,
				value => ConfigManager.Config.PipeShapeEarcons = value
			));

			items.Add(new BoolConfigItem(
				(string)STRINGS.ONIACCESS.CONFIG.PASSABILITY_EARCONS,
				() => ConfigManager.Config.PassabilityEarcons,
				value => ConfigManager.Config.PassabilityEarcons = value
			));

			return items;
		}
	}
}
