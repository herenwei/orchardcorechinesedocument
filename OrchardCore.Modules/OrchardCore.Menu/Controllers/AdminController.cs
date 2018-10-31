using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Menu.Models;
using YesSql;

namespace OrchardCore.Menu.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly INotifier _notifier;

        public AdminController(
            ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IHtmlLocalizer<AdminController> h)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _notifier = notifier;
            H = h;
        }

        public IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Create(string id, string menuContentItemId, string menuItemId)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
            {
                return Unauthorized();
            }

            var contentItem = await _contentManager.NewAsync(id);

            dynamic model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this, true);

            model.MenuContentItemId = menuContentItemId;
            model.MenuItemId = menuItemId;

            return View(model);
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(string id, string menuContentItemId, string menuItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
            {
                return Unauthorized();
            }

            ContentItem menu;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Menu");

            if (!contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Draftable)
            {
                menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.Latest);
            }
            else
            {
                menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.DraftRequired);
            }

            if (menu == null)
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(id);

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, true);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (menuItemId == null)
            {
                // Use the menu as the parent if no target is specified
                menu.Alter<MenuItemsListPart>(part => part.MenuItems.Add(contentItem));
            }
            else
            {
                // Look for the target menu item in the hierarchy
                var parentMenuItem = FindMenuItem(menu.Content, menuItemId);

                // Couldn't find targeted menu item
                if (parentMenuItem == null)
                {
                    return NotFound();
                }

                var menuItems = parentMenuItem?.MenuItemsListPart?.MenuItems as JArray;

                if (menuItems == null)
                {
                    parentMenuItem["MenuItemsListPart"] = new JObject(
                        new JProperty("MenuItems", menuItems = new JArray())
                        );
                }

                menuItems.Add(JObject.FromObject(contentItem));
            }

            _session.Save(menu);

            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = menuContentItemId });
        }

        public async Task<IActionResult> Edit(string menuContentItemId, string menuItemId)
        {
            var menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.Latest);

            if (menu == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu, menu))
            {
                return Unauthorized();
            }

            // Look for the target menu item in the hierarchy
            JObject menuItem = FindMenuItem(menu.Content, menuItemId);

            // Couldn't find targetted menu item
            if (menuItem == null)
            {
                return NotFound();
            }

            var contentItem = menuItem.ToObject<ContentItem>();

            dynamic model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this, false);

            model.MenuContentItemId = menuContentItemId;
            model.MenuItemId = menuItemId;

            return View(model);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(string menuContentItemId, string menuItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
            {
                return Unauthorized();
            }

            ContentItem menu;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Menu");

            if (!contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Draftable)
            {
                menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.Latest);
            }
            else
            {
                menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.DraftRequired);
            }

            if (menu == null)
            {
                return NotFound();
            }

            // Look for the target menu item in the hierarchy
            JObject menuItem = FindMenuItem(menu.Content, menuItemId);

            // Couldn't find targetted menu item
            if (menuItem == null)
            {
                return NotFound();
            }

            var contentItem = menuItem.ToObject<ContentItem>();

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, false);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            menuItem.Merge(contentItem.Content, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            _session.Save(menu);

            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = menuContentItemId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string menuContentItemId, string menuItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
            {
                return Unauthorized();
            }

            ContentItem menu;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Menu");

            if (!contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Draftable)
            {
                menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.Latest);
            }
            else
            {
                menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.DraftRequired);
            }

            if (menu == null)
            {
                return NotFound();
            }

            // Look for the target menu item in the hierarchy
            var menuItem = FindMenuItem(menu.Content, menuItemId);

            // Couldn't find targetted menu item
            if (menuItem == null)
            {
                return NotFound();
            }

            menuItem.Remove();
            _session.Save(menu);

            _notifier.Success(H["Menu item deleted successfully"]);

            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = menuContentItemId });
        }

        private JObject FindMenuItem(JObject contentItem, string menuItemId)
        {
            if (contentItem["ContentItemId"]?.Value<string>() == menuItemId)
            {
                return contentItem;
            }

            if (contentItem.GetValue("MenuItemsListPart") == null)
            {
                return null;
            }

            var menuItems = (JArray)contentItem["MenuItemsListPart"]["MenuItems"];

            JObject result;

            foreach (JObject menuItem in menuItems)
            {
                // Search in inner menu items
                result = FindMenuItem(menuItem, menuItemId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
