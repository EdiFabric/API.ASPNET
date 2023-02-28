namespace EdiFabric.Api.ASPNET.Models
{
    public class AnalyzeParameters
    {
        public string? CharSet { get; set; }
        public string? SyntaxSet { get; set; }
        public bool EancomS3 { get; set; }
        public bool SkipSeq { get; set; }
        public string? Ack { get; set; }
        public bool BasicSyntax { get; set; }
        public string? Model { get; set; }

        public AnalyzeParams ToAnalyzeParams()
        {
            var result = new AnalyzeParams();

            if (!string.IsNullOrEmpty(CharSet))
            {
                result.CharSet = CharSet;
            }
            if (!string.IsNullOrEmpty(SyntaxSet))
            {
                result.SyntaxSet = SyntaxSet;
            }
            result.EancomS3IsDefault = EancomS3;
            result.BasicSyntax = BasicSyntax;
            if (!string.IsNullOrEmpty(Ack))
            {
                result.AckVersion = Ack;
            }
            result.SkipSeqCountValidation = SkipSeq;
            if (!string.IsNullOrEmpty(Model))
            {
                result.Model = Model;
            }

            return result;
        }
    }
}
