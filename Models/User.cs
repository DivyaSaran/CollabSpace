using Microsoft.AspNetCore.Identity;


namespace CollabSpace.Models
{
    // Inheriting from IdentityUser gives us:
    // - Id (string GUID)
    // - UserName (string)
    // - Email (string)
    // - PasswordHash (secure hashed password)
    // - EmailConfirmed, LockoutEnd, TwoFactorEnabled, etc.
    public class User : IdentityUser
    {
        // Custom field for the user's full display name alone
        public string Name { get; set; } = string.Empty;
    }
}


/** Commenting my old code after switching to IdentityUser 
 * for better security and built-in features.
**/


/*    public class User : IdentityUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; } 
        public string Role { get; set; } = "Member";

    }*/

