using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Core;
using Game.LevelIO;

namespace Game.Tests.LevelIO
{
    /// <summary>Test-only <see cref="ILevelSource"/>: JSON text by key, parsed on load. Completes synchronously.</summary>
    public sealed class FakeLevelSource : ILevelSource
    {
        private readonly Dictionary<string, string> jsonByKey = new Dictionary<string, string>();
        private readonly LevelJson json;

        public FakeLevelSource(LevelJson json) => this.json = json;

        public FakeLevelSource Add(string key, string text)
        {
            jsonByKey.Add(key, text);

            return this;
        }

        public Task<Result<LevelData>> LoadAsync(string key)
        {
            if (!jsonByKey.TryGetValue(key, out string text))
                return Task.FromResult(Result<LevelData>.Failure($"No level with key '{key}'."));

            return Task.FromResult(json.Parse(text));
        }
    }
}
