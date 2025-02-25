using System.Runtime.Loader;

namespace EdiFabric.Api.ASPNET
{
    public interface ILocalModelsService
    {
        void Load(string apiKey, string path);
    }
    public class LocalModelsService : ILocalModelsService
    {
        public void Load(string apiKey, string path)
        {
            var loadContext = GetLoadContext(apiKey);

            foreach (var fileName in Directory.GetFiles(path))
            {
                if(loadContext.Assemblies.FirstOrDefault(a => a.Location == fileName) == null)
                    loadContext.LoadFromAssemblyPath(fileName);
            }
        }

        private AssemblyLoadContext GetLoadContext(string apiKey)
        {
            return AssemblyLoadContext.All.FirstOrDefault(alc => alc.Name == apiKey) ?? new CustomAssemblyLoadContext(apiKey);
        }
    }

    class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public CustomAssemblyLoadContext(string subscriptionId) : base(subscriptionId)
        {
        }
    }
}
