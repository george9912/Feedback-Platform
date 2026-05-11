using Azure;
using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.AISearch;
using UserService.Application.Services;
using UserService.Infrastructure.AISearch;

namespace UserService.Infrastructure.Services
{
    public class AzureSearchKnowledgeIngestionService : IKnowledgeIngestionService
    {
        private readonly SearchClient _searchClient;
        private readonly IEmbeddingService _embeddingService;

        public AzureSearchKnowledgeIngestionService(
            IConfiguration configuration,
            IEmbeddingService embeddingService)
        {
            var endpoint = configuration["AzureSearch:Endpoint"];
            var apiKey = configuration["AzureSearch:ApiKey"];
            var indexName = configuration["AzureSearch:IndexName"];

            _searchClient = new SearchClient(
                new Uri(endpoint!),
                indexName!,
                new AzureKeyCredential(apiKey!)
            );

            _embeddingService = embeddingService;
        }

        public async Task UploadChunksAsync(List<KnowledgeChunk> chunks)
        {
            var documents = new List<AppKnowledgeDocument>();

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);

                documents.Add(new AppKnowledgeDocument
                {
                    Id = chunk.Id,
                    Content = chunk.Content,
                    Category = chunk.Category,
                    Source = chunk.Source,
                    ContentVector = embedding
                });
            }

            await _searchClient.UploadDocumentsAsync(documents);
        }
    }
}
