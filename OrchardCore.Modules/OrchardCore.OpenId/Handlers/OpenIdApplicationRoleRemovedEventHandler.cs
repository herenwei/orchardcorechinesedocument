using System.Threading.Tasks;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.Security;

namespace OrchardCore.OpenId.Handlers
{
    public class OpenIdApplicationRoleRemovedEventHandler : IRoleRemovedEventHandler
    {
        private readonly IOpenIdApplicationManager _manager;

        public OpenIdApplicationRoleRemovedEventHandler(IOpenIdApplicationManager manager)
        {
            _manager = manager;
        }

        public async Task RoleRemovedAsync(string roleName)
        {
            foreach (var application in await _manager.ListInRoleAsync(roleName))
            {
                await _manager.RemoveFromRoleAsync(application, roleName);
            }
        }
    }
}