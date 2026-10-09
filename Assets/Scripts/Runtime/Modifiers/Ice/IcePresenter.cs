using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// Reads Ice from the logic and drives its dumb view (D104): the block wears the ice surface and the remaining
    /// exit count sits on the block's anchor cell; the returned look follows the count as exits wear it down. The only
    /// place that knows how Ice keeps its count.
    /// </summary>
    public sealed class IcePresenter : IModifierPresenter
    {
        private readonly IceView prefab;

        public IcePresenter(IceView prefab) => this.prefab = prefab;

        public bool Accepts(IModifier modifier) => modifier is Ice;

        public IModifierLook Show(BlockView view, Block block, IModifier modifier)
        {
            var ice = (Ice)modifier;
            IceView iceView = view.Attach(prefab);

            view.SetSurface(iceView.Surface);
            iceView.Place(BoardLayout.CellCenter(BlockAnchor.Of(block)));
            iceView.SetCount(ice.Durability.Remaining);

            return new IceLook(iceView, ice, view.ResetSurface);
        }
    }
}
