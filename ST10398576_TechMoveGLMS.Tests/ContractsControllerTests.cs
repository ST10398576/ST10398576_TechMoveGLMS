using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ST10398576_TechMoveGLMS.Controllers;
using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Models;
using ST10398576_TechMoveGLMS.Tests.Fakes; // <-- make sure FakeWebHostEnvironment is in its own file
using Xunit;
using System;
using System.Threading.Tasks;

public class ContractsControllerTests
{
    private TechMoveDBContext GetContext()
    {
        var options = new DbContextOptionsBuilder<TechMoveDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
            .Options;
        return new TechMoveDBContext(options);
    }

    [Fact]
    public async Task Create_AddsContract()
    {
        // Arrange
        var context = GetContext();
        var env = new FakeWebHostEnvironment();
        var controller = new ContractsController(context, env);

        // Always fill required fields
        var contract = new Contract
        {
            ClientId = 1, // don’t hardcode ContractId, let EF assign
            ContractStatus = "Active",
            ContractServiceLevel = "High",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(6)
        };

        IFormFile file = null; // no upload for this test

        // Act
        var result = await controller.Create(contract, file);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(1, await context.Contracts.CountAsync());
    }
}
