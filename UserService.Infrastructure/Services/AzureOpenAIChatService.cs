using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.Services;

namespace UserService.Infrastructure.Services
{

    public class AzureOpenAIChatService : IChatCompletionService
    {
        private readonly ChatClient _chatClient;

        public AzureOpenAIChatService(IConfiguration configuration)
        {
            var endpoint =
                configuration["AzureOpenAI:ChatEndpoint"];

            var apiKey =
                configuration["AzureOpenAI:ChatApiKey"];

            var deployment =
                configuration["AzureOpenAI:ChatDeployment"];

            var azureClient = new AzureOpenAIClient(
                new Uri(endpoint!),
                new AzureKeyCredential(apiKey!)
            );

            _chatClient = azureClient.GetChatClient(deployment!);
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    """
                    You are a helpful in-app assistant.
                    Answer clearly and concisely.
                    Keep answers short.
                    """
                ),

                new UserChatMessage(prompt)
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.2f,
                MaxOutputTokenCount = 100
            };

            var result = await _chatClient.CompleteChatAsync(
                messages,
                options
            );

            return result.Value.Content[0].Text;
        }
    }
}
