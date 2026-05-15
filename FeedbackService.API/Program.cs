using Carter;
using FastEndpoints;
using FeedbackService.API.Common.Notifications;
using FeedbackService.API.Features.Clients;
using FeedbackService.API.Features.Feedback.Campaign;
using FeedbackService.API.Features.Feedback.Create;
using FeedbackService.API.Features.Feedback.Delete;
using FeedbackService.API.Features.Feedback.GetById;
using FeedbackService.API.Features.Feedback.ListByUser;
using FeedbackService.API.Features.Feedback.Update;
using FeedbackService.API.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<AppDbContext>(opt =>
//    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

var serviceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"];
var serviceBusEnabled = builder.Configuration.GetValue<bool?>("ServiceBus:Enabled")
    ?? !string.IsNullOrWhiteSpace(serviceBusConnectionString);

if (serviceBusEnabled && !string.IsNullOrWhiteSpace(serviceBusConnectionString))
{
    builder.Services.AddSingleton(_ => new Azure.Messaging.ServiceBus.ServiceBusClient(serviceBusConnectionString));
    builder.Services.AddSingleton<IFeedbackEventPublisher, ServiceBusFeedbackEventPublisher>();
    builder.Services.AddSingleton<ICampaignNotificationDispatcher, CampaignNotificationDispatcher>();
}
else
{
    builder.Services.AddSingleton<IFeedbackEventPublisher, NoOpFeedbackEventPublisher>();
    builder.Services.AddSingleton<ICampaignNotificationDispatcher, NoOpCampaignNotificationDispatcher>();
}

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Handlers
//builder.Services.AddScoped<FeedbackService.API.Features.Feedback.Create.Handler>();
builder.Services.AddScoped<FeedbackService.API.Features.Feedback.GetById.Handler>();
builder.Services.AddScoped<FeedbackService.API.Features.Feedback.ListByUser.Handler>();
builder.Services.AddScoped<FeedbackService.API.Features.Feedback.Delete.Handler>();
builder.Services.AddScoped<FeedbackService.API.Features.Feedback.Update.Handler>();

builder.Services.AddScoped<IUserClient, UserClient>();

// FastEndpoints setup
builder.Services.AddFastEndpoints();
// Carter setup
builder.Services.AddCarter();

// Add HttpClient to communicate Feedback-User
builder.Services.AddHttpClient<IUserClient, UserClient>(client =>
{
    client.BaseAddress = new Uri("http://userservice.api:8080"); //research Resilience
});

var app = builder.Build();

// Configure the HTTP request pipeline.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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

app.UseAuthorization();

app.UseFastEndpoints();   // FastEndpoints pipeline
app.MapCarter();          // Carter modules

app.MapControllers();

//app.MapCreateFeedback();
app.MapGetFeedbackById();
app.MapGetFeedbacksByUser();
app.MapUpdateFeedback();
app.MapDeleteFeedback();
app.MapCampaignRoutes();

app.Run();
