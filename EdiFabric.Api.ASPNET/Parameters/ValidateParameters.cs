using System.Text.Json.Nodes;

namespace EdiFabric.Api.ASPNET.Models
{
    public class ValidateParameters
    {
        public string? Regex { get; set; }
        public string? DateFormat { get; set; }
        public string? TimeFormat { get; set; }
        public bool SkipSeqCount { get; set; }
        public bool SkipHlSeq { get; set; }
        public int SnipLevel { get; set; }
        public int MaxErrors { get; set; }

        public string ToConfig()
        {
            var config = new JsonObject
            {
                ["validate"] = ToValidateObject(),
            };

            return config.ToJsonString();
        }

        internal JsonObject ToValidateObject()
        {
            return new JsonObject
            {
                ["regex"] = string.IsNullOrEmpty(Regex) ? null : Regex,
                ["date_format"] = string.IsNullOrEmpty(DateFormat) ? null : DateFormat,
                ["time_format"] = string.IsNullOrEmpty(TimeFormat) ? null : TimeFormat,
                ["skip_seq_count"] = SkipSeqCount,
                ["skip_hl_seq"] = SkipHlSeq,
                ["snip_level"] = SnipLevel,
                ["max_errors"] = MaxErrors,
            };
        }
    }
}
