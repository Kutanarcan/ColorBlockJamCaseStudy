namespace Game.Core
{
    public sealed class LockData : ModifierData
    {
        public int KeyId;

        /// <summary>Keys required to open.</summary>
        public int Count = 1;

        public override IModifier ToModifier() => new Lock(KeyId, Count);
    }
}
