using Azure.Identity;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Serilog;
using System;
using UserService.Application.Interfaces;
using UserService.Application.Mappings;
using UserService.Application.Services;
using UserService.Application.Validators;
using UserService.Infrastructure;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/user-log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// EF Core
//builder.Services.AddDbContext<UserDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<UserDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Graph client
builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var tenantId = configuration["AzureAd:TenantId"];
    var clientId = configuration["AzureAd:ClientId"];
    var clientSecret = configuration["AzureAd:ClientSecret"];

    if (string.IsNullOrWhiteSpace(tenantId) ||
        string.IsNullOrWhiteSpace(clientId) ||
        string.IsNullOrWhiteSpace(clientSecret))
    {
        throw new InvalidOperationException("AzureAd Graph configuration is missing.");
    }

    var credential = new ClientSecretCredential(
        tenantId,
        clientId,
        clientSecret);

    return new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
});

// Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService.Infrastructure.Services.UserService>();
builder.Services.AddScoped<IGraphUserSyncService, GraphUserSyncService>();

builder.Services.AddAutoMapper(typeof(UserProfile));

// Add services to the container.

builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Pt React
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactLocal", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Pentru a nu mai rula mereu manual migrarile pentru db de containers
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    var retries = 10;
    for (int i = 1; i <= retries; i++)
    {
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("Migration applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration attempt {i} failed: {ex.Message}");

            if (i == retries)
                throw;

            Thread.Sleep(5000);
        }
    }
}

app.UseSwagger();
    app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.UseHttpsRedirection();

app.UseCors("ReactLocal");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
