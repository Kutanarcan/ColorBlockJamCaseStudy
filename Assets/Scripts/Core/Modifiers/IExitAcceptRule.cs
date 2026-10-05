namespace Game.Core
{
    /// <summary>
    /// Sits on a door and decides, cell by cell, whether a block may exit through it.
    /// The door's shape never changes; per-cell state lives here.
    /// </summary>
    public interface IExitAcceptRule : IModifier
    {
        bool Accepts(Door door, Cell cell, Block block);
    }
}
