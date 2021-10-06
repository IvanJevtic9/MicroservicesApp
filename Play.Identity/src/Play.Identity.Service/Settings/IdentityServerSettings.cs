using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace Play.Identity.Service.Settings
{
    /*
     * Bunch of collection properties that are going to represent the different things
     * that we have to configure around indetity server
    */
    public class IdentityServerSettings
    {
        // Different types of access to resources that can be granted to our client
        public IReadOnlyCollection<ApiScope> ApiScopes { get; init; }

        public IReadOnlyCollection<ApiResource> ApiResources { get; init; }

        // Going to contain all clients that are going to have some sort of access
        public IReadOnlyCollection<Client> Clients { get; init; }

        public IReadOnlyCollection<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
    }
}
