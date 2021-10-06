using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Play.Common.Identity
{
    public static class Extensions
    {
        public static AuthenticationBuilder AddJwtBererAuthentication(this IServiceCollection services)
        {
            // ConfigureJwtBearerOptions will handle Audiance and name in JwtBearer
            return services.ConfigureOptions<ConfigureJwtBearerOptions>()
                           .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                           .AddJwtBearer(); 
        }
    }
}
