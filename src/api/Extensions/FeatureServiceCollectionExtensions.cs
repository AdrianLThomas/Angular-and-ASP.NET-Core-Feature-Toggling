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

            var allFeatures = new IFeatureToggle[]
            {
                new ValuesFeature() { ToggleValueProvider = provider }
            };

            RegisterAllFeatures(services, provider, allFeatures);
            RegisterIndividualFeatures(services, provider, allFeatures);
        }

        private static void RegisterAllFeatures(IServiceCollection services, SettingsFeatureProvider provider, IFeatureToggle[] allFeatures)
        {
            services.AddSingleton(allFeatures);
        }

        private static void RegisterIndividualFeatures(IServiceCollection services, SettingsFeatureProvider provider, IFeatureToggle[] allFeatures)
        {
            foreach (IFeatureToggle feature in allFeatures)
            {
                services.AddSingleton(feature);
            }
        }
    }
}
