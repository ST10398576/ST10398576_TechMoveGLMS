using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ST10398576_TechMoveGLMS.API;
using ST10398576_TechMoveGLMS.Models;
using Xunit;

public class ServiceRequestsApiIntegrationTests : IntegrationTestBase
{
    public ServiceRequestsApiIntegrationTests(WebApplicationFactory<ST10398576_TechMoveGLMS.API.Program> factory) : base(factory) { }

    [Fact]
    public async Task GetServiceRequests_ReturnsList()
    {
        var response = await _client.GetAsync("/api/servicerequests");
        response.EnsureSuccessStatusCode();
        var requests = await response.Content.ReadFromJsonAsync<List<ServiceRequest>>();
        Assert.NotNull(requests);
    }

    [Fact]
    public async Task CreateServiceRequest_AddsRequest()
    {
        // create prerequisite client and contract
        var client = new ST10398576_TechMoveGLMS.Models.Client { ClientName = "SRClient", ClientContactDetails = "x", ClientRegion = "R" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", client);
        clientResp.EnsureSuccessStatusCode();
        var createdClient = await clientResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Client>();

        var contract = new ST10398576_TechMoveGLMS.Models.Contract { ContractStatus = "Active", ContractServiceLevel = "High", StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(6), ClientId = createdClient!.ClientId };
        var contractResp = await _client.PostAsJsonAsync("/api/contracts", contract);
        contractResp.EnsureSuccessStatusCode();
        var createdContract = await contractResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Contract>();

        var newRequest = new ServiceRequest
        {
            ServiceDescription = "Test Delivery",
            ServiceCost = 500,
            ServiceStatus = "Pending",
            ContractId = createdContract!.ContractId
        };
        var response = await _client.PostAsJsonAsync("/api/servicerequests", newRequest);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateServiceRequest_ChangesDetails()
    {
        // create prereqs
        var client = new ST10398576_TechMoveGLMS.Models.Client { ClientName = "SRClient2", ClientContactDetails = "x", ClientRegion = "R" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", client);
        clientResp.EnsureSuccessStatusCode();
        var createdClient = await clientResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Client>();

        var contract = new ST10398576_TechMoveGLMS.Models.Contract { ContractStatus = "Active", ContractServiceLevel = "High", StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(6), ClientId = createdClient!.ClientId };
        var contractResp = await _client.PostAsJsonAsync("/api/contracts", contract);
        contractResp.EnsureSuccessStatusCode();
        var createdContract = await contractResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Contract>();

        var newRequest = new ServiceRequest
        {
            ServiceDescription = "Initial",
            ServiceCost = 100,
            ServiceStatus = "Pending",
            ContractId = createdContract!.ContractId
        };
        var createResponse = await _client.PostAsJsonAsync("/api/servicerequests", newRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceRequest>();

        created!.ServiceStatus = "Completed";
        var updateResponse = await _client.PutAsJsonAsync($"/api/servicerequests/{created.ServiceRequestId}", created);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteServiceRequest_RemovesRequest()
    {
        // create prereqs
        var client = new ST10398576_TechMoveGLMS.Models.Client { ClientName = "SRClient3", ClientContactDetails = "x", ClientRegion = "R" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", client);
        clientResp.EnsureSuccessStatusCode();
        var createdClient = await clientResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Client>();

        var contract = new ST10398576_TechMoveGLMS.Models.Contract { ContractStatus = "Active", ContractServiceLevel = "High", StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(6), ClientId = createdClient!.ClientId };
        var contractResp = await _client.PostAsJsonAsync("/api/contracts", contract);
        contractResp.EnsureSuccessStatusCode();
        var createdContract = await contractResp.Content.ReadFromJsonAsync<ST10398576_TechMoveGLMS.Models.Contract>();

        var newRequest = new ServiceRequest
        {
            ServiceDescription = "DeleteMe",
            ServiceCost = 200,
            ServiceStatus = "Pending",
            ContractId = createdContract!.ContractId
        };
        var createResponse = await _client.PostAsJsonAsync("/api/servicerequests", newRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceRequest>();

        var deleteResponse = await _client.DeleteAsync($"/api/servicerequests/{created!.ServiceRequestId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
