using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.AISearch;

namespace UserService.Application.Services
{
    public interface IKnowledgeSearchService
    {
        Task<List<KnowledgeChunk>> SearchRelevantChunksAsync(string question, int top = 3);
    }
}
