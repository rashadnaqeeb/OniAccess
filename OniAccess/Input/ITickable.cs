namespace OniAccess.Input
{
    /// <summary>
    /// Optional interface for handlers that need per-frame updates.
    /// KeyPoller checks for this on the active handler and calls Tick() each frame.
    /// Used by WorldGenHandler for progress polling during world generation.
    /// </summary>
    public interface ITickable
    {
        void Tick();
    }
}
