using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
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
                    // Add a test authentication scheme that automatically authenticates requests for integration tests
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

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
