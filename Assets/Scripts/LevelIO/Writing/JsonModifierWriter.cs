using Game.Core;
using Newtonsoft.Json.Linq;

namespace Game.LevelIO
{
    internal sealed class JsonModifierWriter : IModifierWriter
    {
        private readonly JObject json;

        public JsonModifierWriter(JObject json) => this.json = json;

        public void Int(string key, int value) => json.Add(key, value);

        public void Direction(string key, Direction value) => json.Add(key, DirectionNames.ToName(value));

        public void Axis(string key, Axis value) => json.Add(key, AxisNames.ToName(value));
    }
}
