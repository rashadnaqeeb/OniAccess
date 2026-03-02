using HarmonyLib;
using OniAccess.Speech;

namespace OniAccess.Patches {
	/// <summary>
	/// Announce newly discovered resources via speech.
	/// DiscoveredResources.Discover(Tag, Tag) is the two-arg overload that
	/// all discovery paths funnel through. The method itself guards new
	/// discoveries with Discovered.Add returning true, and fires OnDiscover
	/// only for genuinely new tags. We use a prefix to capture whether the
	/// tag was already known, and only announce in the postfix if it was new.
	/// </summary>
	[HarmonyPatch(typeof(DiscoveredResources), nameof(DiscoveredResources.Discover),
		new[] { typeof(Tag), typeof(Tag) })]
	internal static class DiscoveredResources_Discover_Patch {
		private static bool _wasNew;

		private static void Prefix(DiscoveredResources __instance, Tag tag) {
			_wasNew = !__instance.GetDiscovered().Contains(tag);
		}

		private static void Postfix(Tag tag) {
			if (!_wasNew) return;
			if (!ModToggle.IsEnabled) return;
			string name = tag.ProperNameStripLink();
			SpeechPipeline.SpeakQueued(string.Format(
				(string)STRINGS.ONIACCESS.RESOURCES.DISCOVERED, name));
		}
	}
}
