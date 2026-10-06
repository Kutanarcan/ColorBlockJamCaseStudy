using UnityEngine;

namespace Game.Runtime
{
    /// <summary>The view prefab of each modifier that has a look (D104). Read by <see cref="ModifierPresenters.Default"/>.</summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Modifier Views", fileName = "ModifierViews")]
    public sealed class ModifierViews : ScriptableObject
    {
        [SerializeField] private IceView ice;
        [SerializeField] private ArrowView arrow;

        public IceView Ice => ice;
        public ArrowView Arrow => arrow;
    }
}
