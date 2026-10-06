using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// Which presenter draws which modifier (D104): the first that accepts it; none means the modifier has no look.
    /// A new modifier's look adds a presenter, a view and one line in <see cref="Default"/> (same growth as
    /// <c>ModifierCatalog.Default()</c>, V1 D48).
    /// </summary>
    public sealed class ModifierPresenters
    {
        private readonly IModifierPresenter[] presenters;

        public ModifierPresenters(IModifierPresenter[] presenters) => this.presenters = presenters;

        public static ModifierPresenters Default(ModifierViews views) =>
            new ModifierPresenters(new IModifierPresenter[]
            {
                new IcePresenter(views.Ice),
                new ArrowPresenter(views.Arrow)
            });

        public IModifierPresenter Find(IModifier modifier)
        {
            for (int i = 0; i < presenters.Length; i++)
            {
                if (presenters[i].Accepts(modifier))
                    return presenters[i];
            }

            return null;
        }
    }
}
