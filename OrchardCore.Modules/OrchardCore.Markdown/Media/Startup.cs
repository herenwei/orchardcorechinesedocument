using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Markdown.Media
{
    [RequireFeatures("OrchardCore.Media")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShapeTableProvider, MediaShapes>();
        }
    }
}
