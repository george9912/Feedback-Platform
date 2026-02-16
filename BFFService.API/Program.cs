using BFFService.API.Clients.Feddback;
using BFFService.API.Clients.User;

var builder = WebApplication.CreateBuilder(args);

// Configur?m clien?ii HTTP   + REZILIENTA 
builder.Services.AddHttpClient<IUserClient, UserClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:UserService"]);
});

builder.Services.AddHttpClient<IFeedbackClient, FeedbackClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:FeedbackService"]);
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
