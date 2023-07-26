namespace EdiFabric.Api.ASPNET
{
    public class LocalModelsService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IModelService _modelService;
        private readonly string _apiKey;

        public LocalModelsService(IConfiguration configuration, IModelService modelService)
        {
            _configuration = configuration;
            _modelService = modelService;
            _apiKey = _configuration["ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("No ApiKey configuration in appsettings.json.");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //  Load local EDI models
            //  When models are local they won't be pulled from EdiNation API
            var x12ModelsPath = Directory.GetCurrentDirectory() + @"\EDI";
            foreach (var fileName in Directory.GetFiles(x12ModelsPath))
            {
                var model = File.ReadAllBytes(fileName);
                var modelName = Path.GetFileName(fileName);
                await _modelService.Load(_apiKey, modelName, new MemoryStream(model));
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
