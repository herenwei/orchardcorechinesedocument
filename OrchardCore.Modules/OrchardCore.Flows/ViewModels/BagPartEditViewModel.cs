using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.ViewModels
{
    public class BagPartEditViewModel
    {
        public string[] Prefixes { get; set; } = Array.Empty<string>();
        public string[] ContentTypes { get; set; } = Array.Empty<string>();

        [BindNever]
        public BagPart BagPart { get; set; }

        [BindNever]
        public IUpdateModel Updater { get; set; }

        [BindNever]
        public IEnumerable<ContentTypeDefinition> ContainedContentTypeDefinitions { get; set; }
    }
}
