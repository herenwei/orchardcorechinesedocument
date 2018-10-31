using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using OrchardCore.Data;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Hosting.ShellBuilders;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Setup.Services;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Controllers
{
    [Route("api/tenants")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class ApiController : Controller
    {
        private readonly IShellHost _shellHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IEnumerable<DatabaseProvider> _databaseProviders;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;
        private readonly IDataProtectionProvider _dataProtectorProvider;
        private readonly ISetupService _setupService;
        private readonly ShellSettings _currentShellSettings;
        private readonly IClock _clock;
        private readonly INotifier _notifier;

        public ApiController(
            IShellHost shellHost,
            ShellSettings currentShellSettings,
            IAuthorizationService authorizationService,
            IShellSettingsManager shellSettingsManager,
            IEnumerable<DatabaseProvider> databaseProviders,
            IDataProtectionProvider dataProtectorProvider,
            ISetupService setupService,
            IClock clock,
            INotifier notifier,
            IEnumerable<IRecipeHarvester> recipeHarvesters,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer)
        {
            _dataProtectorProvider = dataProtectorProvider;
            _setupService = setupService;
            _clock = clock;
            _recipeHarvesters = recipeHarvesters;
            _shellHost = shellHost;
            _authorizationService = authorizationService;
            _shellSettingsManager = shellSettingsManager;
            _databaseProviders = databaseProviders;
            _currentShellSettings = currentShellSettings;
            _notifier = notifier;

            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public IStringLocalizer S { get; set; }
        public IHtmlLocalizer H { get; set; }

        [HttpPost]
        [Route("create")]        
        public async Task<IActionResult> Create(CreateApiViewModel model)
        {
            if (!IsDefaultShell())
            {
                return Unauthorized();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            var allShells = await GetShellsAsync();

            if (!string.IsNullOrEmpty(model.Name) && !Regex.IsMatch(model.Name, @"^\w+$"))
            {
                ModelState.AddModelError(nameof(CreateApiViewModel.Name), S["Invalid tenant name. Must contain characters only and no spaces."]);
            }

            if (!IsDefaultShell() && string.IsNullOrWhiteSpace(model.RequestUrlHost) && string.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                ModelState.AddModelError(nameof(CreateApiViewModel.RequestUrlPrefix), S["Host and url prefix can not be empty at the same time."]);
            }

            if (!string.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                if (model.RequestUrlPrefix.Contains('/'))
                {
                    ModelState.AddModelError(nameof(CreateApiViewModel.RequestUrlPrefix), S["The url prefix can not contains more than one segment."]);
                }
            }

            if (ModelState.IsValid)
            {
                if (_shellHost.TryGetSettings(model.Name, out var shellSettings))
                {
                    // Site already exists, return 200 for indempotency purpose

                    var token = CreateSetupToken(shellSettings);

                    return StatusCode(201, GetTenantUrl(shellSettings, token));
                }
                else
                {
                    shellSettings = new ShellSettings
                    {
                        Name = model.Name,
                        RequestUrlPrefix = model.RequestUrlPrefix?.Trim(),
                        RequestUrlHost = model.RequestUrlHost,
                        ConnectionString = model.ConnectionString,
                        TablePrefix = model.TablePrefix,
                        DatabaseProvider = model.DatabaseProvider,
                        State = TenantState.Uninitialized,
                        Secret = Guid.NewGuid().ToString(),
                        RecipeName = model.RecipeName
                    };

                    _shellSettingsManager.SaveSettings(shellSettings);
                    var shellContext = await _shellHost.GetOrCreateShellContextAsync(shellSettings);

                    var token = CreateSetupToken(shellSettings);

                    return Ok(GetTenantUrl(shellSettings, token));
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("setup")]
        public async Task<ActionResult> Setup(SetupApiViewModel model)
        {
            if (!IsDefaultShell())
            {
                return Unauthorized();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!_shellHost.TryGetSettings(model.Name, out var shellSettings))
            {
                ModelState.AddModelError(nameof(SetupApiViewModel.Name), S["Tenant not found: '{0}'", model.Name]);
            }

            if (shellSettings.State == TenantState.Running)
            {
                return StatusCode(201);
            }

            if (shellSettings.State != TenantState.Uninitialized)
            {
                return BadRequest(S["The tenant can't be setup."]);
            }

            var selectedProvider = _databaseProviders.FirstOrDefault(x => String.Equals(x.Value, model.DatabaseProvider, StringComparison.OrdinalIgnoreCase));

            if (selectedProvider == null)
            {
                return BadRequest(S["The database provider is not defined."]);
            }

            var tablePrefix = shellSettings.TablePrefix;

            if (String.IsNullOrEmpty(tablePrefix))
            {
                tablePrefix = model.TablePrefix;
            }

            var connectionString = shellSettings.ConnectionString;

            if (String.IsNullOrEmpty(connectionString))
            {
                connectionString = model.ConnectionString;
            }

            if (selectedProvider.HasConnectionString && String.IsNullOrEmpty(connectionString))
            {
                return BadRequest(S["The connection string is required for this database provider."]);
            }

            var recipeName = shellSettings.RecipeName;

            if (String.IsNullOrEmpty(recipeName))
            {
                recipeName = model.RecipeName;
            }

            RecipeDescriptor recipeDescriptor = null;

            if (String.IsNullOrEmpty(recipeName))
            {
                if (model.Recipe == null)
                {
                    return BadRequest(S["Either 'Recipe' or 'RecipeName' is required."]);
                }

                var tempFilename = Path.GetTempFileName();

                using (var fs = System.IO.File.Create(tempFilename))
                {
                    await model.Recipe.CopyToAsync(fs);
                }

                var fileProvider = new PhysicalFileProvider(Path.GetDirectoryName(tempFilename));

                recipeDescriptor = new RecipeDescriptor
                {
                    FileProvider = fileProvider,
                    BasePath = "",
                    RecipeFileInfo = fileProvider.GetFileInfo(Path.GetFileName(tempFilename))
                };
            }
            else
            {
                var setupRecipes = await _setupService.GetSetupRecipesAsync();
                recipeDescriptor = setupRecipes.FirstOrDefault(x => String.Equals(x.Name, recipeName, StringComparison.OrdinalIgnoreCase));

                if (recipeDescriptor == null)
                {
                    return BadRequest(S["Recipe '{0}' not found.", recipeName]);
                }
            }

            var setupContext = new SetupContext
            {
                ShellSettings = shellSettings,
                SiteName = model.SiteName,
                EnabledFeatures = null, // default list,
                AdminUsername = model.UserName,
                AdminEmail = model.Email,
                AdminPassword = model.Password,
                Errors = new Dictionary<string, string>(),
                Recipe = recipeDescriptor,
                SiteTimeZone = model.SiteTimeZone,
                DatabaseProvider = selectedProvider.Name,
                DatabaseConnectionString = connectionString,
                DatabaseTablePrefix = tablePrefix
            };

            var executionId = await _setupService.SetupAsync(setupContext);

            // Check if a component in the Setup failed
            if (setupContext.Errors.Any())
            {
                foreach (var error in setupContext.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }

                return StatusCode(500, ModelState);
            }

            return Ok(executionId);
        }

        private async Task<IEnumerable<ShellContext>> GetShellsAsync()
        {
            return (await _shellHost.ListShellContextsAsync()).OrderBy(x => x.Settings.Name);
        }

        private bool IsDefaultShell()
        {
            return string.Equals(_currentShellSettings.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase);
        }

        public string GetTenantUrl(ShellSettings shellSettings, string token)
        {
            var requestHostInfo = Request.Host;

            var tenantUrlHost = shellSettings.RequestUrlHost?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).First() ?? requestHostInfo.Host;
            if (requestHostInfo.Port.HasValue)
            {
                tenantUrlHost += ":" + requestHostInfo.Port;
            }

            var result = $"{Request.Scheme}://{tenantUrlHost}";

            if (!string.IsNullOrEmpty(shellSettings.RequestUrlPrefix))
            {
                result += "/" + shellSettings.RequestUrlPrefix;
            }

            if (!string.IsNullOrEmpty(token))
            {
                result += "?token=" + WebUtility.UrlEncode(token);
            }

            return result;
        }

        private string CreateSetupToken(ShellSettings shellSettings)
        {
            // Create a public url to setup the new tenant
            var dataProtector = _dataProtectorProvider.CreateProtector("Tokens").ToTimeLimitedDataProtector();
            var token = dataProtector.Protect(shellSettings.Secret, _clock.UtcNow.Add(new TimeSpan(24, 0, 0)));
            return token;
        }
    }
}
