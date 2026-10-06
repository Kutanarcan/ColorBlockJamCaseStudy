using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.LevelIO;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Composition root of the Gameplay scene until Infrastructure (D62, D64): wires the level source and
    /// starts the session. I4 replaces it with <c>GameplayLifetimeScope</c>; nothing it creates knows that.
    /// </summary>
    public sealed class GameplayInstaller : MonoBehaviour
    {
        [SerializeField] private TextAsset[] levels;
        [SerializeField] private string levelKey;

        private void Start() => InstallAsync(destroyCancellationToken).Forget();

        private async UniTaskVoid InstallAsync(CancellationToken cancellation)
        {
            var source = new TextAssetLevelSource(levels, new LevelJson(ModifierCatalog.Default()));
            Result<LevelData> level = await source.LoadAsync(levelKey)
                .AsUniTask()
                .AttachExternalCancellation(cancellation);

            if (level.IsFailure)
            {
                Debug.LogError($"Level '{levelKey}' could not be loaded: {level.Error}", this);
                return;
            }

            Result<LevelSession> session = LevelSession.TryCreate(level.Value);

            if (session.IsFailure)
            {
                Debug.LogError($"Level '{levelKey}' is invalid: {session.Error}", this);
                return;
            }

            Debug.Log($"Level '{levelKey}' started: {level.Value.Width}x{level.Value.Height}", this);
        }
    }
}
