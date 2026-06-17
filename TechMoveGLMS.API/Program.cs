using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Interfaces;
using ST10398576_TechMoveGLMS.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// In integration tests we set a "Testing" environment and the test project will
// replace the DbContext with an in-memory provider. Skip registering SQL Server
// when running in that environment to avoid multiple provider conflicts.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<TechMoveDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add services to the container.
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Prevent JSON serialization errors when including navigation properties with cycles
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();


namespace ST10398576_TechMoveGLMS.API
{
    public partial class Program { }
}