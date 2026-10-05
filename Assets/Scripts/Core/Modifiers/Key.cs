namespace Game.Core
{
    /// <summary>Passive. Read by <see cref="Lock"/> when the carrying block exits.</summary>
    public sealed class Key : IModifier
    {
        public int KeyId { get; }
        public int Count { get; }

        public Key(int keyId, int count)
        {
            KeyId = keyId;
            Count = count;
        }
    }
}
