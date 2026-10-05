namespace Game.Core
{
    public sealed class KeyData : ModifierData
    {
        public int KeyId;
        public int Count = 1;

        public override IModifier ToModifier() => new Key(KeyId, Count);
    }
}
