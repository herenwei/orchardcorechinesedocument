using System;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Liquid.Filters
{

    public class ContainerFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public ContainerFilter(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var contentItem = input.ToObjectValue() as ContentItem;

            if (contentItem == null)
            {
                throw new ArgumentException("A Content Item was expected");
            }

            string containerId = contentItem.Content?.ContainedPart?.ListContentItemId;

            if (containerId != null)
            {
                var container = await _contentManager.GetAsync(containerId);

                if (container != null)
                {
                    return new ObjectValue(container);
                }
            }

            return new ObjectValue(contentItem);
        }
    }
}
