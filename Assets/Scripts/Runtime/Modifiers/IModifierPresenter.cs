using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// Turns one kind of modifier into what its dumb view shows (D104). Pure C#: the only layer between a modifier's
    /// logic and its look, so views never reference Core.
    /// </summary>
    public interface IModifierPresenter
    {
        bool Accepts(IModifier modifier);

        void Show(BlockView view, Block block, IModifier modifier);
    }
}
