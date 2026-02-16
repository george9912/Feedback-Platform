using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NotificationService.Services;
using System;

namespace NotificationService
{
    public class NotificationFunction
    {
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;

        public NotificationFunction(ILoggerFactory loggerFactory, INotificationService notificationService)
        {
            _logger = loggerFactory.CreateLogger<NotificationFunction>();
            _notificationService = notificationService;
        }

        [Function("SendFeedbackNotifications")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            try
            {
                _logger.LogInformation($"Function executed at: {DateTime.UtcNow}");
                await _notificationService.SendFeedbackNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send feedback notifications.");
            }
        }
    }
}
