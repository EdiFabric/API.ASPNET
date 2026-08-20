namespace EdiFabric.Api.ASPNET.Models
{
    public class WriteParameters
    {
        public string? ContentType { get; set; }
        public string? CharSet { get; set; }
        public string? Postfix { get; set; }

        public string ToContentType()
        {
            if (!string.IsNullOrEmpty(ContentType))
                return ContentType;

            if (!string.IsNullOrEmpty(CharSet))
                return $"application/octet-stream; charset={CharSet}";

            return "application/octet-stream; charset=utf-8";
        }
    }
}
