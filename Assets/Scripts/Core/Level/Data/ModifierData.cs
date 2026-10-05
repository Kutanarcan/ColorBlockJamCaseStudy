namespace Game.Core
{
    public abstract class ModifierData
    {
        public abstract IModifier ToModifier();

        public virtual Result Validate() => Result.Success();
    }
}
