namespace Game.Infrastructure
{
    /// <summary>
    /// The content scene a start opens first (D131): the game opens Home (<see cref="SceneKeys.Main"/>), a level test
    /// opens the tested level (<see cref="SceneKeys.Gameplay"/>). A start service, so the bootstrapper opens it without
    /// knowing which start it is in.
    /// </summary>
    public sealed class FirstScene
    {
        public FirstScene(string key) => Key = key;

        public string Key { get; }
    }
}
