using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.Controllers;
using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Models;
using Xunit;
using System.Threading.Tasks;

public class ServiceRequestsControllerTests
{
    private TechMoveDBContext GetContext()
    {
        var options = new DbContextOptionsBuilder<TechMoveDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
            .Options;
        return new TechMoveDBContext(options);
    }

    [Fact]
    public async Task Create_AddsServiceRequest_WhenContractIsActive()
    {
        // Arrange
        var context = GetContext();

        // Seed an active contract with required fields
        var contract = new Contract
        {
            ClientId = 1,
            ContractStatus = "Active",
            ContractServiceLevel = "High",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(6)
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var controller = new ServiceRequestsController(context);

        // ServiceRequest with required fields
        var request = new ServiceRequest
        {
            ContractId = contract.ContractId, // use EF‑assigned ID
            ServiceDescription = "Test Service",
            ServiceCost = 100,
            ServiceStatus = "Pending"
        };

        // Act
        var result = await controller.Create(request);

        // Assert → should redirect on success
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(1, await context.ServiceRequests.CountAsync());
    }

    [Fact]
    public async Task Create_Fails_WhenContractIsExpired()
    {
        // Arrange
        var context = GetContext();

        // Seed an expired contract with all required fields
        var contract = new Contract
        {
            ClientId = 1,
            ContractStatus = "Expired",
            ContractServiceLevel = "Low",
            StartDate = DateTime.Now.AddMonths(-12),
            EndDate = DateTime.Now.AddMonths(-6)
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var controller = new ServiceRequestsController(context);

        // ServiceRequest with required fields
        var request = new ServiceRequest
        {
            ContractId = contract.ContractId, // use EF‑assigned ID
            ServiceDescription = "Test Service",
            ServiceCost = 100,
            ServiceStatus = "Pending"
        };

        // Act
        var result = await controller.Create(request);

        // Assert → should return ViewResult (validation error)
        Assert.IsType<ViewResult>(result);
        Assert.Equal(0, await context.ServiceRequests.CountAsync());
    }

    [Fact]
    public async Task Edit_UpdatesServiceRequest()
    {
        // Arrange
        var context = GetContext();

        // Seed an active contract (required for ServiceRequest)
        var contract = new Contract
        {
            ClientId = 1,
            ContractStatus = "Active",
            ContractServiceLevel = "High",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(6)
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        // Seed a service request with required fields
        var request = new ServiceRequest
        {
            ContractId = contract.ContractId,
            ServiceDescription = "Old Description",
            ServiceCost = 200,
            ServiceStatus = "Pending"
        };
        context.ServiceRequests.Add(request);
        await context.SaveChangesAsync();

        var controller = new ServiceRequestsController(context);

        // Update description
        request.ServiceDescription = "Updated Description";

        // Act
        var result = await controller.Edit(request.ServiceRequestId, request);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var updated = await context.ServiceRequests.FindAsync(request.ServiceRequestId);
        Assert.Equal("Updated Description", updated.ServiceDescription);
    }

    [Fact]
    public async Task Delete_RemovesServiceRequest()
    {
        // Arrange
        var context = GetContext();

        // Seed an active contract (required for ServiceRequest)
        var contract = new Contract
        {
            ClientId = 1,
            ContractStatus = "Active",
            ContractServiceLevel = "High",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(6)
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        // Seed a service request with required fields
        var request = new ServiceRequest
        {
            ContractId = contract.ContractId,
            ServiceDescription = "DeleteMe",
            ServiceCost = 50,
            ServiceStatus = "Completed"
        };
        context.ServiceRequests.Add(request);
        await context.SaveChangesAsync();

        var controller = new ServiceRequestsController(context);

        // Act
        var result = await controller.DeleteConfirmed(request.ServiceRequestId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(0, await context.ServiceRequests.CountAsync());
    }
}
