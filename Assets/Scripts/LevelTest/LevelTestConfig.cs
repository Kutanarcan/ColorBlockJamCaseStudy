using Game.Meta;
using UnityEngine;

namespace Game.LevelTest
{
    /// <summary>
    /// The level test's own values (D130), kept out of the game's config. Referenced only by the LevelTest scene's
    /// scope, never addressable, so it cannot reach a build. Everything else a test plays with comes from the game's
    /// config.
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Level Test Config", fileName = "LevelTestConfig")]
    public sealed class LevelTestConfig : ScriptableObject, IStartingCoins
    {
        [Tooltip("Coins a level test's wallet starts with; enough to try continues freely.")]
        [SerializeField, Min(0)] private int startCoins = 100000;

        public int Amount => startCoins;
    }
}
