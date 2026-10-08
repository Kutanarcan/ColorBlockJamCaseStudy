using Game.Infrastructure;

namespace Game.Tests.Infrastructure
{
    /// <summary>Holds the request in a field, as SessionState would across entering play mode.</summary>
    internal sealed class FakePlayRequestStore : IPlayRequestStore
    {
        public string Value { get; private set; } = "";

        public string Read() => Value;

        public void Write(string levelKey) => Value = levelKey;
    }
}
