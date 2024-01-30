using System;
using System.Text.Json.Serialization;

namespace SimpleZipNormalizer.CUI.Models
{
    internal class SettingsModel
    {
        public SettingsModel()
        {
            WarnedFilePatterns = Array.Empty<string>();
            ExcludedFilePatterns = Array.Empty<string>();
            BlackList = Array.Empty<string>();
        }

        [JsonPropertyName("warned_file_patterns")]
        public string[] WarnedFilePatterns { get; set; }

        [JsonPropertyName("excluded_file_patterns")]
        public string[] ExcludedFilePatterns { get; set; }

        [JsonPropertyName("black_list")]
        public string[] BlackList { get; set; }
    }
}
