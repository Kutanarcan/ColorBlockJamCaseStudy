using System.Collections.Generic;
using UnityEngine;

namespace Game.Infrastructure
{
    /// <summary>
    /// Values that are data, not code (D72): the levels in play order and presentation timing. Addressable under
    /// <see cref="Key"/> in the <c>Boot</c> group; the bootstrapper loads it once per run. Coins join in M1.
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        public const string Key = "GameConfig";

        [Tooltip("Level keys in play order; a key is the level file's name (V1 D52).")]
        [SerializeField] private string[] levelKeys = { "Level_1" };

        [Tooltip("Seconds between the last exit finishing and the win popup (D89).")]
        [SerializeField, Min(0f)] private float winPopupDelay = 0.5f;

        public IReadOnlyList<string> LevelKeys => levelKeys;

        public float WinPopupDelay => winPopupDelay;
    }
}
