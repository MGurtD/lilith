using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Application.Services.System
{
    public class JsonStringLocalizerFactory(IOptions<LocalizationOptions> options,
        ILogger<JsonStringLocalizer> logger) : IStringLocalizerFactory
    {
        private readonly string _resourcesPath = options.Value.ResourcesPath ?? "Resources";

        public IStringLocalizer Create(Type resourceSource)
        {
            return Create(resourceSource.Name);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return new JsonStringLocalizer(_resourcesPath, baseName, logger);
        }

        private IStringLocalizer Create(string baseName)
        {
            return Create(baseName, _resourcesPath);
        }
    }
}




