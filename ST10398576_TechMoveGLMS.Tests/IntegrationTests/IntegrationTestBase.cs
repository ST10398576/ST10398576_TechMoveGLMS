using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ST10398576_TechMoveGLMS.API;
using ST10398576_TechMoveGLMS.DBContext;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<ST10398576_TechMoveGLMS.API.Program>>
{
    protected readonly HttpClient _client;

    public IntegrationTestBase(WebApplicationFactory<ST10398576_TechMoveGLMS.API.Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Testing");
            builder.ConfigureServices(services =>
            {
                    // Replace DBContext registrations with InMemory for testing
                    var descriptors = services.Where(d =>
                        d.ServiceType == typeof(DbContextOptions<TechMoveDBContext>) ||
                        d.ServiceType == typeof(TechMoveDBContext) ||
                        (d.ImplementationType != null && d.ImplementationType == typeof(TechMoveDBContext))
                    ).ToList();
                    foreach (var d in descriptors) services.Remove(d);

                    services.AddDbContext<TechMoveDBContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
            });
        }).CreateClient();
    }
}
