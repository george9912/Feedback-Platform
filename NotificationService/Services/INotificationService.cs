using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Services
{
    public interface INotificationService
    {
        Task SendFeedbackNotificationsAsync();
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
