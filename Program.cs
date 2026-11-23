using CollabSpace.Components;
using CollabSpace.Data;
using CollabSpace.Models;
using CollabSpace.Repositories;
using CollabSpace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// 1. Blazor Server Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Controllers for API endpoints
builder.Services.AddControllers();

// 3. HttpClient for Blazor components making internal API calls
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["AppBaseUrl"] ?? "https://localhost:7121/")
});

// 4. Database Context (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Middle-layer repository registrations
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddHttpClient<IGitRepositoryService, GitRepositoryService>();

// Register AutoMapper (version 16+ the built-in AddAutoMapper extension method
// on IServiceCollection expects an action configuration callback rather than
// typeof(...) directly) and scan the assembly containing MappingProfile
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<CollabSpace.Mappings.MappingProfile>();
});

//  Register ASP.NET Core Identity with production security rules
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password strength settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    // Email uniqueness constraint
    options.User.RequireUniqueEmail = true;

    // Brute-force account lockout protection
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//  Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "CollabSpace.AuthCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(3); // Cookie expires after 60 mins (temporarily 3 mins)
    options.SlidingExpiration = true;                  // Resets timer if user is active
    options.LoginPath = "/login";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/access-denied";
});

// Register Custom Claims Factory (adds DisplayName, UserId, Email, Username to the cookie)
builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, CustomUserClaimsPrincipalFactory>();

// Optional: Define Claims-Based Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ValidCollabUser", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim(System.Security.Claims.ClaimTypes.Email)
              .RequireClaim("DisplayName"));
});

//  Blazor authentication state cascaded to all components
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// 8. Authentication and Authorization Middleware (Order is critical!)
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


/** Commenting my old code after switching to IdentityDbContext<User> and IdentityUser 


namespace CollabSpace
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container- Enables Blazor Server
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddControllers(); // Enables Web API endpoints
            builder.Services.AddScoped(serv => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7121/")
            });
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();

            app.MapControllers(); // Registers API routes
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            
                 

            app.Run();
        }
    }
}

**/
