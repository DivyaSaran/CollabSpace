using CollabSpace.Components;
using Microsoft.EntityFrameworkCore;
using CollabSpace.Data;

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
