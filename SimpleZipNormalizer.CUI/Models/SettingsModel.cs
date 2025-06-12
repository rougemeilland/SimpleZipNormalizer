using System.Text.Json.Serialization;

namespace SimpleZipNormalizer.CUI.Models
{
    internal sealed class SettingsModel
    {
        public SettingsModel()
        {
            WarnedFilePatterns = [];
            ExcludedFilePatterns = [];
            BlackList = [];
        }

        [JsonPropertyName("warned_file_patterns")]
        public string[] WarnedFilePatterns { get; set; }

        [JsonPropertyName("excluded_file_patterns")]
        public string[] ExcludedFilePatterns { get; set; }

        [JsonPropertyName("black_list")]
        public string[] BlackList { get; set; }
    }
}
