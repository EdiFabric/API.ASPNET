namespace EdiFabric.Api.ASPNET.Models
{
    public class ReadParameters
    {
        public string? CharSet { get; set; }
        
        public bool ContinueOnError { get; set; }
       
        public bool EancomS3 { get; set; }
       
        public bool IgnoreNullValues { get; set; }

        public string? Model { get; set; }

        public ReadParams ToReadParams()
        {
            var result = new ReadParams();

            if (!string.IsNullOrEmpty(CharSet))
            {
                result.CharSet = CharSet;
            }
            result.ContinueOnError = ContinueOnError;
            result.EancomS3IsDefault = EancomS3;
            result.IgnoreNullValues = IgnoreNullValues;
            if (!string.IsNullOrEmpty(Model))
            {
                result.Model = Model;
            }
                
            return result;
        }
    }
}
