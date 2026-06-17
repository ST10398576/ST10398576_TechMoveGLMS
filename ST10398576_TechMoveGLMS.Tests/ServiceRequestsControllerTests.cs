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

public class ServiceRequestsControllerTests
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
            BaseAddress = new Uri("https://localhost:5001")
        };
    }

    [Fact]
    public async Task Index_ReturnsServiceRequests()
    {
        var mockClient = CreateMockHttpClient(new List<ServiceRequest>
        {
            new ServiceRequest { ServiceRequestId = 1, ServiceDescription = "Test", ServiceStatus = "Pending" }
        });

        var controller = new ServiceRequestsController(new HttpClientFactoryMock(mockClient));
        var result = await controller.Index(null, null, null) as ViewResult;

        var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequest>>(result.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Details_ReturnsServiceRequest()
    {
        var mockClient = CreateMockHttpClient(new ServiceRequest { ServiceRequestId = 1, ServiceDescription = "Test" });
        var controller = new ServiceRequestsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.Details(1) as ViewResult;
        var model = Assert.IsAssignableFrom<ServiceRequest>(result.Model);

        Assert.Equal("Test", model.ServiceDescription);
    }

    [Fact]
    public async Task Create_AddsServiceRequest()
    {
        var mockClient = CreateMockHttpClient(new ServiceRequest { ServiceRequestId = 1, ServiceStatus = "Pending" });
        var controller = new ServiceRequestsController(new HttpClientFactoryMock(mockClient));

        var request = new ServiceRequest { ContractId = 1, ServiceDescription = "Test", ServiceCost = 100, ServiceStatus = "Pending" };
        var result = await controller.Create(request);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Edit_UpdatesServiceRequest()
    {
        var mockClient = CreateMockHttpClient(new ServiceRequest { ServiceRequestId = 1, ServiceDescription = "Updated" });
        var controller = new ServiceRequestsController(new HttpClientFactoryMock(mockClient));

        var request = new ServiceRequest { ServiceRequestId = 1, ServiceDescription = "Updated" };
        var result = await controller.Edit(1, request);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsServiceRequestForConfirmation()
    {
        var mockClient = CreateMockHttpClient(new ServiceRequest { ServiceRequestId = 1, ServiceDescription = "ToDelete" });
        var controller = new ServiceRequestsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.Delete(1) as ViewResult;
        var model = Assert.IsAssignableFrom<ServiceRequest>(result.Model);

        Assert.Equal("ToDelete", model.ServiceDescription);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesServiceRequest()
    {
        var mockClient = CreateMockHttpClient(new { });
        var controller = new ServiceRequestsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.DeleteConfirmed(1);
        Assert.IsType<RedirectToActionResult>(result);
    }
}
