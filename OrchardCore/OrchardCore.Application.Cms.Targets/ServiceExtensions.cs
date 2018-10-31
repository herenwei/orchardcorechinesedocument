using System;
using OrchardCore.ResourceManagement.TagHelpers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds Orchard CMS services to the application. 
        /// </summary>
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            return AddOrchardCms(services, null);
        }

        /// <summary>
        /// Adds Orchard CMS services to the application and let the app change the
        /// default tenant behavior and set of features through a configure action.
        /// </summary>
        public static IServiceCollection AddOrchardCms(this IServiceCollection services, Action<OrchardCoreBuilder> configure)
        {
            var builder = services.AddOrchardCore()

                .AddCommands()

                .AddMvc()

                .AddSetupFeatures("OrchardCore.Setup")

                .AddDataAccess()
                .AddDataStorage()
                .AddBackgroundTasks()
                .AddDeferredTasks()

                .AddTheming()
                .AddLiquidViews()
                .AddCaching();

            // OrchardCoreBuilder is not available in OrchardCore.ResourceManagement as it has to
            // remain independent from OrchardCore.
            builder.ConfigureServices(s =>
            {
                s.AddResourceManagement();

                services.AddTagHelpers<LinkTagHelper>();
                services.AddTagHelpers<MetaTagHelper>();
                services.AddTagHelpers<ResourcesTagHelper>();
                services.AddTagHelpers<ScriptTagHelper>();
                services.AddTagHelpers<StyleTagHelper>();
            });


            configure?.Invoke(builder);

            return services;
        }
    }
}
