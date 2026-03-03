using System.Collections.Generic;

using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Handlers.Resources {
	/// <summary>
	/// Screenless flat menu listing world instances of a resource.
	/// Pushed from ResourceBrowserHandler when Enter is pressed on a resource.
	///
	/// All data is re-queried live from ResourceHelper.GetInstances on every
	/// access (ItemCount, GetItemLabel, ActivateCurrentItem). No game state
	/// is cached.
	///
	/// Enter jumps to the instance tile and closes everything.
	/// Escape returns to the resource browser.
	/// </summary>
	internal sealed class ResourceInstanceHandler: BaseMenuHandler {
		private readonly Tag _resourceTag;
		private readonly GameUtil.MeasureUnit _measure;

		internal ResourceInstanceHandler(Tag resourceTag, GameUtil.MeasureUnit measure)
			: base(screen: null)
		{
			_resourceTag = resourceTag;
			_measure = measure;
		}

		public override string DisplayName => _resourceTag.ProperNameStripLink();

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry>(MenuHelpEntries) {
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
				new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
				new HelpEntry("Enter", STRINGS.ONIACCESS.RESOURCES.HELP_JUMP),
			}.AsReadOnly();

		public override int ItemCount => ResourceHelper.GetInstances(_resourceTag).Count;

		public override string GetItemLabel(int index) {
			var instances = ResourceHelper.GetInstances(_resourceTag);
			if (index < 0 || index >= instances.Count) return null;
			return BuildInstanceLabel(instances[index]);
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			var instances = ResourceHelper.GetInstances(_resourceTag);
			if (_currentIndex < 0 || _currentIndex >= instances.Count) return;
			string label = BuildInstanceLabel(instances[_currentIndex]);
			if (!string.IsNullOrEmpty(parentContext))
				label = parentContext + ", " + label;
			SpeechPipeline.SpeakInterrupt(label);
		}

		public override void OnActivate() {
			base.OnActivate();
			var instances = ResourceHelper.GetInstances(_resourceTag);
			if (instances.Count > 0)
				SpeechPipeline.SpeakQueued(BuildInstanceLabel(instances[0]));
		}

		protected override void ActivateCurrentItem() {
			var instances = ResourceHelper.GetInstances(_resourceTag);
			if (_currentIndex < 0 || _currentIndex >= instances.Count) return;
			int cell = instances[_currentIndex].Cell;

			// Close screen first so the show patch removes the buried browser
			// handler via RemoveByScreen (no OnActivate). Then pop this handler.
			if (AllResourcesScreen.Instance != null)
				AllResourcesScreen.Instance.Show(false);

			HandlerStack.Pop();

			if (Tiles.TileCursor.Instance != null) {
				string speech = Tiles.TileCursor.Instance.JumpTo(cell);
				if (speech != null)
					SpeechPipeline.SpeakInterrupt(speech);
			}
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				HandlerStack.Pop();
				return true;
			}
			return base.HandleKeyDown(e);
		}

		private string BuildInstanceLabel(ResourceHelper.InstanceEntry entry) {
			string amount = ResourceHelper.FormatAmount(entry.Amount, _measure);
			string coords = GridCoordinates.Format(entry.Cell);

			string buildingName = entry.Building != null
				? entry.Building.GetProperName() : null;

			if (!string.IsNullOrEmpty(buildingName))
				return string.Format(
					(string)STRINGS.ONIACCESS.RESOURCES.INSTANCE_IN_BUILDING,
					amount, buildingName, coords);
			return string.Format(
				(string)STRINGS.ONIACCESS.RESOURCES.INSTANCE_LOOSE,
				amount, coords);
		}
	}
}
