namespace Game.Runtime
{
    /// <summary>What Ice's look drives as the count changes: a new count with a shake, and the melt at zero.</summary>
    public interface IIceView
    {
        /// <summary>Shows a lower count with a shake.</summary>
        void Bump(int remaining);

        /// <summary>The ice is gone: the count leaves the block.</summary>
        void Melt();
    }
}
