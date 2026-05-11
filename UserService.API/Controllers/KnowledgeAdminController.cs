using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Application.AISearch;
using UserService.Application.DTOs;
using UserService.Application.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/admin/knowledge")]
    public class KnowledgeAdminController : ControllerBase
    {
        private readonly IKnowledgeIndexService _knowledgeIndexService;
        private readonly IKnowledgeIngestionService _knowledgeIngestionService;

        public KnowledgeAdminController(
      IKnowledgeIndexService knowledgeIndexService,
      IKnowledgeIngestionService knowledgeIngestionService)
        {
            _knowledgeIndexService = knowledgeIndexService;
            _knowledgeIngestionService = knowledgeIngestionService;
        }

        [HttpPost("create-index")]
        public async Task<IActionResult> CreateIndex()
        {
            await _knowledgeIndexService.CreateOrUpdateIndexAsync();

            return Ok(new
            {
                Message = "Knowledge index created or updated successfully."
            });
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            var chunks = new List<KnowledgeChunk>
                        {
                            new KnowledgeChunk
                            {
                                Content = "Users are available in the Directory page.",
                                Category = "Navigation",
                                Source = "ApplicationSeed"
                            },
                            new KnowledgeChunk
                            {
                                Content = "Authenticated user profile is available in the MyProfile page.",
                                Category = "Navigation",
                                Source = "ApplicationSeed"
                            },
                            new KnowledgeChunk
                            {
                                Content = "Feedbacks are available in the Feedbacks page.",
                                Category = "Navigation",
                                Source = "ApplicationSeed"
                            },
                            new KnowledgeChunk
                            {
                                Content = "Notifications are available in the Notifications page.",
                                Category = "Navigation",
                                Source = "ApplicationSeed"
                            }
                        };

            await _knowledgeIngestionService.UploadChunksAsync(chunks);

            return Ok(new
            {
                Message = "Knowledge chunks uploaded successfully.",
                Count = chunks.Count
            });
        }
    }
}