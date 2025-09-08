using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model;
using GeekShopping.ProductAPI.Models.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MySqlContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, MySqlContext context)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;            
        }

        public void Initialize()
        {
            if (_roleManager.FindByNameAsync(IdentityConfiguration.Admin).Result != null)
                return;
            _roleManager.CreateAsync(new IdentityRole(IdentityConfiguration.Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(IdentityConfiguration.Client)).GetAwaiter().GetResult();

            ApplicationUser admin = new ApplicationUser()
            {
                UserName = "andre-admin",
                Email = "andre-admin@email.com.br",
                EmailConfirmed = true,
                PhoneNumber = "+55 (11) 12345-6789",
                FirstName = "André",
                LastName = "Admin"
            };

            _userManager.CreateAsync(admin,"12345678@Andre").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(admin, IdentityConfiguration.Admin).GetAwaiter().GetResult();

            var adminClaims = _userManager.AddClaimsAsync(admin, new Claim[]
            {
                new Claim(JwtClaimTypes.Name,$"{admin.FirstName} {admin.LastName}"),
                new Claim(JwtClaimTypes.GivenName,$"{admin.FirstName}"),
                new Claim(JwtClaimTypes.FamilyName,$"{admin.LastName}"),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.Admin),

            }).Result;

            ApplicationUser client = new ApplicationUser()
            {
                UserName = "andre-client",
                Email = "andre-client@email.com.br",
                EmailConfirmed = true,
                PhoneNumber = "+55 (11) 12345-6789",
                FirstName = "André",
                LastName = "Client"
            };

            _userManager.CreateAsync(client, "12345678@Andre").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(client, IdentityConfiguration.Admin).GetAwaiter().GetResult();

            var clientClaims = _userManager.AddClaimsAsync(client, new Claim[]
            {
                new Claim(JwtClaimTypes.Name,$"{client.FirstName} {client.LastName}"),
                new Claim(JwtClaimTypes.GivenName,$"{client.FirstName}"),
                new Claim(JwtClaimTypes.FamilyName,$"{client.LastName}"),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.Client),

            }).Result;
        }
    }
}
