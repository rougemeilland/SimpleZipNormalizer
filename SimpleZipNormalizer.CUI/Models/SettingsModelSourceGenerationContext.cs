using System.Text.Json.Serialization;

namespace SimpleZipNormalizer.CUI.Models
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(SettingsModel))]
    internal sealed partial class SettingsModelSourceGenerationContext
        : JsonSerializerContext
    {
    }
}
