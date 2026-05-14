using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Application.DTOs;
using UserService.Application.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]

    public class ChatController : ControllerBase
    {
        private readonly IChatCompletionService _chatService;
        private readonly IKnowledgeSearchService _knowledgeSearchService;
        public ChatController(
      IChatCompletionService chatService,
      IKnowledgeSearchService knowledgeSearchService)
        {
            _chatService = chatService;
            _knowledgeSearchService = knowledgeSearchService;
        }

        public class ChatRequest { public string Question { get; set; } }
        public class ChatResponse { public string Answer { get; set; } }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question cannot be empty.");

            // 1. Retrieve relevant context (if any) for the question
            var chunks = await _knowledgeSearchService.SearchRelevantChunksAsync(request.Question);

            var contextSnippet = string.Join(
                "\n",
                chunks.Select(c => $"- {c.Content}")
            );

            if (string.IsNullOrWhiteSpace(contextSnippet))
            {
                return Ok(new ChatResponse
                {
                    Answer = "I don't have enough context to answer the question."
                });
            }
            // 2. Build the combined prompt
            string prompt =
                   "You are an in-app assistant. Answer only using the application context. " +
                   "Answer in Romanian. Maximum one sentence. " +
                   "Do not ask follow-up questions. Do not create lists.\n\n" +
                   $"Application Context:\n{contextSnippet}\n\n" +
                   $"Question: {request.Question}\n" +
                   "Answer:";

            // 3. Call local LLM for completion
            string answer = await _chatService.GetCompletionAsync(prompt);

            // 4. Return the answer
            return Ok(new ChatResponse { Answer = answer });
        }
    }
}