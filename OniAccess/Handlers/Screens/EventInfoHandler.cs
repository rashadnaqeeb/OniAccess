using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for EventInfoScreen, the popup used by story trait discovery/completion
	/// and gameplay events (meteor showers, food fights, etc.). Reads the title,
	/// description, optional location/time, and option buttons.
	///
	/// SetEventData runs after StartScreen returns, so content is empty on the first
	/// DiscoverWidgets call. Uses the _firstDiscovery deferral pattern.
	/// </summary>
	public class EventInfoHandler : BaseWidgetHandler {
		private bool _firstDiscovery = true;
		private string _title;

		public override string DisplayName => _title
			?? (string)STRINGS.ONIACCESS.HANDLERS.EVENT_INFO;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		protected override int MaxDiscoveryRetries => 3;

		public EventInfoHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override void OnActivate() {
			_firstDiscovery = true;
			_title = null;
			base.OnActivate();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (_firstDiscovery) {
				_firstDiscovery = false;
				return false;
			}

			var traverse = Traverse.Create(screen);

			var headerLabel = traverse.Field("eventHeader").GetValue<LocText>();
			var descLabel = traverse.Field("eventDescriptionLabel").GetValue<LocText>();
			var locationLabel = traverse.Field("eventLocationLabel").GetValue<LocText>();
			var timeLabel = traverse.Field("eventTimeLabel").GetValue<LocText>();

			string header = headerLabel.text;
			string desc = descLabel.text;
			string location = locationLabel.gameObject.activeInHierarchy
				? locationLabel.text : null;
			string time = timeLabel.gameObject.activeInHierarchy
				? timeLabel.text : null;

			if (!string.IsNullOrEmpty(header))
				_title = header;

			var parts = new List<string>();
			if (!string.IsNullOrEmpty(header))
				parts.Add(header);
			if (!string.IsNullOrEmpty(desc))
				parts.Add(desc);
			if (!string.IsNullOrEmpty(location))
				parts.Add(location);
			if (!string.IsNullOrEmpty(time))
				parts.Add(time);

			if (parts.Count > 0) {
				_widgets.Add(new LabelWidget {
					Label = string.Join(". ", parts),
					GameObject = screen.gameObject
				});
			}

			var buttonsGroup = traverse.Field("buttonsGroup")
				.GetValue<UnityEngine.GameObject>();
			if (buttonsGroup != null) {
				var kbuttons = buttonsGroup.GetComponentsInChildren<KButton>(false);
				foreach (var kb in kbuttons) {
					if (!kb.gameObject.activeInHierarchy
						|| !kb.isInteractable) continue;

					string label = GetButtonLabel(kb, null);
					if (string.IsNullOrEmpty(label)) continue;

					_widgets.Add(new ButtonWidget {
						Label = label,
						Component = kb,
						GameObject = kb.gameObject
					});
				}
			}

			Util.Log.Debug($"EventInfoHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}
	}
}
