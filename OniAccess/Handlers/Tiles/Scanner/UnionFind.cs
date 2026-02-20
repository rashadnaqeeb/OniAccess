namespace OniAccess.Handlers.Tiles.Scanner {
	/// <summary>
	/// Array-based union-find with path compression and union by rank.
	/// Allocated once per clustering domain, reset per scan via Reset().
	/// </summary>
	public class UnionFind {
		private int[] _parent;
		private int[] _rank;
		private int _size;

		public UnionFind(int size) {
			_size = size;
			_parent = new int[size];
			_rank = new int[size];
			InitArrays();
		}

		/// <summary>
		/// Re-initialize for a new scan. Reuses existing arrays if size
		/// matches, otherwise reallocates.
		/// </summary>
		public void Reset(int size) {
			if (size != _size) {
				_size = size;
				_parent = new int[size];
				_rank = new int[size];
			}
			InitArrays();
		}

		public void Union(int a, int b) {
			int rootA = Find(a);
			int rootB = Find(b);
			if (rootA == rootB) return;
			if (_rank[rootA] < _rank[rootB]) {
				_parent[rootA] = rootB;
			} else if (_rank[rootA] > _rank[rootB]) {
				_parent[rootB] = rootA;
			} else {
				_parent[rootB] = rootA;
				_rank[rootA]++;
			}
		}

		public int Find(int cell) {
			while (_parent[cell] != cell) {
				_parent[cell] = _parent[_parent[cell]];
				cell = _parent[cell];
			}
			return cell;
		}

		private void InitArrays() {
			for (int i = 0; i < _size; i++) {
				_parent[i] = i;
				_rank[i] = 0;
			}
		}
	}
}
