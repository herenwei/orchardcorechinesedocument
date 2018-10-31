﻿using System;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement.TagHelpers
{

    [HtmlTargetElement("meta", Attributes = NameAttributeName)]
    public class MetaTagHelper : TagHelper
    {
        private const string NameAttributeName = "asp-name";

        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }

        public string Content { get; set; }

        public string HttpEquiv { get; set; }

        public string Charset { get; set; }

        public string Separator { get; set; }

        private readonly IResourceManager _resourceManager;

        public MetaTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var metaEntry = new MetaEntry(Name, Content, HttpEquiv, Charset);

            foreach (var attribute in output.Attributes)
            {
                if (String.Equals(attribute.Name, "name", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                metaEntry.SetAttribute(attribute.Name, attribute.Value.ToString());
            }

            _resourceManager.AppendMeta(metaEntry, Separator ?? ", ");

            output.TagName = null;
        }
    }
}
