using System.Collections.Generic;

using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Resources {
	/// <summary>
	/// Two-level resource browser backed by AllResourcesScreen.
	/// Level 0 = discovered resource categories, level 1 = resources within category.
	///
	/// Shift+P at level 1 toggles pin. Shift+C at any level clears all pins.
	/// Enter at level 1 pushes ResourceInstanceHandler.
	/// Escape at any level closes AllResourcesScreen.
	/// </summary>
	internal sealed class ResourceBrowserHandler: NestedMenuHandler {
		internal ResourceBrowserHandler(KScreen screen) : base(screen) { }

		public override string DisplayName =>
			(string)STRINGS.ONIACCESS.RESOURCES.BROWSER_TITLE;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry>(NestedNavHelpEntries) {
				new HelpEntry("Shift+P", STRINGS.ONIACCESS.RESOURCES.HELP_PIN),
				new HelpEntry("Shift+C", STRINGS.ONIACCESS.RESOURCES.HELP_CLEAR_PINS),
			}.AsReadOnly();

		protected override int MaxLevel => 1;
		protected override int SearchLevel => 1;

		// ========================================
		// ITEM COUNTS AND LABELS
		// ========================================

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0)
				return ResourceHelper.GetCategories().Count;
			var categories = ResourceHelper.GetCategories();
			if (indices[0] < 0 || indices[0] >= categories.Count) return 0;
			return ResourceHelper.GetResources(categories[indices[0]].Tag).Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			var categories = ResourceHelper.GetCategories();
			if (level == 0) {
				if (indices[0] < 0 || indices[0] >= categories.Count) return null;
				var cat = categories[indices[0]];
				return ResourceHelper.BuildCategoryLabel(cat.Tag, cat.Header);
			}
			if (indices[0] < 0 || indices[0] >= categories.Count) return null;
			var categoryTag = categories[indices[0]].Tag;
			var resources = ResourceHelper.GetResources(categoryTag);
			if (indices[1] < 0 || indices[1] >= resources.Count) return null;
			var measure = ResourceHelper.GetMeasure(categoryTag);
			return ResourceHelper.BuildResourceLabel(resources[indices[1]], measure);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level >= 1) {
				var categories = ResourceHelper.GetCategories();
				if (indices[0] >= 0 && indices[0] < categories.Count)
					return categories[indices[0]].Header.elements.LabelText.text;
			}
			return null;
		}

		// ========================================
		// LEAF ACTIVATION: push instance handler
		// ========================================

		protected override void ActivateLeafItem(int[] indices) {
			var categories = ResourceHelper.GetCategories();
			if (indices[0] < 0 || indices[0] >= categories.Count) return;
			var categoryTag = categories[indices[0]].Tag;
			var resources = ResourceHelper.GetResources(categoryTag);
			if (indices[1] < 0 || indices[1] >= resources.Count) return;

			var resourceTag = resources[indices[1]];
			var measure = ResourceHelper.GetMeasure(categoryTag);

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
			for (int i = 0; i < categories.Count; i++) {
				var resources = ResourceHelper.GetResources(categories[i].Tag);
				if (flatIndex < offset + resources.Count) {
					outIndices[0] = i;
					outIndices[1] = flatIndex - offset;
					return;
				}
				offset += resources.Count;
			}
		}

		// ========================================
		// TICK: Shift+P pin, Shift+C clear
		// ========================================

		public override bool Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.P)
				&& InputUtil.ShiftHeld()) {
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
			var categories = ResourceHelper.GetCategories();
			int catIdx = GetIndex(0);
			if (catIdx < 0 || catIdx >= categories.Count) return;
			var resources = ResourceHelper.GetResources(categories[catIdx].Tag);
			int resIdx = GetIndex(1);
			if (resIdx < 0 || resIdx >= resources.Count) return;

			var tag = resources[resIdx];
			var pinned = ClusterManager.Instance.activeWorld.worldInventory.pinnedResources;
			if (pinned.Contains(tag)) {
				pinned.Remove(tag);
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.RESOURCES.UNPINNED);
			} else {
				pinned.Add(tag);
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.RESOURCES.PINNED);
			}
		}

		private void ClearAllPins() {
			var pinned = ClusterManager.Instance.activeWorld.worldInventory.pinnedResources;
			pinned.Clear();
			SpeechPipeline.SpeakInterrupt(
				(string)STRINGS.ONIACCESS.RESOURCES.ALL_UNPINNED);
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
			if (AllResourcesScreen.Instance != null)
				AllResourcesScreen.Instance.Show(false);
		}
	}
}
