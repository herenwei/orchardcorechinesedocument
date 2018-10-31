using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.OpenId.Settings
{
    public class OpenIdServerSettings
    {
        public bool TestingModeEnabled { get; set; }
        public TokenFormat AccessTokenFormat { get; set; }
        public string Authority { get; set; }
        public StoreLocation? CertificateStoreLocation { get; set; }
        public StoreName? CertificateStoreName { get; set; }
        public string CertificateThumbprint { get; set; }
        public PathString AuthorizationEndpointPath { get; set; }
        public PathString LogoutEndpointPath { get; set; }
        public PathString TokenEndpointPath { get; set; }
        public PathString UserinfoEndpointPath { get; set; }
        public ISet<string> GrantTypes { get; } = new HashSet<string>(StringComparer.Ordinal);
        public bool UseRollingTokens { get; set; }

        public enum TokenFormat
        {
            Encrypted = 0,
            JWT = 1
        }
    }
}
