using Game.LevelTest;
using UnityEditor;
using UnityEngine;

namespace Game.Tests.LevelTest
{
    /// <summary>A <see cref="LevelTestConfig"/> with a badge, as the LevelTest scene's scope requires one.</summary>
    internal static class TestConfigs
    {
        public static LevelTestConfig WithBadge(GameObject badge)
        {
            var config = ScriptableObject.CreateInstance<LevelTestConfig>();
            var serialized = new SerializedObject(config);
            serialized.FindProperty("badge").objectReferenceValue = badge;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return config;
        }
    }
}
