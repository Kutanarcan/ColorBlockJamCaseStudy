using System;
using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Test-only: lets a test put any fake modifier into a level. Each build calls the factory again.</summary>
    internal sealed class FakeModifierData : ModifierData
    {
        private readonly Func<IModifier> create;

        public FakeModifierData(Func<IModifier> create) => this.create = create;

        public override IModifier ToModifier() => create();
    }
}
