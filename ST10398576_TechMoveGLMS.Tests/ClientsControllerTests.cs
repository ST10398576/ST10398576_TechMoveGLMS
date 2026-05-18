using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.Controllers;
using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Models;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ClientsControllerTests
{
    private TechMoveDBContext GetContext()
    {
        var options = new DbContextOptionsBuilder<TechMoveDBContext>()
            .UseInMemoryDatabase(databaseName: "ClientsTestDB")
            .Options;
        return new TechMoveDBContext(options);
    }

    [Fact]
    public async Task Index_ReturnsClients()
    {
        var context = GetContext();
        context.Clients.Add(new Client { ClientName = "Test", ClientContactDetails = "123", ClientRegion = "Cape Town" });
        await context.SaveChangesAsync();

        var controller = new ClientsController(context);

        var result = await controller.Index() as ViewResult;
        var model = Assert.IsAssignableFrom<IEnumerable<Client>>(result.Model);

        Assert.Single(model);
    }

    [Fact]
    public async Task Create_AddsClient()
    {
        var context = new TechMoveDBContext(
            new DbContextOptionsBuilder<TechMoveDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var controller = new ClientsController(context);

        var client = new Client { ClientName = "New", ClientContactDetails = "456", ClientRegion = "Johannesburg" };
        var result = await controller.Create(client);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(await context.Clients.ToListAsync()); // expect exactly 1
    }

    [Fact]
    public async Task Edit_UpdatesClient()
    {
        var context = GetContext();
        var client = new Client { ClientName = "Old", ClientContactDetails = "789", ClientRegion = "Durban" };
        context.Clients.Add(client);
        await context.SaveChangesAsync();

        var controller = new ClientsController(context);
        client.ClientName = "Updated";

        var result = await controller.Edit(client.ClientId, client);

        Assert.IsType<RedirectToActionResult>(result);
        var updated = await context.Clients.FindAsync(client.ClientId);
        Assert.Equal("Updated", updated.ClientName);
    }

    [Fact]
    public async Task Delete_RemovesClient()
    {
        var context = new TechMoveDBContext(
            new DbContextOptionsBuilder<TechMoveDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var client = new Client { ClientName = "DeleteMe", ClientContactDetails = "000", ClientRegion = "Pretoria" };
        context.Clients.Add(client);
        await context.SaveChangesAsync();

        var controller = new ClientsController(context);
        var result = await controller.DeleteConfirmed(client.ClientId);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(await context.Clients.ToListAsync()); // expect 0
    }
}
