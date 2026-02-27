namespace OniAccess.Handlers.Tiles

class CellContext (line 4)
  HashSet<UnityEngine.GameObject> Claimed { get; } (line 5)
    // Initialized to a new empty HashSet. Used by ICellSection implementations
    // to mark GameObjects already claimed for output, preventing duplicate
    // entries when multiple sections might otherwise speak the same entity.
