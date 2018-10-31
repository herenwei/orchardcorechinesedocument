using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Cache
{
    public class CacheContext
    {
        private HashSet<string> _contexts;
        private HashSet<string> _tags;
        private string _cacheId;
        private DateTimeOffset? _expiresOn;
        private TimeSpan? _expiresAfter;
        private TimeSpan? _expiresSliding;

        public CacheContext(string cacheId)
        {
            _cacheId = cacheId;
        }

        /// <summary>
        /// Defines the absolute time the value should be cached for.
        /// If not called a sliding value will be used.
        /// </summary>
        public CacheContext WithExpiryOn(DateTimeOffset expiry)
        {
            _expiresOn = expiry;
            return this;
        }

        /// <summary>
        /// Defines the absolute time (relative from now) the value should be cached for.
        /// If not called a sliding value will be used.
        /// </summary>
        public CacheContext WithExpiryAfter(TimeSpan duration)
        {
            _expiresAfter = duration;
            return this;
        }

        /// <summary>
        /// Defines the sliding expiry time the value should be cached for.
        /// If not called a default sliding value will be used (unless an absolute expiry time has been specified).
        /// </summary>
        public CacheContext WithExpirySliding(TimeSpan window)
        {
            _expiresSliding = window;
            return this;
        }

        /// <summary>
        /// Defines a dimension to cache the shape for. For instance by using <code>"user"</code>
        /// each user will get a different value.
        /// </summary>
        public CacheContext AddContext(params string[] contexts)
        {
            if (_contexts == null)
            {
                _contexts = new HashSet<string>();
            }

            foreach (var context in contexts)
            {
                _contexts.Add(context);
            }

            return this;
        }

        /// <summary>
        /// Removes a specific context.
        /// </summary>
        public CacheContext RemoveContext(string context)
        {
            if (_contexts != null)
            {
                _contexts.Remove(context);
            }

            return this;
        }

        public CacheContext AddTag(params string[] tags)
        {
            if (_tags == null)
            {
                _tags = new HashSet<string>();
            }

            foreach (var tag in tags)
            {
                _tags.Add(tag);
            }

            return this;
        }

        public CacheContext RemoveTag(string tag)
        {
            if (_tags != null)
            {
                _tags.Remove(tag);
            }

            return this;
        }

        public string CacheId => _cacheId;
        public ICollection<string> Contexts => (ICollection<string>) _contexts ?? Array.Empty<string>();
        public IEnumerable<string> Tags => _tags ?? Enumerable.Empty<string>();
        public DateTimeOffset? ExpiresOn => _expiresOn;
        public TimeSpan? ExpiresAfter => _expiresAfter;
        public TimeSpan? ExpiresSliding => _expiresSliding;

    }
}
