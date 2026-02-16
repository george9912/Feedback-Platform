using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.DbContexts;
using NotificationService.Services;

var builder = FunctionsApplication.CreateBuilder(args);


builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Services.AddDbContext<FeedbackDbContext>(options =>
    options.UseSqlServer("Server=localhost,1414;Database=FeedbackDb;User Id=sa;Password=GeoParola2025!;TrustServerCertificate=True;"));

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer("Server=localhost,1414;Database=UserDb;User Id=sa;Password=GeoParola2025!;TrustServerCertificate=True;"));

builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();

builder.Build().Run();
