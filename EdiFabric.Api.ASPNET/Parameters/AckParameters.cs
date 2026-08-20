using System.Text.Json.Nodes;

namespace EdiFabric.Api.ASPNET.Models
{
    public class AckParameters : ValidateParameters
    {
        public bool SuppressTa1 { get; set; }
        public bool Ak901p { get; set; }
        public bool GenForValid { get; set; }
        public bool Gen997 { get; set; }

        public new string ToConfig()
        {
            var config = new JsonObject
            {
                ["validate"] = ToValidateObject(),
                ["ack"] = new JsonObject
                {
                    ["supress_ta1"] = SuppressTa1,
                    ["ak901p"] = Ak901p,
                    ["gen_for_valid"] = GenForValid,
                    ["gen997"] = Gen997,
                },
            };

            return config.ToJsonString();
        }
    }
}
