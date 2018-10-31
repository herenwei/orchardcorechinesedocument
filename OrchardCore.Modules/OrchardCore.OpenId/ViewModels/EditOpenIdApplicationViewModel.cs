using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.OpenId.Validators;

namespace OrchardCore.OpenId.ViewModels
{
    public class EditOpenIdApplicationViewModel : IValidatableObject
    {
        [HiddenInput]
        public string Id { get; set; }
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string DisplayName { get; set; }
        public string RedirectUris { get; set; }
        public string PostLogoutRedirectUris { get; set; }
        public string Type { get; set; }
        public string ConsentType { get; set; }
        public bool UpdateClientSecret { get; set; }
        public string ClientSecret { get; set; }
        public List<RoleEntry> RoleEntries { get; } = new List<RoleEntry>();
        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool AllowLogoutEndpoint { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return OpenIdUrlValidator.ValidateUrls(nameof(RedirectUris), RedirectUris)
                .Union(OpenIdUrlValidator.ValidateUrls(nameof(PostLogoutRedirectUris), PostLogoutRedirectUris));
        }

        public class RoleEntry
        {
            public string Name { get; set; }
            public bool Selected { get; set; }
        }
    }
}
