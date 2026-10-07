using CollabSpace.Components;
using CollabSpace.Data;
using CollabSpace.Models;
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

// 5. Register ASP.NET Core Identity with production security rules
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

// 6. Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "CollabSpace.Auth";
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/not-found";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// 7. Blazor authentication state cascaded to all components
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
