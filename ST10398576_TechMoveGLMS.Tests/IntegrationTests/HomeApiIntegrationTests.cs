using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ST10398576_TechMoveGLMS.API; // backend API namespace
using ST10398576_TechMoveGLMS.Models;
using Xunit;

public class HomeApiIntegrationTests : IntegrationTestBase
{
    public HomeApiIntegrationTests(WebApplicationFactory<ST10398576_TechMoveGLMS.API.Program> factory) : base(factory) { }

    [Fact]
    public async Task GetDashboardSummary_ReturnsCounts()
    {
        var response = await _client.GetAsync("/api/home/dashboard");
        response.EnsureSuccessStatusCode();

        var summary = await response.Content.ReadFromJsonAsync<DashboardSummary>();
        Assert.NotNull(summary);

        // Basic sanity checks
        Assert.True(summary.TotalClients >= 0);
        Assert.True(summary.TotalContracts >= 0);
        Assert.True(summary.TotalServiceRequests >= 0);
    }
}
