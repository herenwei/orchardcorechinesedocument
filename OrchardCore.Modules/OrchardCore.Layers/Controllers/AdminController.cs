using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Layers.Handlers;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Layers.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly ISiteService _siteService;
		private readonly ILayerService _layerService;
		private readonly IAuthorizationService _authorizationService;
		private readonly ISession _session;
		private readonly ISignal _signal;
		private readonly INotifier _notifier;

		public AdminController(
			ISignal signal,
			IAuthorizationService authorizationService,
			ISession session,
			ILayerService layerService,
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            ISiteService siteService,
			IStringLocalizer<AdminController> s,
			IHtmlLocalizer<AdminController> h,
			INotifier notifier
			)
        {
			_signal = signal;
			_authorizationService = authorizationService;
			_session = session;
			_layerService = layerService;
            _contentManager = contentManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _siteService = siteService;
			S = s;
			H = h;
			_notifier = notifier;
		}

		public IStringLocalizer S { get; }
		public IHtmlLocalizer H { get; }

		public async Task<IActionResult> Index()
        {
			if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
			{
				return Unauthorized();
			}

			var layers = await _layerService.GetLayersAsync();
			var widgets = await _layerService.GetLayerWidgetsAsync(c => c.Latest == true);

			var model = new LayersIndexViewModel { Layers = layers.Layers };

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            model.Zones = siteSettings.As<LayerSettings>().Zones ?? Array.Empty<string>();
            model.Widgets = new Dictionary<string, List<dynamic>>();

            foreach (var widget in widgets.OrderBy(x => x.Position))
            {
				var zone = widget.Zone;
				List <dynamic> list;
				if (!model.Widgets.TryGetValue(zone, out list))
				{
					model.Widgets.Add(zone, list = new List<dynamic>());
				}

                list.Add(await _contentItemDisplayManager.BuildDisplayAsync(widget.ContentItem, this, "SummaryAdmin"));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(LayersIndexViewModel model)
        {
			if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
			{
				return Unauthorized();
			}

			return RedirectToAction("Index");
        }

		public async Task<IActionResult> Create()
		{
			if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
			{
				return Unauthorized();
			}

			return View();
		}

		[HttpPost, ActionName("Create")]
		public async Task<IActionResult> CreatePost(LayerEditViewModel model)
		{
			if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
			{
				return Unauthorized();
			}

			var layers = await _layerService.GetLayersAsync();

			ValidateViewModel(model, layers, isNew: true);

			if (ModelState.IsValid)
			{
				layers.Layers.Add(new Layer
				{
					Name = model.Name,
					Rule = model.Rule,
					Description = model.Description
				});

				await _layerService.UpdateAsync(layers);

				return RedirectToAction("Index");
			}

			return View(model);			
		}

		public async Task<IActionResult> Edit(string name)
		{
			if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
			{
				return Unauthorized();
			}

			var layers = await _layerService.GetLayersAsync();

			var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, name));

			if (layer == null)
			{
				return NotFound();
			}

			var model = new LayerEditViewModel
			{
				Name = layer.Name,
				Rule = layer.Rule,
				Description = layer.Description
			};

			return View(model);
		}

		[HttpPost, ActionName("Edit")]
		public async Task<IActionResult> EditPost(LayerEditViewModel model)
		{
			if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
			{
				return Unauthorized();
			}

			var layers = await _layerService.GetLayersAsync();

			ValidateViewModel(model, layers, isNew: false);

			if (ModelState.IsValid)
			{
				var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, model.Name));

				if (layer == null)
				{
					return NotFound();
				}

				layer.Name = model.Name;
				layer.Rule = model.Rule;
				layer.Description = model.Description;

				await _layerService.UpdateAsync(layers);

				return RedirectToAction("Index");
			}

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(string name)
		{
			if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
			{
				return Unauthorized();
			}

			var layers = await _layerService.GetLayersAsync();

			var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, name));

			if (layer == null)
			{
				return NotFound();
			}

			var widgets = await _layerService.GetLayerWidgetsAsync(c => c.Latest == true);

			if (!widgets.Any(x => String.Equals(x.Layer, name, StringComparison.OrdinalIgnoreCase)))
			{
				layers.Layers.Remove(layer);
				_notifier.Success(H["Layer deleted successfully."]);
			}
			else
			{
				_notifier.Error(H["The layer couldn't be deleted: you must remove any associated widgets first."]);
			}
			
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> UpdatePosition(string contentItemId, double position, string zone)
		{
			if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
			{
				return StatusCode(401);
			}

			// Load the latest version first if any
			var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

			if (contentItem == null)
			{
				return StatusCode(404);
			}

			var layerMetadata = contentItem.As<LayerMetadata>();

			if (layerMetadata == null)
			{
				return StatusCode(403);
			}

			layerMetadata.Position = position;
			layerMetadata.Zone = zone;

			contentItem.Apply(layerMetadata);

			_session.Save(contentItem);

            // In case the moved contentItem is the draft for a published contentItem we update it's position too.
            // We do that because we want the position of published and draft version to be the same.
            if (contentItem.IsPublished() == false)
            {
                var publishedContentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Published);
                if (publishedContentItem != null)
                {
                    layerMetadata = contentItem.As<LayerMetadata>();

                    if (layerMetadata == null)
                    {
                        return StatusCode(403);
                    }

                    layerMetadata.Position = position;
                    layerMetadata.Zone = zone;

                    publishedContentItem.Apply(layerMetadata);

                    _session.Save(publishedContentItem);
                }
            }

            // Clear the cache
            _signal.SignalToken(LayerMetadataHandler.LayerChangeToken);
			
			if (Request.Headers != null && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return StatusCode(200); 
			}
			else
			{
				return RedirectToAction("Index");
			}			
		}

		private void ValidateViewModel(LayerEditViewModel model, LayersDocument layers, bool isNew)
		{
			if (String.IsNullOrWhiteSpace(model.Name))
			{
				ModelState.AddModelError(nameof(LayerEditViewModel.Name), S["The layer name is required."]);
			}
			else if (isNew && layers.Layers.Any(x => String.Equals(x.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
			{
				ModelState.AddModelError(nameof(LayerEditViewModel.Name), S["The layer name already exists."]);
			}

			if (String.IsNullOrWhiteSpace(model.Rule))
			{
				ModelState.AddModelError(nameof(LayerEditViewModel.Rule), S["The rule is required."]);
			}
		}
	}
}
