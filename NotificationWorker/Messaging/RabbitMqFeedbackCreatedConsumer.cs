using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NotificationWorker.Common.Events;
using NotificationWorker.Processing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationWorker.Messaging;

public sealed class RabbitMqFeedbackCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqFeedbackCreatedConsumer> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RabbitMqFeedbackCreatedConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMqFeedbackCreatedConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = _configuration["RabbitMq:Host"] ?? "rabbitmq";
        var userName = _configuration["RabbitMq:Username"] ?? "guest";
        var password = _configuration["RabbitMq:Password"] ?? "guest";
        var virtualHost = _configuration["RabbitMq:VirtualHost"] ?? "/";
        var exchange = _configuration["RabbitMq:FeedbackCreated:Exchange"] ?? "feedback.events";
        var routingKey = _configuration["RabbitMq:FeedbackCreated:RoutingKey"] ?? "feedback.created";
        var queue = _configuration["RabbitMq:FeedbackCreated:Queue"] ?? "notification.feedback.created";
        var dlx = _configuration["RabbitMq:FeedbackCreated:DeadLetterExchange"] ?? "feedback.events.dlx";
        var dlRoutingKey = _configuration["RabbitMq:FeedbackCreated:DeadLetterRoutingKey"] ?? "feedback.created.deadletter";
        var dlq = _configuration["RabbitMq:FeedbackCreated:DeadLetterQueue"] ?? "notification.feedback.created.dlq";

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

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Direct, durable: true, autoDelete: false);
        _channel.ExchangeDeclare(exchange: dlx, type: ExchangeType.Direct, durable: true, autoDelete: false);

        _channel.QueueDeclare(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = dlx,
                ["x-dead-letter-routing-key"] = dlRoutingKey
            });

        _channel.QueueBind(queue: queue, exchange: exchange, routingKey: routingKey);

        _channel.QueueDeclare(
            queue: dlq,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(queue: dlq, exchange: dlx, routingKey: dlRoutingKey);

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            await HandleMessageAsync(ea, stoppingToken);
        };

        _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);

        _logger.LogInformation(
            "RabbitMQ consumer started. Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey}",
            exchange,
            queue,
            routingKey);

        return Task.CompletedTask;
    }

    internal async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken ct)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            var result = await ProcessPayloadAsync(payload, ct);

            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            if (result == FeedbackCreatedProcessResult.Duplicate)
            {
                _logger.LogInformation(
                    "Duplicate message acknowledged. MessageId={MessageId}",
                    ea.BasicProperties?.MessageId);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid FeedbackCreated payload. MessageId={MessageId}", ea.BasicProperties?.MessageId);
            _channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process FeedbackCreated message. MessageId={MessageId}", ea.BasicProperties?.MessageId);
            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    internal async Task<FeedbackCreatedProcessResult> ProcessPayloadAsync(string payload, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<FeedbackCreatedEvent>(payload, JsonOptions)
            ?? throw new InvalidOperationException("FeedbackCreated payload cannot be null.");

        using var scope = _scopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IFeedbackCreatedProcessor>();
        return await processor.ProcessAsync(evt, ct);
    }

    public override void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch
        {
            // Swallow dispose exceptions to avoid crashing shutdown.
        }

        _channel?.Dispose();
        _connection?.Dispose();

        base.Dispose();
    }
}
