using Game.Infrastructure;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>
    /// Puts a saved level into the <c>Levels</c> group under its key, with the <c>remote</c> label (D115, D118), so a
    /// new level plays and ships without touching the Addressables window. Saving again keeps the one entry.
    /// </summary>
    internal static class LevelAddressables
    {
        private const string GroupName = "Levels";

        public static void Register(string assetPath, string key)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings == null ? null : settings.FindGroup(GroupName);

            if (group == null)
            {
                Debug.LogError($"Level '{key}' was saved but not made addressable: no '{GroupName}' group.");
                return;
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), group);
            entry.address = key;
            entry.SetLabel(ContentLabels.Remote, true);
            AssetDatabase.SaveAssets();
        }
    }
}
