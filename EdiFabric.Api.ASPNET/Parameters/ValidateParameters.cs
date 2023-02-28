namespace EdiFabric.Api.ASPNET.Models
{
    public class ValidateParameters
    {
        public string? SyntaxSet { get; set; }

        public string? DecimalPoint { get; set; }

        public bool SkipTrailer { get; set; }

        public bool EancomS3 { get; set; }

        public bool StructureOnly { get; set; }

        public bool BasicSyntax { get; set; }

        public ValidateParams ToValidateParams()
        {
            var result = new ValidateParams();

            result.SkipTrailerValidation = SkipTrailer;
            if (!string.IsNullOrEmpty(DecimalPoint))
            {
                result.DecimalPoint = DecimalPoint == "." ? '.' : ',';
            }
            if (!string.IsNullOrEmpty(SyntaxSet))
            {
                result.SyntaxSet = SyntaxSet;
            }
            result.EancomS3IsDefault = EancomS3;
            result.StructureOnly = StructureOnly;
            result.BasicSyntax = BasicSyntax;

            return result;
        }
    }
}
