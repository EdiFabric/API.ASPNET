using System.Runtime.Loader;

namespace EdiFabric.Api.ASPNET
{
    public class LocalModelsService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;

        public LocalModelsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("No ApiKey configuration in appsettings.json.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //  Load local EDI models
            //  When models are local they won't be pulled from EdiNation API
            var loadContext = new CustomAssemblyLoadContext(_apiKey);
            var modelsPath = Directory.GetCurrentDirectory() + @"\EDI";
            foreach (var fileName in Directory.GetFiles(modelsPath))
                loadContext.LoadFromAssemblyPath(fileName);

            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public CustomAssemblyLoadContext(string subscriptionId) : base(subscriptionId, isCollectible: true)
        {
        }
    }
}
