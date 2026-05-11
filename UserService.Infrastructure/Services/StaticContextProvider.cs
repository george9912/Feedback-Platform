using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.Services;

namespace UserService.Infrastructure.Services
{

    public class StaticContextProvider : IAppContextProvider
    {
        // Simulate a static knowledge base (could be loaded from file or DB)
        private readonly List<(string Key, string Info)> _knowledgeBase = new()
    {
        ("users", "The **Directory** page (under the sidebar 'Directory') shows a list of users in the system."),
        ("my profile", "The **MyProfile** page shows your personal information and settings."),
        ("feedbacks", "The **Feedbacks** section allows viewing and tracking user feedback submissions."),
        ("home", "The **Home** dashboard provides an overview and recent notifications."),
        // add more context pairs as needed
    };

        public string GetRelevantContext(string userQuestion)
        {
            if (string.IsNullOrWhiteSpace(userQuestion)) return string.Empty;
            string q = userQuestion.ToLower();
            // Naive approach: return all info whose key is mentioned in the question
            var matches = _knowledgeBase
                .Where(entry => q.Contains(entry.Key))
                .Select(entry => entry.Info);
            return string.Join("\n", matches);
        }
    }

}
