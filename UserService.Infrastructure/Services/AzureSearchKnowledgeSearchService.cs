using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
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
    public class AzureSearchKnowledgeSearchService : IKnowledgeSearchService
    {
        private readonly SearchClient _searchClient;
        private readonly IEmbeddingService _embeddingService;

        public AzureSearchKnowledgeSearchService(
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

        public async Task<List<KnowledgeChunk>> SearchRelevantChunksAsync(string question, int top = 3)
        {
            var questionVector = await _embeddingService.GenerateEmbeddingAsync(question);

            var vectorQuery = new VectorizedQuery(questionVector)
            {
                KNearestNeighborsCount = top,
                Fields = { "ContentVector" }
            };

            var options = new SearchOptions
            {
                Size = top,
                VectorSearch = new VectorSearchOptions
                {
                    Queries = { vectorQuery }
                }
            };

            var results = await _searchClient.SearchAsync<AppKnowledgeDocument>(
                searchText: null,
                options: options
            );

            var chunks = new List<KnowledgeChunk>();

            await foreach (var result in results.Value.GetResultsAsync())
            {
                chunks.Add(new KnowledgeChunk
                {
                    Id = result.Document.Id,
                    Content = result.Document.Content,
                    Category = result.Document.Category,
                    Source = result.Document.Source
                });
            }

            return chunks;
        }
    }
}
