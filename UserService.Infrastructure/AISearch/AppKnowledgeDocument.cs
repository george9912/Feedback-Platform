using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Infrastructure.AISearch
{
    public class AppKnowledgeDocument
    {
        [SimpleField(IsKey = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [SearchableField]
        public string Content { get; set; } = string.Empty;

        [SimpleField(IsFilterable = true)]
        public string Category { get; set; } = string.Empty;

        [SimpleField(IsFilterable = true)]
        public string Source { get; set; } = string.Empty;

        [VectorSearchField(
            VectorSearchDimensions = 1536,
            VectorSearchProfileName = "app-vector-profile")]
        public float[] ContentVector { get; set; } = Array.Empty<float>();
    }
}
