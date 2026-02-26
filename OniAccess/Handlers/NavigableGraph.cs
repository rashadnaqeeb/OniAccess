using System;
using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers {
	/// <summary>
	/// Reusable cursor for navigating a directed acyclic graph.
	/// Up/Down follows edges between parents and children.
	/// Left/Right cycles among siblings (other nodes sharing the
	/// same origin from the last Up/Down move).
	///
	/// All neighbor lookups are computed on demand via caller-supplied
	/// lambdas. No graph structure is cached internally.
	/// </summary>
	public class NavigableGraph<T> where T : class {
		private readonly Func<T, IReadOnlyList<T>> _getParents;
		private readonly Func<T, IReadOnlyList<T>> _getChildren;
		private readonly Func<IReadOnlyList<T>> _getRoots;

		private T _current;
		private IReadOnlyList<T> _siblings;
		private int _siblingIndex;

		public T Current => _current;

		public NavigableGraph(
			Func<T, IReadOnlyList<T>> getParents,
			Func<T, IReadOnlyList<T>> getChildren,
			Func<IReadOnlyList<T>> getRoots = null) {
			_getParents = getParents;
			_getChildren = getChildren;
			_getRoots = getRoots;
		}

		/// <summary>
		/// Set the current node without establishing sibling context.
		/// Left/Right does nothing until the first Up or Down.
		/// </summary>
		public void MoveTo(T node) {
			_current = node;
			_siblings = null;
			_siblingIndex = 0;
		}

		/// <summary>
		/// Set the current node with root-level sibling context.
		/// Left/Right cycles among the provided roots.
		/// </summary>
		public void MoveToWithSiblings(T node, IReadOnlyList<T> siblings) {
			_current = node;
			_siblings = siblings;
			_siblingIndex = IndexOf(siblings, node);
		}

		/// <summary>
		/// Move to the first child. Pushes current siblings context
		/// by setting siblings to the children of the current node.
		/// Returns the new current node, or null if no children.
		/// </summary>
		public T NavigateDown() {
			if (_current == null) return null;
			var children = _getChildren(_current);
			if (children == null || children.Count == 0) return null;

			_siblings = children;
			_siblingIndex = 0;
			_current = children[0];
			return _current;
		}

		/// <summary>
		/// Move to the first parent. Sets siblings to the parents
		/// of the node we came from.
		/// Returns the new current node, or null if at root.
		/// When at root and getRoots was provided, establishes root
		/// sibling context for Left/Right cycling.
		/// </summary>
		public T NavigateUp() {
			if (_current == null) return null;
			var parents = _getParents(_current);
			if (parents == null || parents.Count == 0) {
				if (_getRoots != null) {
					var roots = _getRoots();
					if (roots != null && roots.Count > 0) {
						_siblings = roots;
						_siblingIndex = IndexOf(roots, _current);
					}
				}
				return null;
			}

			_siblings = parents;
			_siblingIndex = 0;
			_current = parents[0];
			return _current;
		}

		/// <summary>
		/// Cycle among siblings (wraps around).
		/// Returns the new current node, or null if no sibling context.
		/// The out parameter indicates whether the cycle wrapped.
		/// </summary>
		public T CycleSibling(int direction, out bool wrapped) {
			wrapped = false;
			if (_current == null || _siblings == null || _siblings.Count <= 1)
				return null;

			int next = (_siblingIndex + direction + _siblings.Count) % _siblings.Count;
			wrapped = direction > 0 ? next <= _siblingIndex : next >= _siblingIndex;
			_siblingIndex = next;
			_current = _siblings[next];
			return _current;
		}

		public bool HasChildren {
			get {
				if (_current == null) return false;
				var children = _getChildren(_current);
				return children != null && children.Count > 0;
			}
		}

		public bool HasParents {
			get {
				if (_current == null) return false;
				var parents = _getParents(_current);
				return parents != null && parents.Count > 0;
			}
		}

		public bool HasSiblings => _siblings != null && _siblings.Count > 1;

		private static int IndexOf(IReadOnlyList<T> list, T item) {
			if (list == null) return 0;
			for (int i = 0; i < list.Count; i++) {
				if (ReferenceEquals(list[i], item))
					return i;
			}
			return 0;
		}
	}
}
