using Game.Core;

namespace Game.Runtime
{
    /// <summary>What the director needs for exits: a step per exiting block, and a way to clear what they left.</summary>
    public interface IExitSteps
    {
        IStep For(Block block, Direction direction);

        /// <summary>Removes the effects exits leave behind (particles still flying).</summary>
        void ClearEffects();
    }
}
