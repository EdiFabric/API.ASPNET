namespace EdiFabric.Api.ASPNET.Models
{
    public class AckParameters
    {
        public string? SyntaxSet { get; set; }
        public bool EancomS3 { get; set; }
        public bool DetectDuplicates { get; set; }
        public string? TechnicalAck { get; set; }
        public bool AckForValidTrans { get; set; }
        public int TranRefNumber { get; set; }
        public int InterchangeRefNumber { get; set; }
        public bool BatchAcks { get; set; }
        public bool BasicSyntax { get; set; }
        public bool Ak901isP { get; set; }
        public string? Ack { get; set; }

        public AckParams ToAckParams()
        {
            var result = new AckParams();

            if (!string.IsNullOrEmpty(SyntaxSet))
            {
                result.SyntaxSet = SyntaxSet;
            }
            result.DetectDuplicates = DetectDuplicates;
            if (!string.IsNullOrEmpty(TechnicalAck))
            {
                result.TechnicalAck = TechnicalAck;
            }
            result.GenerateForValidMessages = AckForValidTrans;
            result.MessageControlNumber = TranRefNumber;
            if (!string.IsNullOrEmpty(Ack))
            {
                result.AckVersion = Ack;
            }
            result.EancomS3IsDefault = EancomS3;
            result.BatchAcks = BatchAcks;
            result.InterchangeControlNumber = InterchangeRefNumber;
            result.Ak901ShouldBeP = Ak901isP;
            result.BasicSyntax = BasicSyntax;

            return result;
        }
    }
}
