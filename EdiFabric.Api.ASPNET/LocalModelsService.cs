using System.Text.Json;
using System.Text.Json.Nodes;
using EdiFabric.Native.X12;

namespace EdiFabric.Api.ASPNET
{
    public interface ILocalModelsService
    {
        void Load(string serial, string mapPath);
        void LoadOnline(string serial);
    }

    public class LocalModelsService : ILocalModelsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

        public void LoadOnline(string serial)
        {
            var map = new JsonObject
            {
                ["default"] = serial,
                ["maps"] = new JsonObject(),
            };

            EdiFabricX12.SetMap(map.ToJsonString(JsonOptions));
        }

        public void Load(string serial, string mapPath)
        {
            var mapLocation = Path.GetDirectoryName(Path.GetFullPath(mapPath))
                ?? throw new InvalidOperationException($"Can't resolve the directory for map file '{mapPath}'.");

            var localMap = JsonNode.Parse(File.ReadAllText(mapPath))?.AsObject()
                ?? throw new InvalidDataException($"Map file '{mapPath}' is not a JSON object.");

            var maps = localMap["maps"]?.AsObject();
            if (maps is not null)
            {
                foreach (var entry in maps)
                {
                    if (entry.Value is JsonObject mapEntry)
                        mapEntry["location"] = mapLocation;
                }
            }

            if (string.IsNullOrWhiteSpace(localMap["default"]?.GetValue<string>()))
                localMap["default"] = serial;

            EdiFabricX12.SetMap(localMap.ToJsonString(JsonOptions));
        }
    }
}
