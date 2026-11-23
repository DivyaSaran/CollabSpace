using CollabSpace.DTOs;
using CollabSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // POST: /api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                Name = request.Name
            };

            // 1. Create the user with Identity password hashing and policies
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }

            // 2. Ensure default roles exist ("Admin" and "Member")
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await _roleManager.RoleExistsAsync("Member"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Member"));
            }

            // 3. Assign role (default to "Member" if not specified)
            var roleToAssign = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "Member";

            await _userManager.AddToRoleAsync(user, roleToAssign);

            return Ok(new { message = $"User registered successfully as {roleToAssign}." });
        }

        // POST: /api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Validates password, checks lockout counter, and issues the auth cookie
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                request.Password,
                isPersistent: request.RememberMe,
                lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked, new { message = "Account is locked due to too many failed attempts. Try again in 10 minutes." });
            }

            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Name,
                user.Email,
                Role = roles.FirstOrDefault() ?? "Member"
            });
        }

        // POST: /api/Auth/form-login
        [HttpPost("form-login")]
        public async Task<IActionResult> FormLogin([FromForm] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Redirect("/login?error=Invalid%20credentials.");
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                request.Password,
                isPersistent: request.RememberMe,
                lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return Redirect("/login?error=Account%20locked.%20Try%20again%20later.");
            }

            if (!result.Succeeded)
            {
                return Redirect("/login?error=Invalid%20credentials.");
            }

            // Redirect to workspaces page with the cookie attached to browser headers!
            return Redirect("/workspaces");
        }

        // POST: /api/Auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
            {
                // Security best practice: do not reveal whether the email exists
                return Ok(new { Message = "If an account with that email exists, a password reset link has been generated." });
            }

            // Generate cryptographic Identity reset token and Base64Url-encode it for safe URL transport
            var rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            // Build the reset URL pointing to your Blazor page
            var resetUrl = $"{Request.Scheme}://{Request.Host}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

            // In production, send `resetUrl` via IEmailSender. For local dev/testing, return it in the response:
            return Ok(new
            {
                Message = "Password reset link generated.",
                ResetLink = resetUrl
            });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
            {
                return BadRequest(new { Message = "Invalid password reset request." });
            }

            try
            {
                var decodedBytes = WebEncoders.Base64UrlDecode(model.Token);
                var rawToken = Encoding.UTF8.GetString(decodedBytes);

                var result = await _userManager.ResetPasswordAsync(user, rawToken, model.NewPassword);
                if (result.Succeeded)
                {
                    // Optional: Clear any existing lockout count after a successful password reset
                    await _userManager.ResetAccessFailedCountAsync(user);
                    return Ok(new { Message = "Your password has been reset successfully. You can now sign in." });
                }

                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(new { Message = errors });
            }
            catch
            {
                return BadRequest(new { Message = "Invalid or expired reset token." });
            }
        }

    }




    // Data Transfer Objects (DTOs)
    public class RegisterRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Member";
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}