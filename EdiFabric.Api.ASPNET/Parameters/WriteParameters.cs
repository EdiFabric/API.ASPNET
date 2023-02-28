using System.Runtime.InteropServices;

namespace EdiFabric.Api.ASPNET.Models
{
    public class WriteParameters
    {
        public string? ContentType { get; set; }
        public string? CharSet { get; set; }

        public bool PreserveWhitespace { get; set; }

        public string? Postfix { get; set; }

        public bool EancomS3 { get; set; }

        public bool NoG1 { get; set; }

        public string? TrailerMessage { get; set; }

        public WriteParams ToWriteParams()
        {
            var result = new WriteParams();

            result.ContentType = "application/octet-stream; charset=utf-8";
            if (!string.IsNullOrEmpty(CharSet))
            {
                result.CharSet = CharSet;
                result.ContentType = $"application/octet-stream; charset={CharSet}";
            }
            result.PreserveWhitespace = PreserveWhitespace;
            if (!string.IsNullOrEmpty(Postfix))
            {
                result.Postfix = Postfix;
            }
            result.EancomS3IsDefault = EancomS3;
            result.NoG1 = NoG1;
            if (!string.IsNullOrEmpty(TrailerMessage))
            {
                result.NcpdpTrailerMessage = TrailerMessage;
            }

            return result;
        }
    }
}
