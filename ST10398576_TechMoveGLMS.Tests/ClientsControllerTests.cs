using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using ST10398576_TechMoveGLMS.Controllers;
using ST10398576_TechMoveGLMS.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

public class ClientsControllerTests
{
    private HttpClient CreateMockHttpClient(object responseObject)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseObject), Encoding.UTF8, "application/json")
            });

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5002")
        };
    }

    [Fact]
    public async Task Index_ReturnsClients()
    {
        var mockClient = CreateMockHttpClient(new List<Client>
        {
            new Client { ClientId = 1, ClientName = "Test", ClientContactDetails = "123", ClientRegion = "Cape Town" }
        });

        var controller = new ClientsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.Index() as ViewResult;
        var model = Assert.IsAssignableFrom<IEnumerable<Client>>(result.Model);

        Assert.Single(model);
    }

    [Fact]
    public async Task Details_ReturnsClient()
    {
        var mockClient = CreateMockHttpClient(new Client { ClientId = 1, ClientName = "Test" });
        var controller = new ClientsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.Details(1) as ViewResult;
        var model = Assert.IsAssignableFrom<Client>(result.Model);

        Assert.Equal("Test", model.ClientName);
    }

    [Fact]
    public async Task Edit_UpdatesClient()
    {
        var mockClient = CreateMockHttpClient(new Client { ClientId = 1, ClientName = "Updated" });
        var controller = new ClientsController(new HttpClientFactoryMock(mockClient));

        var client = new Client { ClientId = 1, ClientName = "Updated" };
        var result = await controller.Edit(1, client);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsClientForConfirmation()
    {
        var mockClient = CreateMockHttpClient(new Client { ClientId = 1, ClientName = "ToDelete" });
        var controller = new ClientsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.Delete(1) as ViewResult;
        var model = Assert.IsAssignableFrom<Client>(result.Model);

        Assert.Equal("ToDelete", model.ClientName);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesClient()
    {
        var mockClient = CreateMockHttpClient(new { });
        var controller = new ClientsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.DeleteConfirmed(1);
        Assert.IsType<RedirectToActionResult>(result);
    }
}

// Helper to simulate IHttpClientFactory
public class HttpClientFactoryMock : IHttpClientFactory
{
    private readonly HttpClient _client;
    public HttpClientFactoryMock(HttpClient client) => _client = client;
    public HttpClient CreateClient(string name = "") => _client;
}
