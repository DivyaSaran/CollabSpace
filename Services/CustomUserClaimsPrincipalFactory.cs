using System.Security.Claims;
using CollabSpace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CollabSpace.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, IdentityRole>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Ensuring Standard claims + custom DisplayName are present in the cookie
            var displayName = !string.IsNullOrWhiteSpace(user.Name) ? user.Name : user.UserName ?? user.Email ?? "User";
            identity.AddClaim(new Claim("DisplayName", displayName));

            return identity;
        }
    }
}