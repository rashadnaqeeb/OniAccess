using System.Collections.Generic;

using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Resources {
	/// <summary>
	/// Two-level resource browser backed by AllResourcesScreen.
	/// Level 0 = discovered resource categories, level 1 = resources within category.
	///
	/// If any resources are pinned, a synthetic "Pinned" category appears at
	/// index 0 containing all pinned resources. Regular categories follow,
	/// offset by 1. If nothing is pinned the offset is 0.
	///
	/// Space at level 1 toggles pin. Shift+C at any level clears all pins.
	/// Enter at level 1 pushes ResourceInstanceHandler.
	/// Escape at any level closes AllResourcesScreen.
	/// </summary>
	internal sealed class ResourceBrowserHandler: NestedMenuHandler {
		internal ResourceBrowserHandler(KScreen screen) : base(screen) { }

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.Space),
			new ConsumedKey(KKeyCode.C, Modifier.Shift),
		};
		public override IReadOnlyList<ConsumedKey> ConsumedKeys => _consumedKeys;

		public override string DisplayName =>
			(string)STRINGS.ONIACCESS.RESOURCES.BROWSER_TITLE;

		public override void OnActivate() {
			PlayOpenSound();
			base.OnActivate();
			string first = GetItemLabel(0, new int[MaxLevel + 1]);
			if (first != null)
				SpeechPipeline.SpeakQueued(first);
		}

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry>(NestedNavHelpEntries) {
				new HelpEntry("Space", STRINGS.ONIACCESS.RESOURCES.HELP_PIN),
				new HelpEntry("Shift+C", STRINGS.ONIACCESS.RESOURCES.HELP_CLEAR_PINS),
			}.AsReadOnly();

		protected override int MaxLevel => 1;
		protected override int SearchLevel => 1;

		// ========================================
		// PINNED CATEGORY OFFSET
		// ========================================

		/// <summary>
		/// 1 when pinned resources exist (synthetic category at index 0), 0 otherwise.
		/// </summary>
		private int PinnedOffset =>
			ClusterManager.Instance.activeWorld.worldInventory.pinnedResources.Count > 0 ? 1 : 0;

		private bool IsPinnedCategory(int catIndex) =>
			PinnedOffset == 1 && catIndex == 0;

		// ========================================
		// ITEM COUNTS AND LABELS
		// ========================================

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0)
				return ResourceHelper.GetCategories().Count + PinnedOffset;

			if (IsPinnedCategory(indices[0]))
				return ResourceHelper.GetPinnedResources().Count;

			var categories = ResourceHelper.GetCategories();
			int catIdx = indices[0] - PinnedOffset;
			if (catIdx < 0 || catIdx >= categories.Count) return 0;
			return ResourceHelper.GetResources(categories[catIdx].Tag).Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (level == 0) {
				if (IsPinnedCategory(indices[0]))
					return (string)STRINGS.ONIACCESS.RESOURCES.PINNED;

				var categories = ResourceHelper.GetCategories();
				int catIdx = indices[0] - PinnedOffset;
				if (catIdx < 0 || catIdx >= categories.Count) return null;
				return ResourceHelper.BuildCategoryLabel(categories[catIdx].Tag);
			}

			if (IsPinnedCategory(indices[0])) {
				var pinned = ResourceHelper.GetPinnedResources();
				if (indices[1] < 0 || indices[1] >= pinned.Count) return null;
				var measure = ResourceHelper.GetMeasureForResource(pinned[indices[1]]);
				return ResourceHelper.BuildResourceLabel(pinned[indices[1]], measure);
			}

			var cats = ResourceHelper.GetCategories();
			int ci = indices[0] - PinnedOffset;
			if (ci < 0 || ci >= cats.Count) return null;
			var categoryTag = cats[ci].Tag;
			var resources = ResourceHelper.GetResources(categoryTag);
			if (indices[1] < 0 || indices[1] >= resources.Count) return null;
			var m = ResourceHelper.GetMeasure(categoryTag);
			return ResourceHelper.BuildResourceLabel(resources[indices[1]], m);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level >= 1) {
				if (IsPinnedCategory(indices[0]))
					return (string)STRINGS.ONIACCESS.RESOURCES.PINNED;

				var categories = ResourceHelper.GetCategories();
				int catIdx = indices[0] - PinnedOffset;
				if (catIdx >= 0 && catIdx < categories.Count)
					return categories[catIdx].Tag.ProperNameStripLink();
			}
			return null;
		}

		// ========================================
		// LEAF ACTIVATION: push instance handler
		// ========================================

		protected override void ActivateLeafItem(int[] indices) {
			Tag resourceTag;
			GameUtil.MeasureUnit measure;

			if (IsPinnedCategory(indices[0])) {
				var pinned = ResourceHelper.GetPinnedResources();
				if (indices[1] < 0 || indices[1] >= pinned.Count) return;
				resourceTag = pinned[indices[1]];
				measure = ResourceHelper.GetMeasureForResource(resourceTag);
			} else {
				var categories = ResourceHelper.GetCategories();
				int catIdx = indices[0] - PinnedOffset;
				if (catIdx < 0 || catIdx >= categories.Count) return;
				var categoryTag = categories[catIdx].Tag;
				var resources = ResourceHelper.GetResources(categoryTag);
				if (indices[1] < 0 || indices[1] >= resources.Count) return;
				resourceTag = resources[indices[1]];
				measure = ResourceHelper.GetMeasure(categoryTag);
			}

			if (ResourceHelper.GetInstances(resourceTag).Count == 0) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.RESOURCES.NO_INSTANCES);
				return;
			}

			PlayOpenSound();
			HandlerStack.Push(new ResourceInstanceHandler(resourceTag, measure));
		}

		// ========================================
		// SEARCH: flat across all resources
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			int count = 0;
			var categories = ResourceHelper.GetCategories();
			for (int i = 0; i < categories.Count; i++)
				count += ResourceHelper.GetResources(categories[i].Tag).Count;
			return count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			var categories = ResourceHelper.GetCategories();
			int offset = 0;
			for (int i = 0; i < categories.Count; i++) {
				var resources = ResourceHelper.GetResources(categories[i].Tag);
				if (flatIndex < offset + resources.Count)
					return resources[flatIndex - offset].ProperNameStripLink();
				offset += resources.Count;
			}
			return null;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			var categories = ResourceHelper.GetCategories();
			int offset = 0;
			int po = PinnedOffset;
			for (int i = 0; i < categories.Count; i++) {
				var resources = ResourceHelper.GetResources(categories[i].Tag);
				if (flatIndex < offset + resources.Count) {
					outIndices[0] = i + po;
					outIndices[1] = flatIndex - offset;
					return;
				}
				offset += resources.Count;
			}
		}

		// ========================================
		// TICK: Space pin, Shift+C clear
		// ========================================

		public override bool Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space)
				&& !InputUtil.AnyModifierHeld()) {
				TogglePin();
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.C)
				&& InputUtil.ShiftHeld()) {
				ClearAllPins();
				return true;
			}
			return base.Tick();
		}

		private void TogglePin() {
			if (Level != 1) return;

			bool inPinned = IsPinnedCategory(GetIndex(0));
			Tag tag;
			if (inPinned) {
				var pinned = ResourceHelper.GetPinnedResources();
				int resIdx = GetIndex(1);
				if (resIdx < 0 || resIdx >= pinned.Count) return;
				tag = pinned[resIdx];
			} else {
				var categories = ResourceHelper.GetCategories();
				int catIdx = GetIndex(0) - PinnedOffset;
				if (catIdx < 0 || catIdx >= categories.Count) return;
				var resources = ResourceHelper.GetResources(categories[catIdx].Tag);
				int resIdx = GetIndex(1);
				if (resIdx < 0 || resIdx >= resources.Count) return;
				tag = resources[resIdx];
			}

			var pinnedList = ClusterManager.Instance.activeWorld.worldInventory.pinnedResources;
			if (pinnedList.Contains(tag)) {
				pinnedList.Remove(tag);
				PlaySound("HUD_Click_Deselect");
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.RESOURCES.UNPINNED);

				if (inPinned) {
					var remaining = ResourceHelper.GetPinnedResources();
					if (remaining.Count == 0) {
						// Pinned category gone — drop to level 0
						Level = 0;
						SetIndex(0, 0);
						SetIndex(1, 0);
						string label = GetItemLabel(0, new int[MaxLevel + 1]);
						if (label != null)
							SpeechPipeline.SpeakQueued(label);
					} else {
						int idx = GetIndex(1);
						if (idx >= remaining.Count)
							idx = remaining.Count - 1;
						SetIndex(1, idx);
						string label = GetItemLabel(Level, new[] { GetIndex(0), idx });
						if (label != null)
							SpeechPipeline.SpeakQueued(label);
					}
				}
			} else {
				int oldOffset = PinnedOffset;
				pinnedList.Add(tag);
				PlaySound("HUD_Click");
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.RESOURCES.PINNED);

				// PinnedOffset 0→1: pinned category inserted at index 0,
				// so current category index must shift up by 1
				if (oldOffset == 0 && PinnedOffset == 1)
					SetIndex(0, GetIndex(0) + 1);
			}
		}

		private void ClearAllPins() {
			var pinned = ClusterManager.Instance.activeWorld.worldInventory.pinnedResources;
			if (pinned.Count == 0) return;

			bool wasInPinned = IsPinnedCategory(GetIndex(0));
			pinned.Clear();
			PlaySound("HUD_Click_Deselect");
			SpeechPipeline.SpeakInterrupt(
				(string)STRINGS.ONIACCESS.RESOURCES.ALL_UNPINNED);

			if (wasInPinned) {
				Level = 0;
				SetIndex(0, 0);
				SetIndex(1, 0);
				string label = GetItemLabel(0, new int[MaxLevel + 1]);
				if (label != null)
					SpeechPipeline.SpeakQueued(label);
			} else if (Level == 0) {
				// Category indices shifted down by 1, adjust
				int idx = GetIndex(0) - 1;
				if (idx < 0) idx = 0;
				SetIndex(0, idx);
			} else {
				// Level 1 in regular category: category index shifted down by 1
				SetIndex(0, GetIndex(0) - 1);
			}
		}

		// ========================================
		// ESCAPE: close AllResourcesScreen
		// ========================================

		public override bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				CloseScreen();
				return true;
			}
			return base.HandleKeyDown(e);
		}

		internal void CloseScreen() {
			PlayCloseSound();
			if (AllResourcesScreen.Instance != null)
				AllResourcesScreen.Instance.Show(false);
		}

		private static void PlaySound(string name) {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound(name));
			} catch (System.Exception ex) {
				Util.Log.Error($"ResourceBrowserHandler.PlaySound failed: {ex.Message}");
			}
		}
	}
}
