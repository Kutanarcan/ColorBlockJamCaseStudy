using System.Threading.Tasks;

namespace Game.Core
{
    /// <summary>Levels are reached by key, never by path: a TextAsset today, Addressables or a download later.</summary>
    public interface ILevelSource
    {
        Task<Result<LevelData>> LoadAsync(string key);
    }
}
