﻿using System.Collections.Specialized;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.ViewModels
{
    public class ListPartSettingsViewModel
    {
        public ListPartSettings ListPartSettings { get; set; }
        public NameValueCollection ContentTypes { get; set; }
        public string[] ContainedContentTypes { get; set; }
        public int PageSize { get; set; }
    }
}
