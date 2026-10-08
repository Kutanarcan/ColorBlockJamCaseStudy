using System.Collections.Generic;
using UnityEngine;

namespace Game.Infrastructure
{
    /// <summary>
    /// Values that are data, not code (D72): the levels in play order, presentation timing and the economy (D79,
    /// D124). Addressable under <see cref="Key"/> in the <c>Boot</c> group; the bootstrapper loads it once per run.
    /// A level test shares every value except the starting coins (D130).
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        public const string Key = "GameConfig";

        [Tooltip("Level keys in play order; a key is the level file's name (V1 D52).")]
        [SerializeField] private string[] levelKeys = { "Level_1" };

        [Tooltip("Seconds between the last exit finishing and the win popup (D89).")]
        [SerializeField, Min(0f)] private float winPopupDelay = 0.5f;

        [Header("Economy")]
        [Tooltip("Coins a new player starts with.")]
        [SerializeField, Min(0)] private int startCoins = 1000;

        [Tooltip("Coins a won level adds (D79).")]
        [SerializeField, Min(0)] private int winReward = 50;

        [Tooltip("Price of the first continue in a level attempt (D124).")]
        [SerializeField, Min(1)] private int continueBasePrice = 900;

        [Tooltip("How much dearer each further continue in the same attempt gets (D124).")]
        [SerializeField, Min(0)] private int continuePriceStep = 1000;

        [Tooltip("Seconds a continue adds to the timer (D124).")]
        [SerializeField, Min(1f)] private float continueSeconds = 20f;

        public IReadOnlyList<string> LevelKeys => levelKeys;

        public float WinPopupDelay => winPopupDelay;

        public int StartCoins => startCoins;

        public int WinReward => winReward;

        public int ContinueBasePrice => continueBasePrice;

        public int ContinuePriceStep => continuePriceStep;

        public float ContinueSeconds => continueSeconds;
    }
}
