using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Html;

namespace OrchardCore.Indexing
{
    public class DocumentIndex
    {
        public DocumentIndex(string contentItemId)
        {
            ContentItemId = contentItemId;
        }

        public List<DocumentIndexEntry> Entries { get; } = new List<DocumentIndexEntry>();

        public void Set(string name, string value, DocumentIndexOptions options)
        {
            Entries.Add(new DocumentIndexEntry(name, value, Types.Text, options));
        }

        public void Set(string name, IHtmlContent value, DocumentIndexOptions options)
        {
            Entries.Add(new DocumentIndexEntry(name, value, Types.Text, options));
        }

        public void Set(string name, DateTimeOffset? value, DocumentIndexOptions options)
        {
            Entries.Add(new DocumentIndexEntry(name, value, Types.DateTime, options));
        }

        public void Set(string name, int? value, DocumentIndexOptions options)
        {
            Entries.Add(new DocumentIndexEntry(name, value, Types.Integer, options));
        }

        public void Set(string name, bool? value, DocumentIndexOptions options)
        {
            Entries.Add(new DocumentIndexEntry(name, value, Types.Boolean, options));
        }

        public void Set(string name, double? value, DocumentIndexOptions options)
        {
            Entries.Add(new DocumentIndexEntry(name, value, Types.Number, options));
        }

        public void Set(string name, decimal? value, DocumentIndexOptions options)
        {
            Entries.Add(new DocumentIndexEntry(name, value, Types.Number, options));
        }

        public string ContentItemId { get; }

        public enum Types
        {
            Integer,
            Text,
            DateTime,
            Boolean,
            Number
        }

        public class DocumentIndexEntry
        {
            public DocumentIndexEntry(string name, object value, Types type, DocumentIndexOptions options)
            {
                Name = name;
                Value = value;
                Type = type;
                Options = options;
            }

            public string Name { get; }
            public object Value { get; }
            public Types Type { get; }
            public DocumentIndexOptions Options { get; }
        }
    }
}
