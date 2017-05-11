using FeatureToggle;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Features.Custom
{
    public abstract class BaseFeature : SimpleFeatureToggle, IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (!FeatureEnabled)
            {
                StopExecution(context);
            }
        }

        private static void StopExecution(ResourceExecutingContext context)
        {
            context.Result = new ContentResult()
            {
                Content = "Resource unavailable - feature disabled"
            };
        }
    }
}
