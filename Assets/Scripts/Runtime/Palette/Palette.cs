using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The real colors behind colorIds (V1 D23). Data only: materials are made from it at load (D95),
    /// so the colors can move to remote config later without a build.
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Palette", fileName = "Palette")]
    public sealed class Palette : ScriptableObject
    {
        [SerializeField] private Color[] colors = new Color[0];

        public int Count => colors.Length;

        public Color ColorOf(int colorId) => colors[colorId];
    }
}
