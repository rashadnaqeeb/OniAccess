using System.Collections.Generic;

using Database;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Skills {
	/// <summary>
	/// Tab 3: navigable DAG of skills using NavigableGraph.
	/// Up moves to the first prerequisite. Down moves to the first dependent.
	/// Left/Right cycles among siblings from the last Up/Down move.
	/// Enter learns the current skill.
	/// </summary>
	internal class TreeTab: ISkillsTab {
		private readonly SkillsScreenHandler _parent;
		private NavigableGraph<Skill> _graph;
		private Tag _lastModel;

		internal TreeTab(SkillsScreenHandler parent) {
			_parent = parent;
		}

		public string TabName => (string)STRINGS.ONIACCESS.SKILLS.TREE_TAB;

		// ========================================
		// ISkillsTab
		// ========================================

		public void OnTabActivated(bool announce) {
			RebuildGraph();
			if (announce)
				SpeechPipeline.SpeakInterrupt(TabName);
			var model = SkillsHelper.GetDupeModel(_parent.SelectedDupe);
			var roots = SkillsHelper.GetRootSkills(model);
			if (roots.Count > 0) {
				_graph.MoveToWithSiblings(roots[0], roots);
				SpeechPipeline.SpeakQueued(
					SkillsHelper.BuildSkillLabel(roots[0], _parent.SelectedDupe));
			}
		}

		internal void OnTabActivatedAt(Skill skill) {
			RebuildGraph();
			SpeechPipeline.SpeakInterrupt(TabName);
			_graph.MoveTo(skill);
			SpeechPipeline.SpeakQueued(
				SkillsHelper.BuildSkillLabel(skill, _parent.SelectedDupe));
		}

		public void OnTabDeactivated() { }

		public bool HandleInput() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) {
				EnsureGraphCurrent();
				var node = _graph.NavigateDown();
				if (node != null) {
					PlayHoverSound();
					SpeechPipeline.SpeakInterrupt(
						SkillsHelper.BuildSkillLabel(node, _parent.SelectedDupe));
				} else {
					SpeechPipeline.SpeakInterrupt(
						STRINGS.ONIACCESS.SKILLS.DEAD_END);
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)) {
				EnsureGraphCurrent();
				var node = _graph.NavigateUp();
				if (node != null) {
					PlayHoverSound();
					SpeechPipeline.SpeakInterrupt(
						SkillsHelper.BuildSkillLabel(node, _parent.SelectedDupe));
				} else {
					SpeechPipeline.SpeakInterrupt(
						STRINGS.ONIACCESS.SKILLS.ROOT_NODE);
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow)) {
				EnsureGraphCurrent();
				var node = _graph.CycleSibling(1, out bool wrapped);
				if (node != null) {
					if (wrapped) PlayWrapSound();
					else PlayHoverSound();
					SpeechPipeline.SpeakInterrupt(
						SkillsHelper.BuildSkillLabel(node, _parent.SelectedDupe));
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow)) {
				EnsureGraphCurrent();
				var node = _graph.CycleSibling(-1, out bool wrapped);
				if (node != null) {
					if (wrapped) PlayWrapSound();
					else PlayHoverSound();
					SpeechPipeline.SpeakInterrupt(
						SkillsHelper.BuildSkillLabel(node, _parent.SelectedDupe));
				}
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
				var skill = _graph.Current;
				if (skill != null)
					SkillsHelper.TryLearnSkill(
						skill, _parent.SelectedDupe, _parent.Screen);
				return true;
			}

			return false;
		}

		public bool HandleKeyDown(KButtonEvent e) => false;

		// ========================================
		// Graph management
		// ========================================

		private void RebuildGraph() {
			var model = SkillsHelper.GetDupeModel(_parent.SelectedDupe);
			_lastModel = model;
			_graph = new NavigableGraph<Skill>(
				getParents: skill => SkillsHelper.GetParents(skill),
				getChildren: skill => SkillsHelper.GetChildren(skill),
				getRoots: () => SkillsHelper.GetRootSkills(model));
		}

		private void EnsureGraphCurrent() {
			var model = SkillsHelper.GetDupeModel(_parent.SelectedDupe);
			if (_graph == null || model != _lastModel) {
				RebuildGraph();
				var roots = SkillsHelper.GetRootSkills(model);
				if (roots.Count > 0)
					_graph.MoveToWithSiblings(roots[0], roots);
			}
		}

		// ========================================
		// Sounds
		// ========================================

		static void PlayHoverSound() {
			try { KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Mouseover")); }
			catch (System.Exception ex) { Util.Log.Warn($"TreeTab: hover sound failed: {ex.Message}"); }
		}

		static void PlayWrapSound() {
			try { KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click")); }
			catch (System.Exception ex) { Util.Log.Warn($"TreeTab: wrap sound failed: {ex.Message}"); }
		}
	}
}
