namespace Game.Runtime
{
    /// <summary>What LoseLife's buttons do in Gameplay, the only place that opens it (D134).</summary>
    public interface ILoseLifeActions
    {
        void Retry();

        void Leave();

        /// <summary>X: back to play (D122), also when LoseLife came from Settings.</summary>
        void Close();
    }
}
