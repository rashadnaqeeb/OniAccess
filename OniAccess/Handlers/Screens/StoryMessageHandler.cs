using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for StoryMessageScreen, a blocking popup shown during victory
	/// sequences (ColonyAchievementTracker.BeginVictorySequence). Presents
	/// the achievement title + body as a Label and the dismiss button.
	///
	/// Title and body are set via property setters after StartScreen + Show,
	/// so DiscoverWidgets defers one frame via the _firstDiscovery pattern
	/// (same as ConfirmDialogHandler) and allows up to 3 retries.
	/// </summary>
	public class StoryMessageHandler : BaseWidgetHandler {
		private bool _firstDiscovery = true;

		public override string DisplayName =>
			(string)STRINGS.ONIACCESS.HANDLERS.STORY_MESSAGE;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		protected override int MaxDiscoveryRetries => 3;

		public StoryMessageHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override void OnActivate() {
			_firstDiscovery = true;
			base.OnActivate();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (_firstDiscovery) {
				_firstDiscovery = false;
				return false;
			}

			var traverse = Traverse.Create(screen);

			var titleLabel = traverse.Field("titleLabel").GetValue<LocText>();
			var bodyLabel = traverse.Field("bodyLabel").GetValue<LocText>();

			string title = titleLabel != null ? titleLabel.text : null;
			string body = bodyLabel != null ? bodyLabel.text : null;

			string combined = null;
			if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(body))
				combined = title + ". " + body;
			else if (!string.IsNullOrEmpty(title))
				combined = title;
			else if (!string.IsNullOrEmpty(body))
				combined = body;

			if (!string.IsNullOrEmpty(combined)) {
				_widgets.Add(new WidgetInfo {
					Label = combined,
					Component = null,
					Type = WidgetType.Label,
					GameObject = screen.gameObject
				});
			}

			var button = traverse.Field("button").GetValue<KButton>();
			if (button != null) {
				_widgets.Add(new WidgetInfo {
					Label = GetButtonLabel(button, (string)STRINGS.UI.CONFIRMDIALOG.OK),
					Component = button,
					Type = WidgetType.Button,
					GameObject = button.gameObject
				});
			}

			Util.Log.Debug($"StoryMessageHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}
	}
}
