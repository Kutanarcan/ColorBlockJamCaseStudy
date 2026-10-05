using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: closes one cell of its door. Shape of Jumping Single Door / Size-Changing Door.</summary>
    internal sealed class FakeClosedDoorCell : IExitAcceptRule
    {
        private readonly Cell closed;

        public FakeClosedDoorCell(Cell closed) => this.closed = closed;

        public bool Accepts(Door door, Cell cell, Block block) => cell != closed;
    }
}
