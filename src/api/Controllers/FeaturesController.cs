using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FeatureToggle;
using api.Features;

namespace api.Controllers
{
    [Route("api/[controller]")]
    public class FeaturesController : Controller
    {
        private readonly IEnumerable<IFeatureToggle> _allFeatures;

        public FeaturesController(ValuesFeature valuesFeature, NavigationFeature navigationFeature)
        {
            _allFeatures = new List<IFeatureToggle>()
            {
                valuesFeature,
                navigationFeature
                // It would be much nicer if we could inject all registered IFeatureToggle in here, but
                // it appears to be a restriction of the DI in ASP.NET core when registering a concrete
                // type. It would avoid having to maintain this list manually.
            };
        }
        
        [HttpGet]
        public IDictionary<string, bool> Get()
        {
            var allFeatures = _allFeatures.Select(x => new { Key = x.GetType().Name, Value = x.FeatureEnabled });

            return allFeatures.ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
