namespace Game.Runtime
{
    /// <summary>
    /// What the Settings popup's buttons do where it is opened (D121): one implementation per place, Gameplay and Home.
    /// The toggles are the same everywhere and need no action.
    /// </summary>
    public interface ISettingsActions
    {
        /// <summary>Whether this place has a Home button (Gameplay yes, Home no; D126).</summary>
        bool ShowsHome { get; }

        void Home();

        void Close();
    }
}
