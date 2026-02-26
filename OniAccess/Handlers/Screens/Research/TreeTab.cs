using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Research {
	/// <summary>
	/// Tree tab: navigable DAG of technologies using NavigableGraph.
	/// Up moves to the first prerequisite. Down moves to the first dependent.
	/// Left/Right cycles among siblings from the last Up/Down move.
	/// Enter queues the current tech for research.
	/// </summary>
	internal class TreeTab: IResearchTab {
		private readonly ResearchScreenHandler _parent;
		private readonly NavigableGraph<Tech> _graph;

		internal TreeTab(ResearchScreenHandler parent) {
			_parent = parent;
			_graph = new NavigableGraph<Tech>(
				getParents: tech => tech.requiredTech,
				getChildren: tech => (IReadOnlyList<Tech>)tech.unlockedTech);
		}

		public string TabName => (string)STRINGS.ONIACCESS.RESEARCH.TREE_TAB;

		// ========================================
		// IResearchTab
		// ========================================

		public void OnTabActivated(bool announce) {
			if (announce)
				SpeechPipeline.SpeakInterrupt(TabName);
			// Default to first root node with root sibling context
			var roots = ResearchHelper.GetRootTechs();
			if (roots.Count > 0) {
				_graph.MoveToWithSiblings(roots[0], roots);
				SpeechPipeline.SpeakQueued(ResearchHelper.BuildTechLabel(roots[0]));
			}
		}

		/// <summary>
		/// Enter the tree focused on a specific tech (from Space in Browse/Queue).
		/// No sibling context until the first Up or Down.
		/// </summary>
		internal void OnTabActivatedAt(Tech tech) {
			SpeechPipeline.SpeakInterrupt(TabName);
			_graph.MoveTo(tech);
			SpeechPipeline.SpeakQueued(ResearchHelper.BuildTechLabel(tech));
		}

		public void OnTabDeactivated() { }

		public bool HandleInput() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) {
				var node = _graph.NavigateDown();
				if (node != null) {
					PlayHoverSound();
					SpeechPipeline.SpeakInterrupt(ResearchHelper.BuildTechLabel(node));
				} else {
					SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.RESEARCH.DEAD_END);
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)) {
				var node = _graph.NavigateUp();
				if (node != null) {
					PlayHoverSound();
					SpeechPipeline.SpeakInterrupt(ResearchHelper.BuildTechLabel(node));
				} else {
					SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.RESEARCH.ROOT_NODE);
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow)) {
				var node = _graph.CycleSibling(1, out bool wrapped);
				if (node != null) {
					if (wrapped) PlayWrapSound();
					else PlayHoverSound();
					SpeechPipeline.SpeakInterrupt(ResearchHelper.BuildTechLabel(node));
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow)) {
				var node = _graph.CycleSibling(-1, out bool wrapped);
				if (node != null) {
					if (wrapped) PlayWrapSound();
					else PlayHoverSound();
					SpeechPipeline.SpeakInterrupt(ResearchHelper.BuildTechLabel(node));
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
				var tech = _graph.Current;
				if (tech != null && !tech.IsComplete()) {
					global::Research.Instance.SetActiveResearch(tech, clearQueue: true);
					SpeechPipeline.SpeakInterrupt(
						string.Format(STRINGS.ONIACCESS.RESEARCH.QUEUED, tech.Name));
				} else if (tech != null) {
					SpeechPipeline.SpeakInterrupt(ResearchHelper.BuildTechLabel(tech));
				}
				return true;
			}

			return false;
		}

		public bool HandleKeyDown(KButtonEvent e) => false;

		// ========================================
		// Sounds
		// ========================================

		static void PlayHoverSound() {
			try { KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Mouseover")); } catch (System.Exception ex) { Util.Log.Warn($"TreeTab: hover sound failed: {ex.Message}"); }
		}

		static void PlayWrapSound() {
			try { KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click")); } catch (System.Exception ex) { Util.Log.Warn($"TreeTab: wrap sound failed: {ex.Message}"); }
		}
	}
}
