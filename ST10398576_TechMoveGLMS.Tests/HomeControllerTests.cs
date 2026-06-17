using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using ST10398576_TechMoveGLMS.Controllers;
using ST10398576_TechMoveGLMS.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class HomeControllerTests
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
    public async Task Index_ReturnsDashboardSummary()
    {
        // Arrange: mock API response
        var mockSummary = new DashboardSummary
        {
            TotalClients = 5,
            TotalContracts = 3,
            ActiveContracts = 2,
            ExpiredContracts = 1,
            TotalServiceRequests = 10,
            PendingRequests = 4,
            CompletedRequests = 6
        };

        var mockClient = CreateMockHttpClient(mockSummary);
        var controller = new HomeController(new HttpClientFactoryMock(mockClient));

        // Act
        var result = await controller.Index() as ViewResult;

        // Assert
        var model = Assert.IsAssignableFrom<DashboardSummary>(result.Model);
        Assert.Equal(5, model.TotalClients);
        Assert.Equal(3, model.TotalContracts);
        Assert.Equal(10, model.TotalServiceRequests);
    }
}

