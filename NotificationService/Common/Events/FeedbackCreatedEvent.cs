using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Common.Events
{
    public sealed class FeedbackCreatedEvent
    {
        public Guid FeedbackId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
