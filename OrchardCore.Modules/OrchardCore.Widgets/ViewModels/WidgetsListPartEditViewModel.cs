using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Widgets.Models;

namespace OrchardCore.Widgets.ViewModels
{
    public class WidgetsListPartEditViewModel
    {

        public string[] AvailableZones { get; set; } = Array.Empty<string>();

        public string[] Zones { get; set; } = Array.Empty<string>();
        public string[] Prefixes { get; set; } = Array.Empty<string>();
        public string[] ContentTypes { get; set; } = Array.Empty<string>();

        public WidgetsListPart WidgetsListPart { get; set; }

        [BindNever]
        public IUpdateModel Updater { get; set; }
    }
}
