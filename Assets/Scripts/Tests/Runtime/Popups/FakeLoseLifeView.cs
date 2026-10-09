using System;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>Remembers what it shows; the test presses its buttons.</summary>
    internal sealed class FakeLoseLifeView : ILoseLifeView
    {
        public event Action ActionClicked;

        public event Action CloseClicked;

        public int Title { get; private set; }

        public string Action { get; private set; }

        public void ShowTitle(int levelNumber) => Title = levelNumber;

        public void ShowAction(string label) => Action = label;

        public void PressAction() => ActionClicked?.Invoke();

        public void PressClose() => CloseClicked?.Invoke();
    }
}
