using System.Text;
using System.Text.Json;
using FeedbackService.API.Common.Events;
using RabbitMQ.Client;

namespace FeedbackService.API.Common.Notifications;

public sealed class RabbitMqFeedbackEventPublisher : IFeedbackEventPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqFeedbackEventPublisher> _logger;

    public RabbitMqFeedbackEventPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqFeedbackEventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task PublishFeedbackCreatedAsync(FeedbackCreatedEvent evt, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var host = _configuration["RabbitMq:Host"] ?? "rabbitmq";
        var userName = _configuration["RabbitMq:Username"] ?? "guest";
        var password = _configuration["RabbitMq:Password"] ?? "guest";
        var virtualHost = _configuration["RabbitMq:VirtualHost"] ?? "/";
        var exchange = _configuration["RabbitMq:FeedbackCreated:Exchange"] ?? "feedback.events";
        var routingKey = _configuration["RabbitMq:FeedbackCreated:RoutingKey"] ?? "feedback.created";

        var portValue = _configuration["RabbitMq:Port"];
        var port = 5672;
        if (!string.IsNullOrWhiteSpace(portValue) && int.TryParse(portValue, out var parsedPort))
        {
            port = parsedPort;
        }

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = userName,
            Password = password,
            VirtualHost = virtualHost,
            DispatchConsumersAsync = true
        };

        try
        {
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Direct, durable: true, autoDelete: false);

            var payload = JsonSerializer.Serialize(evt);
            var body = Encoding.UTF8.GetBytes(payload);
            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            props.ContentType = "application/json";
            props.MessageId = evt.EventId.ToString();
            props.CorrelationId = evt.CorrelationId;
            props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "RabbitMQ publish failed for FeedbackId={FeedbackId}. Feedback flow will continue.",
                evt.FeedbackId);
        }

        return Task.CompletedTask;
    }
}
