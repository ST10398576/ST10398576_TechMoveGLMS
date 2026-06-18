using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Interfaces;
using ST10398576_TechMoveGLMS.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add JWT Authentication
// Prepare JWT signing key (support Base64-encoded keys or plain-text keys)
var jwtKeyConfig = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKeyConfig))
{
    throw new InvalidOperationException("Jwt:Key is not configured. Set Jwt:Key in appsettings or environment variables.");
}

byte[] jwtKeyBytes;
try
{
    jwtKeyBytes = Convert.FromBase64String(jwtKeyConfig);
}
catch (FormatException)
{
    jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKeyConfig);
}

if (jwtKeyBytes.Length < 32)
{
    throw new InvalidOperationException($"Configured JWT key is too short: {jwtKeyBytes.Length * 8} bits. Require at least 256 bits for HS256.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes)
        };
    });

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

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TechMove GLMS API",
        Version = "v1"
    });

    // Define the security scheme
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and then your JWT token.\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    // Apply the scheme globally
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
// Lightweight health endpoint used by Docker healthchecks. It attempts a DB connection.
app.MapGet("/health", async (IServiceProvider services) =>
{
    try
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechMoveDBContext>();
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect ? Results.Ok(new { status = "Healthy" }) : Results.StatusCode(503);
    }
    catch
    {
        return Results.StatusCode(503);
    }
});

app.MapControllers();
app.Run();


namespace ST10398576_TechMoveGLMS.API
{
    public partial class Program { }
}