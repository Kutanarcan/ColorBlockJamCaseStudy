using Game.Meta;
using UnityEngine;

namespace Game.LevelTest
{
    /// <summary>
    /// The level test's own values (D130), kept out of the game's config. Referenced only by the LevelTest scene's
    /// scope, never addressable, so it cannot reach a build, and neither can the badge prefab it references.
    /// Everything else a test plays with comes from the game's config.
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Level Test Config", fileName = "LevelTestConfig")]
    public sealed class LevelTestConfig : ScriptableObject, IStartingCoins
    {
        [Tooltip("Coins a level test's wallet starts with; enough to try continues freely.")]
        [SerializeField, Min(0)] private int startCoins = 100000;

        [Tooltip("The \"LEVEL TEST\" badge shown over every screen of a level test (D131): its own canvas, no raycast.")]
        [SerializeField] private GameObject badge;

        public int Amount => startCoins;

        public GameObject Badge => badge;
    }
}
