namespace Game.Core
{
    /// <summary>
    /// Definition of one modifier. Each subtype creates its own runtime modifier, so adding a modifier
    /// never touches the builder. A fresh instance per build keeps counters out of the definition.
    /// </summary>
    public abstract class ModifierData
    {
        public abstract IModifier ToModifier();
    }
}
