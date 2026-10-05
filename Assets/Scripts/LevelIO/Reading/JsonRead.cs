using Game.Core;
using Newtonsoft.Json.Linq;

namespace Game.LevelIO
{
    /// <summary>Typed reads from JObject. Every failure names its path, e.g. level.blocks[0].modifiers[1].</summary>
    internal static class JsonRead
    {
        public static int Int(JObject json, string key, string path)
        {
            JToken token = Required(json, key, path);

            if (TryInt(token, out int value))
                return value;

            throw Invalid(path, key, "an integer");
        }

        public static float Float(JObject json, string key, string path)
        {
            JToken token = Required(json, key, path);

            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
                return (float)token;

            throw Invalid(path, key, "a number");
        }

        public static string String(JObject json, string key, string path)
        {
            JToken token = Required(json, key, path);

            if (token.Type == JTokenType.String)
                return (string)token;

            throw Invalid(path, key, "a string");
        }

        public static Direction Direction(JObject json, string key, string path)
        {
            JToken token = Required(json, key, path);

            if (token.Type == JTokenType.String && DirectionNames.TryParse((string)token, out Direction direction))
                return direction;

            throw Invalid(path, key, "Up, Down, Left or Right");
        }

        public static JArray Array(JObject json, string key, string path)
        {
            if (Required(json, key, path) is JArray array)
                return array;

            throw Invalid(path, key, "an array");
        }

        /// <summary>A missing or null list reads as empty (null here).</summary>
        public static JArray OptionalArray(JObject json, string key, string path)
        {
            if (!json.TryGetValue(key, out JToken token) || token.Type == JTokenType.Null)
                return null;

            if (token is JArray array)
                return array;

            throw Invalid(path, key, "an array");
        }

        public static JObject Object(JToken token, string path)
        {
            if (token is JObject json)
                return json;

            throw new LevelFormatException($"{path} must be an object.");
        }

        public static Cell Cell(JToken token, string path)
        {
            if (token is JArray pair && pair.Count == 2 && TryInt(pair[0], out int x) && TryInt(pair[1], out int y))
                return new Cell(x, y);

            throw new LevelFormatException($"{path} must be a cell [x, y].");
        }

        private static bool TryInt(JToken token, out int value)
        {
            if (token.Type == JTokenType.Integer && ((JValue)token).Value is long number
                && number >= int.MinValue && number <= int.MaxValue)
            {
                value = (int)number;

                return true;
            }

            value = 0;

            return false;
        }

        private static JToken Required(JObject json, string key, string path)
        {
            if (json.TryGetValue(key, out JToken token))
                return token;

            throw new LevelFormatException($"{path} has no \"{key}\".");
        }

        private static LevelFormatException Invalid(string path, string key, string expected) =>
            new LevelFormatException($"{path}.{key} must be {expected}.");
    }
}
