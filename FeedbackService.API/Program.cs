using Carter;
using FastEndpoints;
using FeedbackService.API.Features.Clients;
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

//if(app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("RUN_MIGRATIONS") == "true")
//{
//    using var scope = app.Services.CreateScope();
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.Migrate();
//}


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

app.Run();
