﻿using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Indexing
{
    public class BuildIndexContext
    {
        public BuildIndexContext(
            DocumentIndex documentIndex, 
            ContentItem contentItem,
            IList<string> keys)
        {
            ContentItem = contentItem;
            DocumentIndex = documentIndex;
            Keys = keys;
        }

        public IList<string> Keys { get; }
        public ContentItem ContentItem { get; }
        public DocumentIndex DocumentIndex { get; }
    }
}
