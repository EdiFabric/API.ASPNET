namespace EdiFabric.Api.ASPNET
{
    public class ErrorDetails
    {
        public int Code { get; set; }
        public List<string> Details { get; set; } = new();
    }
}
