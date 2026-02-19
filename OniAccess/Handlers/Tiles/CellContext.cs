using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles {
	public class CellContext {
		public HashSet<UnityEngine.GameObject> Claimed { get; }
			= new HashSet<UnityEngine.GameObject>();
	}
}
