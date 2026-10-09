using System;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// What LevelFail shows for one fail kind (D124): title, icon and description; the bonus is the seconds a
    /// continue adds. A new fail kind is a new content, not new code in the view.
    /// </summary>
    [Serializable]
    public sealed class FailContent
    {
        [SerializeField] private string title = "Out of Time!";
        [SerializeField] private Sprite icon;

        [Tooltip("{0} is the seconds a continue adds.")]
        [SerializeField] private string description = "Get {0} seconds to keep playing!";

        public string Title => title;

        public Sprite Icon => icon;

        public string Description => description;
    }
}
