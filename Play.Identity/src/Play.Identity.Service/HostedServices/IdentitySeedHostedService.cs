using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace Play.Identity.Service.HostedServices
{
    public class IdentitySeedHostedService : IHostedService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IdentitySettings identitySettings;

        public IdentitySeedHostedService(IServiceScopeFactory serviceScopeFactory, IOptions<IdentitySettings> identityOptions)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            identitySettings = identityOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceScopeFactory.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await CreateRoleIfNotExistAsync(Roles.ADMIN, roleManager);
            await CreateRoleIfNotExistAsync(Roles.PLAYER, roleManager);

            var adminUser = await userManager.FindByEmailAsync(identitySettings.AdminUserEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser()
                {
                    UserName = identitySettings.AdminUserEmail,
                    Email = identitySettings.AdminUserEmail
                };

                await userManager.CreateAsync(adminUser, identitySettings.AdminUserPassword);
                await userManager.AddToRoleAsync(adminUser, Roles.ADMIN);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static async Task CreateRoleIfNotExistAsync(string role, RoleManager<ApplicationRole> roleManager)
        {
            var roleExist = await roleManager.RoleExistsAsync(role);

            if (!roleExist)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }
    }
}
