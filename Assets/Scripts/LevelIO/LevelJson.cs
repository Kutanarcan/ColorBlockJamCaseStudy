using Game.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.LevelIO
{
    /// <summary>
    /// LevelData to JSON and back, mapped by hand over JObject: no reflection, IL2CPP safe.
    /// Parse only turns text into data; structural checks stay in LevelSession.TryCreate.
    /// </summary>
    public sealed class LevelJson
    {
        private readonly LevelJsonReader reader;
        private readonly LevelJsonWriter writer;

        public LevelJson(ModifierCatalog catalog)
        {
            reader = new LevelJsonReader(catalog);
            writer = new LevelJsonWriter(catalog);
        }

        public string Serialize(LevelData level) => writer.Write(level).ToString(Formatting.Indented);

        public Result<LevelData> Parse(string json)
        {
            try
            {
                return Result<LevelData>.Success(reader.Read(JObject.Parse(json)));
            }
            catch (JsonException e)
            {
                return Result<LevelData>.Failure(e.Message);
            }
            catch (LevelFormatException e)
            {
                return Result<LevelData>.Failure(e.Message);
            }
        }
    }
}
