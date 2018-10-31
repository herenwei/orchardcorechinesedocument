using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing.Services;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IIndexingTaskManager, IndexingTaskManager>();
            services.AddScoped<IContentHandler, CreateIndexingTaskContentHandler>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
