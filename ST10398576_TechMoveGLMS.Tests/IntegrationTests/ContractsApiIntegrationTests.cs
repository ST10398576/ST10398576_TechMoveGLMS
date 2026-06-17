using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ST10398576_TechMoveGLMS.Models;
using Xunit;

public class ContractsApiIntegrationTests : IntegrationTestBase
{
    public ContractsApiIntegrationTests(WebApplicationFactory<ST10398576_TechMoveGLMS.API.Program> factory) : base(factory) { }

    [Fact]
    public async Task GetContracts_ReturnsList()
    {
        var response = await _client.GetAsync("/api/contracts");
        response.EnsureSuccessStatusCode();
        var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
        Assert.NotNull(contracts);
    }

    [Fact]
    public async Task CreateContract_AddsContract()
    {
        // create a client first so the contract FK is valid
        var client = new ST10398576_TechMoveGLMS.Models.Client { ClientName = "TC", ClientContactDetails = "x", ClientRegion = "R" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", client);
        clientResp.EnsureSuccessStatusCode();
        var createdClient = await clientResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Client>();

        var newContract = new Contract
        {
            ContractStatus = "Active",
            ContractServiceLevel = "High",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(6),
            ClientId = createdClient!.ClientId
        };
        var response = await _client.PostAsJsonAsync("/api/contracts", newContract);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContract_ChangesDetails()
    {
        // create a client and contract first
        var client = new ST10398576_TechMoveGLMS.Models.Client { ClientName = "TC2", ClientContactDetails = "x", ClientRegion = "R" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", client);
        clientResp.EnsureSuccessStatusCode();
        var createdClient = await clientResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Client>();

        var newContract = new Contract
        {
            ContractStatus = "Active",
            ContractServiceLevel = "Low",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(3),
            ClientId = createdClient!.ClientId
        };
        var createResponse = await _client.PostAsJsonAsync("/api/contracts", newContract);
        var created = await createResponse.Content.ReadFromJsonAsync<Contract>();

        created!.ContractServiceLevel = "High";
        var updateResponse = await _client.PutAsJsonAsync($"/api/contracts/{created.ContractId}", created);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ChangesContractStatus()
    {
        // create client and contract
        var client = new ST10398576_TechMoveGLMS.Models.Client { ClientName = "TC3", ClientContactDetails = "x", ClientRegion = "R" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", client);
        clientResp.EnsureSuccessStatusCode();
        var createdClient = await clientResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Client>();

        var newContract = new Contract
        {
            ContractStatus = "Active",
            ContractServiceLevel = "Medium",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(6),
            ClientId = createdClient!.ClientId
        };
        var createResponse = await _client.PostAsJsonAsync("/api/contracts", newContract);
        var created = await createResponse.Content.ReadFromJsonAsync<Contract>();

        var patchResponse = await _client.PatchAsync($"/api/contracts/{created!.ContractId}/status",
            JsonContent.Create("Expired"));
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteContract_RemovesContract()
    {
        var client = new ST10398576_TechMoveGLMS.Models.Client { ClientName = "TC4", ClientContactDetails = "x", ClientRegion = "R" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", client);
        clientResp.EnsureSuccessStatusCode();
        var createdClient = await clientResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Client>();

        var newContract = new Contract
        {
            ContractStatus = "Active",
            ContractServiceLevel = "Low",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(6),
            ClientId = createdClient!.ClientId
        };
        var createResponse = await _client.PostAsJsonAsync("/api/contracts", newContract);
        var created = await createResponse.Content.ReadFromJsonAsync<Contract>();

        var deleteResponse = await _client.DeleteAsync($"/api/contracts/{created!.ContractId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
