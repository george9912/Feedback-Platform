using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.AISearch;

namespace UserService.Application.Services
{
    public interface IKnowledgeIngestionService
    {
        Task UploadChunksAsync(List<KnowledgeChunk> chunks);
    }
}
