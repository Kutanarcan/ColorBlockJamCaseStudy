namespace Game.Runtime
{
    /// <summary>
    /// The panels that end a level (D134): what the director plays once the level is won or failed. Each is a step,
    /// so it waits in the director's flow and a restart cancels it (D69). The scene decides what a panel shows and
    /// what its buttons do.
    /// </summary>
    public interface ILevelPanels
    {
        IStep Win();

        IStep Fail();
    }
}
