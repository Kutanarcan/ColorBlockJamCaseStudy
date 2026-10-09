using System;

namespace Game.Runtime
{
    /// <summary>What the Play presenter drives: the title, the heart beside it, the action label, two buttons.</summary>
    public interface IPlayView
    {
        event Action ActionClicked;

        event Action CloseClicked;

        void ShowTitle(string title);

        void ShowTitleIcon(bool shown);

        void ShowAction(string label);
    }
}
