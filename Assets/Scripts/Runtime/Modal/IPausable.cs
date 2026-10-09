namespace Game.Runtime
{
    /// <summary>
    /// Play the modal layer stops while a modal is shown (D70, D122): pause stops the ticks and locks input, resume
    /// starts them again. Both can be called more than once.
    /// </summary>
    public interface IPausable
    {
        void Pause();

        void Resume();
    }
}
