using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FeatureToggle;

namespace api.Controllers
{
    [Route("api/[controller]")]
    public class FeaturesController : Controller
    {
        private readonly IFeatureToggle[] _allFeatures;

        public FeaturesController(IFeatureToggle[] allFeatures)
        {
            _allFeatures = allFeatures;
        }
        
        [HttpGet]
        public IDictionary<string, bool> Get()
        {
            var allFeatures = _allFeatures.Select(x => new { Key = x.GetType().Name, Value = x.FeatureEnabled });

            return allFeatures.ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
