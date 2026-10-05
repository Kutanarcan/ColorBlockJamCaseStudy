using Game.Core;
using Newtonsoft.Json.Linq;

namespace Game.LevelIO
{
    internal sealed class JsonModifierReader : IModifierReader
    {
        private readonly JObject json;
        private readonly string path;

        public JsonModifierReader(JObject json, string path)
        {
            this.json = json;
            this.path = path;
        }

        public int Int(string key) => JsonRead.Int(json, key, path);

        public Direction Direction(string key) => JsonRead.Direction(json, key, path);
    }
}
