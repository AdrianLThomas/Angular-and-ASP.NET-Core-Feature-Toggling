using api.Features;
using api.Features.Custom;
using FeatureToggle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace api.Extensions
{
    public static class FeatureServiceCollectionExtensions
    {

        public static void AddFeatures(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var provider = new SettingsFeatureProvider(configuration);
            services.AddSingleton<ValuesFeature>(new ValuesFeature() { ToggleValueProvider = provider });
        }
    }
}
