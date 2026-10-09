using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// Turns one kind of modifier into what its dumb view shows (D104). Pure C#: the only layer between a modifier's
    /// logic and its look, so views never reference Core. A look that changes with the logic is returned to be
    /// refreshed; a look that never changes returns null.
    /// </summary>
    public interface IModifierPresenter
    {
        bool Accepts(IModifier modifier);

        IModifierLook Show(BlockView view, Block block, IModifier modifier);
    }
}
