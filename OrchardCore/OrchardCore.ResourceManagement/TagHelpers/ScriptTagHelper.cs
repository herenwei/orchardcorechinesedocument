using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement.TagHelpers
{

    [HtmlTargetElement("script", Attributes = NameAttributeName)]
    [HtmlTargetElement("script", Attributes = SrcAttributeName)]
    [HtmlTargetElement("script", Attributes = AtAttributeName)]
    public class ScriptTagHelper : TagHelper
    {
        private const string NameAttributeName = "asp-name";
        private const string SrcAttributeName = "asp-src";
        private const string AtAttributeName = "at";

        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }

        [HtmlAttributeName(SrcAttributeName)]
        public string Source { get; set; }

        public string CdnSrc { get; set; }
        public string DebugSrc { get; set; }
        public string DebugCdnSrc { get; set; }

        public bool UseCdn { get; set; }
        public string Condition { get; set; }
        public string Culture { get; set; }
        public bool Debug { get; set; }
        public string DependsOn { get; set; }
        public string Version { get; set; }

        [HtmlAttributeName(AtAttributeName)]
        public ResourceLocation At { get; set; }

        private readonly IResourceManager _resourceManager;

        public ScriptTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            if (String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Source))
            {
                RequireSettings setting;

                if (String.IsNullOrEmpty(DependsOn))
                {
                    // Include custom script url
                    setting = _resourceManager.Include("script", Source, DebugSrc);
                }
                else
                {
                    // Anonymous declaration with dependencies, then display

                    // Using the source as the name to prevent duplicate references to the same file
                    var name = Source.ToLowerInvariant();

                    var definition = _resourceManager.InlineManifest.DefineScript(name);
                    definition.SetUrl(Source, DebugSrc);

                    if (!String.IsNullOrEmpty(Version))
                    {
                        definition.SetVersion(Version);
                    }

                    if (!String.IsNullOrEmpty(CdnSrc))
                    {
                        definition.SetCdn(CdnSrc, DebugCdnSrc);
                    }

                    if (!String.IsNullOrEmpty(Culture))
                    {
                        definition.SetCultures(Culture.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    if (!String.IsNullOrEmpty(DependsOn))
                    {
                        definition.SetDependencies(DependsOn.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    if (!String.IsNullOrEmpty(Version))
                    {
                        definition.SetVersion(Version);
                    }

                    setting = _resourceManager.RegisterResource("script", name);
                }

                if (At != ResourceLocation.Unspecified)
                {
                    setting.AtLocation(At);
                }

                if (!String.IsNullOrEmpty(Condition))
                {
                    setting.UseCondition(Condition);
                }

                setting.UseDebugMode(Debug);

                if (!String.IsNullOrEmpty(Culture))
                {
                    setting.UseCulture(Culture);
                }

                foreach (var attribute in output.Attributes)
                {
                    setting.SetAttribute(attribute.Name, attribute.Value.ToString());
                }
            }
            else if (!String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Source))
            {
                // Resource required

                var setting = _resourceManager.RegisterResource("script", Name);

                if (At != ResourceLocation.Unspecified)
                {
                    setting.AtLocation(At);
                }

                setting.UseCdn(UseCdn);

                if (!String.IsNullOrEmpty(Condition))
                {
                    setting.UseCondition(Condition);
                }

                setting.UseDebugMode(Debug);

                if (!String.IsNullOrEmpty(Culture))
                {
                    setting.UseCulture(Culture);
                }

                if (!String.IsNullOrEmpty(Version))
                {
                    setting.UseVersion(Version);
                }
            }
            else if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Source))
            {
                // Inline declaration

                var definition = _resourceManager.InlineManifest.DefineScript(Name);
                definition.SetUrl(Source, DebugSrc);

                if (!String.IsNullOrEmpty(Version))
                {
                    definition.SetVersion(Version);
                }

                if (!String.IsNullOrEmpty(CdnSrc))
                {
                    definition.SetCdn(CdnSrc, DebugCdnSrc);
                }

                if (!String.IsNullOrEmpty(Culture))
                {
                    definition.SetCultures(Culture.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                if (!String.IsNullOrEmpty(DependsOn))
                {
                    definition.SetDependencies(DependsOn.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                if (!String.IsNullOrEmpty(Version))
                {
                    definition.SetVersion(Version);
                }

                // If At is specified then we also render it
                if (At != ResourceLocation.Unspecified)
                {
                    var setting = _resourceManager.RegisterResource("script", Name);

                    setting.AtLocation(At);

                    if (!String.IsNullOrEmpty(Condition))
                    {
                        setting.UseCondition(Condition);
                    }

                    setting.UseDebugMode(Debug);

                    if (!String.IsNullOrEmpty(Culture))
                    {
                        setting.UseCulture(Culture);
                    }

                    foreach (var attribute in output.Attributes)
                    {
                        setting.SetAttribute(attribute.Name, attribute.Value.ToString());
                    }
                }
            }
            else if (String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Source))
            {
                // Custom script content

                var childContent = await output.GetChildContentAsync();

                var builder = new TagBuilder("script");
                builder.InnerHtml.AppendHtml(childContent);
                builder.TagRenderMode = TagRenderMode.Normal;

                foreach (var attribute in output.Attributes)
                {
                    builder.Attributes.Add(attribute.Name, attribute.Value.ToString());
                }

                // If no type was specified, define a default one
                if (!builder.Attributes.ContainsKey("type"))
                {
                    builder.Attributes.Add("type", "text/javascript");
                }

                if (At == ResourceLocation.Head)
                {
                    _resourceManager.RegisterHeadScript(builder);
                }
                else
                {
                    _resourceManager.RegisterFootScript(builder);
                }
            }
        }
    }
}
