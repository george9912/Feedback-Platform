using Azure.Communication.Email;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Clients;
using NotificationService.Interfaces;
using NotificationService.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton(_ =>
{
    var cs = builder.Configuration["AcsEmail:ConnectionString"]
             ?? throw new Exception("Missing AcsEmail:ConnectionString");
    return new EmailClient(cs);
});

builder.Services.AddHttpClient("UserService", client =>
{
    var baseUrl = builder.Configuration["UserService:BaseUrl"]
                 ?? throw new Exception("Missing UserService:BaseUrl");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddSingleton<IUserClient, UserClient>();
builder.Services.AddSingleton<IEmailSender, AcsEmailSender>();

builder.Build().Run();