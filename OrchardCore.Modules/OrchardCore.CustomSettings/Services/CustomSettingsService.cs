using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Settings;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace OrchardCore.CustomSettings.Services
{
    public class CustomSettingsService
    {
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly Lazy<IDictionary<string, ContentTypeDefinition>> _settingsTypes;

        public CustomSettingsService(
            ISiteService siteService,
            IContentManager contentManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IContentDefinitionManager contentDefinitionManager)
        {
            _siteService = siteService;
            _contentManager = contentManager;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _contentDefinitionManager = contentDefinitionManager;
            _settingsTypes = new Lazy<IDictionary<string, ContentTypeDefinition>>(
                () => _contentDefinitionManager
                     .ListTypeDefinitions()
                     .Where(x => x.Settings.ToObject<ContentTypeSettings>().Stereotype == "CustomSettings")
                     .ToDictionary(x => x.Name));
        }

        public IEnumerable<string> GetAllSettingsTypeNames()
        {
            return _settingsTypes.Value.Keys;
        }

        public IEnumerable<ContentTypeDefinition> GetAllSettingsTypes()
        {
            return _settingsTypes.Value.Values;
        }

        public IEnumerable<ContentTypeDefinition> GetSettingsTypes(params string[] settingsTypeNames)
        {
            foreach (var settingsTypeName in settingsTypeNames)
            {
                ContentTypeDefinition settingsType;
                if (_settingsTypes.Value.TryGetValue(settingsTypeName, out settingsType))
                {
                    yield return settingsType;
                }
            }
        }

        public ContentTypeDefinition GetSettingsType(string settingsTypeName)
        {
            ContentTypeDefinition settingsType;

            _settingsTypes.Value.TryGetValue(settingsTypeName, out settingsType);

            return settingsType;
        }

        public async Task<bool> CanUserCreateSettingsAsync(ContentTypeDefinition settingsType)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return await _authorizationService.AuthorizeAsync(user, Permissions.CreatePermissionForType(settingsType));
        }

        public Task<ContentItem> GetSettingsAsync(string settingsTypeName, Action isNew = null)
        {
            var settingsType = GetSettingsType(settingsTypeName);
            if (settingsType == null)
            {
                return Task.FromResult<ContentItem>(null);
            }

            return GetSettingsAsync(settingsType, isNew);
        }

        public async Task<ContentItem> GetSettingsAsync(ContentTypeDefinition settingsType, Action isNew = null)
        {
            var site = await _siteService.GetSiteSettingsAsync();

            return await GetSettingsAsync(site, settingsType, isNew);
        }

        public async Task<ContentItem> GetSettingsAsync(ISite site, ContentTypeDefinition settingsType, Action isNew = null)
        {
            JToken property;
            ContentItem contentItem;

            if (site.Properties.TryGetValue(settingsType.Name, out property))
            {
                // Create existing content item
                contentItem = property.ToObject<ContentItem>();
            }
            else
            {
                contentItem = await _contentManager.NewAsync(settingsType.Name);
                isNew?.Invoke();
            }

            return contentItem;
        }
    }
}