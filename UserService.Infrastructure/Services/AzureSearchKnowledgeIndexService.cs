using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.Services;
using UserService.Infrastructure.AISearch;

namespace UserService.Infrastructure.Services
{
    public class AzureSearchKnowledgeIndexService : IKnowledgeIndexService
    {
        private readonly SearchIndexClient _indexClient;
        private readonly string _indexName;

        public AzureSearchKnowledgeIndexService(IConfiguration configuration)
        {
            var endpoint = configuration["AzureSearch:Endpoint"];
            var apiKey = configuration["AzureSearch:ApiKey"];
            _indexName = configuration["AzureSearch:IndexName"]!;

            _indexClient = new SearchIndexClient(
                new Uri(endpoint!),
                new AzureKeyCredential(apiKey!)
            );
        }

        public async Task CreateOrUpdateIndexAsync()
        {
            var fieldBuilder = new FieldBuilder();
            var fields = fieldBuilder.Build(typeof(AppKnowledgeDocument));

            var vectorSearch = new VectorSearch
            {
                Profiles =
                {
                    new VectorSearchProfile(
                        "app-vector-profile",
                        "app-hnsw-config")
                },
                Algorithms =
                {
                    new HnswAlgorithmConfiguration("app-hnsw-config")
                }
            };

            var index = new SearchIndex(_indexName)
            {
                Fields = fields,
                VectorSearch = vectorSearch
            };

            await _indexClient.CreateOrUpdateIndexAsync(index);
        }
    }
}
