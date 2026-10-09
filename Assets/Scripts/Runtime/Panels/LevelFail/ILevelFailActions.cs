namespace Game.Runtime
{
    /// <summary>What LevelFail's buttons do in the Gameplay scene (D124).</summary>
    public interface ILevelFailActions
    {
        /// <summary>The continue is paid: give the level its seconds and play on.</summary>
        void Continue(int seconds);

        /// <summary>X: the Play popup (Retry) takes the panel's place (U5.4).</summary>
        void Close();
    }
}
