using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Play.Common.Settings;
using System;

namespace Play.Common.Identity
{
    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IConfiguration configuration;
        public ConfigureJwtBearerOptions(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Configure(string name, JwtBearerOptions options)
        {
            if(name == JwtBearerDefaults.AuthenticationScheme)
            {
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings))
                                                   .Get<ServiceSettings>();

                // Authorization server that is only one authorized to provide access tokens
                options.Authority = serviceSettings.Authority;

                // Defines who is intended user of the tokens
                options.Audience = serviceSettings.Name;

                // Without this we will have potential problems integrating with other providers. With this we will getting into correct standard 
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            }
        }

        public void Configure(JwtBearerOptions options)
        {
            this.Configure(options.ForwardDefault, options);
        }
    }
}
