using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.Services;

namespace UserService.Infrastructure.Services
{
    public class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;

        public AzureOpenAIEmbeddingService(IConfiguration configuration)
        {
            var endpoint = configuration["AzureOpenAIEmbeddings:Endpoint"];
            var apiKey = configuration["AzureOpenAIEmbeddings:ApiKey"];
            var deployment = configuration["AzureOpenAIEmbeddings:Deployment"];

            var azureClient = new AzureOpenAIClient(
                new Uri(endpoint!),
                new AzureKeyCredential(apiKey!)
            );

            _embeddingClient = azureClient.GetEmbeddingClient(deployment!);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var result = await _embeddingClient.GenerateEmbeddingAsync(text);

            return result.Value.ToFloats().ToArray();
        }
    }
}
