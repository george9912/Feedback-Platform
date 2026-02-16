using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackService.Domain.Entities
{
    public class Feedback
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid MentorId { get; set; }       
        public Guid StudentId { get; set; }      

        public string Title { get; set; }        
        public string Comments { get; set; }       
        public int Rating { get; set; }            // 1-5

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
