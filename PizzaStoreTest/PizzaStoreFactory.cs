using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PizzaStoreApi.Database;

namespace PizzaStoreTest;

public class PizzaStoreFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace the application's database registration with a fresh test database.
            services.RemoveAll<PizzaStoreDbContext>();
            services.RemoveAll<DbContextOptions<PizzaStoreDbContext>>();

            services.AddDbContext<PizzaStoreDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
