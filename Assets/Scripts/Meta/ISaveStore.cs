namespace Game.Meta
{
    /// <summary>
    /// Where saved data lives between runs, one section per key (D128). The store knows no feature: each feature
    /// owns its data type and its key, so a new feature adds a section instead of growing a shared save.
    /// </summary>
    public interface ISaveStore
    {
        /// <summary>The section under <paramref name="key"/>, or a new <typeparamref name="T"/> when there is none
        /// or it cannot be read.</summary>
        T Load<T>(string key) where T : class, new();

        void Save<T>(string key, T data) where T : class;
    }
}
