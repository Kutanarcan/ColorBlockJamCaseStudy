using Game.Core;

namespace Game.Runtime
{
    /// <summary>Reads Arrow from the logic and drives its dumb view (D104): the arrow laid along its axis on the block's run.</summary>
    public sealed class ArrowPresenter : IModifierPresenter
    {
        private readonly ArrowView prefab;

        public ArrowPresenter(ArrowView prefab) => this.prefab = prefab;

        public bool Accepts(IModifier modifier) => modifier is Arrow;

        public void Show(BlockView view, Block block, IModifier modifier)
        {
            var arrow = (Arrow)modifier;
            ArrowView arrowView = view.Attach(prefab);
            ArrowPlacement placement = ArrowPlacementRule.Of(block, arrow.Axis, arrowView.MaxLength);

            arrowView.Show(placement.Size, placement.Placement.Position, placement.Placement.Yaw);
        }
    }
}
