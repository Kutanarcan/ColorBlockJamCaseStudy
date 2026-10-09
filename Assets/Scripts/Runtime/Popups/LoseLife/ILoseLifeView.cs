using System;

namespace Game.Runtime
{
    /// <summary>What the LoseLife presenter drives: the title, the action label and the two buttons.</summary>
    public interface ILoseLifeView
    {
        event Action ActionClicked;

        event Action CloseClicked;

        void ShowTitle(int levelNumber);

        void ShowAction(string label);
    }
}
