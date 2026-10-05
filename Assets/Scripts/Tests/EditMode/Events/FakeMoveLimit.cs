using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: fails the level after a number of committed moves. Shape of Dynamite.</summary>
    internal sealed class FakeMoveLimit : IMoveListener
    {
        private int movesLeft;

        public FakeMoveLimit(int moves) => movesLeft = moves;

        public void OnMoveCommitted(Entity owner, ILevelCommands commands)
        {
            movesLeft--;

            if (movesLeft <= 0)
                commands.Fail();
        }
    }
}
