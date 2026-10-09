namespace Game.Runtime
{
    /// <summary>
    /// What the Play popup is and does where it is opened (D121): in Gameplay it retries the level and costs a life
    /// (the heart shows), X goes Home; on Home it plays the level, X closes.
    /// </summary>
    public interface IPlayActions
    {
        /// <summary>Retry (Gameplay) rather than Play (Home): the label, and the heart beside the title.</summary>
        bool IsRetry { get; }

        void Play();

        void Close();
    }
}
