using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebsiteBuilderForBusinesses.DataAccess.Postgres;

namespace WebsiteBuilderForBusinesses.API;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly string _testConnectionString =
        "Host=localhost;Port=5433;Database=websitebuilder_tests;Username=postgres;Password=postgres";
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<WebBuilderDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            services.AddDbContext<WebBuilderDbContext>(options =>
                options.UseNpgsql(_testConnectionString));
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        });
        var host = base.CreateHost(builder);
        return host;
    }
}