namespace Game.Core
{
    /// <summary>
    /// Holds the block until <c>count</c> keys with its key id have exited. Every lock with the same
    /// key id counts the same exit, so one key block can open several locks.
    /// </summary>
    public sealed class Lock : ISuspender, IDurable
    {
        public int KeyId { get; }
        public Capability Suspends => Capability.Move | Capability.Exit;
        public Durability Durability { get; }

        public Lock(int keyId, int count)
        {
            KeyId = keyId;
            Durability = new Durability(count);
        }

        public int AmountFor(Block exited)
        {
            int keys = 0;

            for (int i = 0; i < exited.ModifierCount; i++)
            {
                if (exited.GetModifier(i) is Key key && key.KeyId == KeyId)
                    keys += key.Count;
            }

            return keys;
        }
    }
}
