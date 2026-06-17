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

public class ContractsControllerTests
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
            BaseAddress = new Uri("https://localhost:7066")
        };
    }

    [Fact]
    public async Task Index_ReturnsContracts()
    {
        var mockClient = CreateMockHttpClient(new List<Contract>
        {
            new Contract { ContractId = 1, ContractStatus = "Active", ContractServiceLevel = "High" }
        });

        var controller = new ContractsController(new HttpClientFactoryMock(mockClient));
        var result = await controller.Index(null, null, null, null) as ViewResult;

        var model = Assert.IsAssignableFrom<IEnumerable<Contract>>(result.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Details_ReturnsContract()
    {
        var mockClient = CreateMockHttpClient(new Contract { ContractId = 1, ContractStatus = "Active" });
        var controller = new ContractsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.Details(1) as ViewResult;
        var model = Assert.IsAssignableFrom<Contract>(result.Model);

        Assert.Equal("Active", model.ContractStatus);
    }

    [Fact]
    public async Task Create_AddsContract()
    {
        var mockClient = CreateMockHttpClient(new Contract { ContractId = 1, ContractStatus = "Active" });
        var controller = new ContractsController(new HttpClientFactoryMock(mockClient));

        var contract = new Contract { ClientId = 1, ContractStatus = "Active", ContractServiceLevel = "High" };
        var result = await controller.Create(contract);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Edit_UpdatesContract()
    {
        var mockClient = CreateMockHttpClient(new Contract { ContractId = 1, ContractStatus = "Updated" });
        var controller = new ContractsController(new HttpClientFactoryMock(mockClient));

        var contract = new Contract { ContractId = 1, ContractStatus = "Updated" };
        var result = await controller.Edit(1, contract);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsContractForConfirmation()
    {
        var mockClient = CreateMockHttpClient(new Contract { ContractId = 1, ContractStatus = "ToDelete" });
        var controller = new ContractsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.Delete(1) as ViewResult;
        var model = Assert.IsAssignableFrom<Contract>(result.Model);

        Assert.Equal("ToDelete", model.ContractStatus);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesContract()
    {
        var mockClient = CreateMockHttpClient(new { });
        var controller = new ContractsController(new HttpClientFactoryMock(mockClient));

        var result = await controller.DeleteConfirmed(1);
        Assert.IsType<RedirectToActionResult>(result);
    }
}
