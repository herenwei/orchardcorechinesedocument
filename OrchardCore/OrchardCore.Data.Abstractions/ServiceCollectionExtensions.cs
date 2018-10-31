using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TryAddDataProvider(this IServiceCollection services, string name, string value, bool hasConnectionString, bool hasTablePrefix, bool isDefault)
        {
            for (var i = services.Count - 1; i >= 0; i--)
            {
                var entry = services[i];
                if (entry.ImplementationInstance != null)
                {
                    var databaseProvider = entry.ImplementationInstance as DatabaseProvider;
                    if (databaseProvider != null && String.Equals(databaseProvider.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        services.RemoveAt(i);
                    }
                }
            }

            services.AddSingleton(new DatabaseProvider { Name = name, Value = value, HasConnectionString = hasConnectionString, HasTablePrefix = hasTablePrefix , IsDefault = isDefault });

            return services;
        }
    }
}
