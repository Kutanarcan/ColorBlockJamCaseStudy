using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>Fake: gives its entity another color. Shape of Color-Switching / Colorful Door mechanics.</summary>
    internal sealed class FakeColorSource : IColorSource
    {
        public int ColorId { get; }

        public FakeColorSource(int colorId) => ColorId = colorId;
    }
}
