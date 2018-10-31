using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Events;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.LocationExpander;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Mvc.LocationExpander;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host and tenant level services for managing themes.
        /// </summary>
        public static OrchardCoreBuilder AddTheming(this OrchardCoreBuilder builder)
        {
            builder.AddThemingHost()
                .ConfigureServices(services =>
                {
                    services.Configure<MvcOptions>((options) =>
                    {
                        options.Filters.Add(typeof(ModelBinderAccessorFilter));
                        options.Filters.Add(typeof(NotifyFilter));
                        options.Filters.Add(typeof(SiteViewResultFilter));
                    });

                    services.AddScoped<IUpdateModelAccessor, LocalModelBinderAccessor>();

                    services.AddScoped<IShapeTemplateViewEngine, RazorShapeTemplateViewEngine>();
                    services.AddSingleton<IApplicationFeatureProvider<ViewsFeature>, ThemingViewsFeatureProvider>();
                    services.AddScoped<IViewLocationExpanderProvider, ThemeViewLocationExpanderProvider>();

                    services.AddScoped<IShapeTemplateHarvester, BasicShapeTemplateHarvester>();
                    services.AddTransient<IShapeTableManager, DefaultShapeTableManager>();

                    services.AddScoped<IShapeTableProvider, ShapeAttributeBindingStrategy>();
                    services.AddScoped<IShapeTableProvider, ShapePlacementParsingStrategy>();
                    services.AddScoped<IShapeTableProvider, ShapeTemplateBindingStrategy>();

                    services.TryAddEnumerable(
                        ServiceDescriptor.Transient<IConfigureOptions<ShapeTemplateOptions>, ShapeTemplateOptionsSetup>());
                    services.TryAddSingleton<IShapeTemplateFileProviderAccessor, ShapeTemplateFileProviderAccessor>();

                    services.AddShapeAttributes<CoreShapes>();
                    services.AddScoped<IShapeTableProvider, CoreShapesTableProvider>();
                    services.AddShapeAttributes<ZoneShapes>();
                    services.AddScoped<IShapeTableProvider, LayoutShapes>();

                    services.AddScoped<IHtmlDisplay, DefaultHtmlDisplay>();
                    services.AddScoped<ILayoutAccessor, LayoutAccessor>();
                    services.AddScoped<IThemeManager, ThemeManager>();
                    services.AddScoped<IPageTitleBuilder, PageTitleBuilder>();

                    services.AddScoped<IShapeFactory, DefaultShapeFactory>();
                    services.AddScoped<IDisplayHelperFactory, DisplayHelperFactory>();

                    services.AddScoped<INotifier, Notifier>();

                    services.AddScoped(typeof(IPluralStringLocalizer<>), typeof(PluralStringLocalizer<>));
                    services.AddShapeAttributes<DateTimeShapes>();

                    services.AddTagHelpers<AddAlternateTagHelper>();
                    services.AddTagHelpers<AddClassTagHelper>();
                    services.AddTagHelpers<AddWrapperTagHelper>();
                    services.AddTagHelpers<BaseShapeTagHelper>();
                    services.AddTagHelpers<ClearAlternatesTagHelper>();
                    services.AddTagHelpers<ClearClassesTagHelper>();
                    services.AddTagHelpers<ClearWrappersTagHelper>();
                    services.AddTagHelpers<InputIsDisabledTagHelper>();
                    services.AddTagHelpers<RemoveAlternateTagHelper>();
                    services.AddTagHelpers<RemoveClassTagHelper>();
                    services.AddTagHelpers<RemoveWrapperTagHelper>();
                    services.AddTagHelpers<ShapeMetadataTagHelper>();
                    services.AddTagHelpers<ShapeTagHelper>();
                    services.AddTagHelpers<ValidationMessageTagHelper>();
                    services.AddTagHelpers<ZoneTagHelper>();
                });

            return builder;
        }

        /// <summary>
        /// Adds host level services for managing themes.
        /// </summary>
        public static OrchardCoreBuilder AddThemingHost(this OrchardCoreBuilder builder)
        {
            var services = builder.ApplicationServices;

            services.AddSingleton<IExtensionDependencyStrategy, ThemeExtensionDependencyStrategy>();
            services.AddSingleton<IFeatureBuilderEvents, ThemeFeatureBuilderEvents>();

            return builder;
        }
    }
}
