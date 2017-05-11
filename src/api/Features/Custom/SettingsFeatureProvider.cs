using System;
using Microsoft.Extensions.Configuration;
using FeatureToggle;

namespace api.Features.Custom
{
    public class SettingsFeatureProvider : IBooleanToggleValueProvider
    {
        private readonly IConfigurationRoot _configuration;

        public SettingsFeatureProvider(IConfigurationRoot configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _configuration = configuration;
        }

        public bool EvaluateBooleanToggleValue(IFeatureToggle toggle)
        {
            if (toggle == null)
                throw new ArgumentNullException(nameof(toggle));

            string settingName = toggle.GetType().Name;
            string keyName = $"FeatureToggle:{settingName}";
            string value = _configuration[keyName];

            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException($"Key not found in AppSetting.json: {keyName}");

            return Convert.ToBoolean(value);
        }
    }
}
