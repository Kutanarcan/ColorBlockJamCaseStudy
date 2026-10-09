namespace Game.Runtime
{
    /// <summary>
    /// A modifier's look that follows the logic after it is built (D104): it reads its modifier again and shows what
    /// changed. Ice's count is one; a look that never changes (Arrow) has none.
    /// </summary>
    public interface IModifierLook
    {
        void Refresh();
    }
}
