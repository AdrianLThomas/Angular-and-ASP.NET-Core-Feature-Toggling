using api.Features;
using api.Features.Custom;
using FeatureToggle;
using FeatureToggle.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace api.Extensions
{
    public static class FeatureServiceCollectionExtensions
    {

        public static void AddFeatures(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var provider = new AppSettingsProvider{ Configuration = configuration };
            
            services.AddSingleton(new ValuesFeature() { ToggleValueProvider = provider });
            services.AddSingleton(new NavigationFeature() { ToggleValueProvider = provider });
        }
    }
}
