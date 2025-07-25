using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp", "Auction app full access")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientId ="postman",
                ClientName = "postman",
                AllowedScopes ={"openid", "profile", "auctionApp"},
                RedirectUris = {"www.getpostman.com/oauth2/callback"},
                ClientSecrets = new[]{new Secret("NotASecret".Sha256())},
                AllowedGrantTypes = {GrantType.ResourceOwnerPassword}
            },
            new Client
            {
                ClientId ="nextApp",
                ClientName = "nextApp",
                AllowedScopes ={"openid", "profile", "auctionApp"},
                RedirectUris = {"http://localhost:3000/api/auth/callback/id-server"},
                ClientSecrets = {new Secret("NotASecret".Sha256())},
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                AllowOfflineAccess = true,
                AccessTokenLifetime = 3600*24*30,
                RequirePkce = false
            }           
        };
}
