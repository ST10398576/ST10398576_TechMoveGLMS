using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ST10398576_TechMoveGLMS.API; // backend API namespace
using ST10398576_TechMoveGLMS.Models;
using Xunit;

public class ClientsApiIntegrationTests : IntegrationTestBase
{
    public ClientsApiIntegrationTests(WebApplicationFactory<ST10398576_TechMoveGLMS.API.Program> factory) : base(factory) { }

    [Fact]
    public async Task GetClients_ReturnsList()
    {
        var response = await _client.GetAsync("/api/clients");
        response.EnsureSuccessStatusCode();
        var clients = await response.Content.ReadFromJsonAsync<List<Client>>();
        Assert.NotNull(clients);
    }

    [Fact]
    public async Task CreateClient_AddsClient()
    {
        var newClient = new Client { ClientName = "Test Client", ClientContactDetails = "12345", ClientRegion = "Cape Town" };
        var response = await _client.PostAsJsonAsync("/api/clients", newClient);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_ChangesDetails()
    {
        var newClient = new Client { ClientName = "UpdateMe", ClientContactDetails = "000", ClientRegion = "CT" };
        var createResponse = await _client.PostAsJsonAsync("/api/clients", newClient);
        var created = await createResponse.Content.ReadFromJsonAsync<Client>();

        created.ClientName = "Updated Name";
        var updateResponse = await _client.PutAsJsonAsync($"/api/clients/{created.ClientId}", created);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_RemovesClient()
    {
        var newClient = new Client { ClientName = "DeleteMe", ClientContactDetails = "111", ClientRegion = "CT" };
        var createResponse = await _client.PostAsJsonAsync("/api/clients", newClient);
        var created = await createResponse.Content.ReadFromJsonAsync<Client>();

        var deleteResponse = await _client.DeleteAsync($"/api/clients/{created.ClientId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
