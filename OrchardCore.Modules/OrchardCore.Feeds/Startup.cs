﻿using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Feeds;

namespace OrchardCore.Scripting
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddFeeds();
        }
    }
}
