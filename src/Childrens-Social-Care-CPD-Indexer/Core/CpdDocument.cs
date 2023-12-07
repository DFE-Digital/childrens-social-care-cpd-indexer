using System.Diagnostics.CodeAnalysis;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.IdentityModel.Tokens;

namespace Childrens_Social_Care_CPD_Indexer.Core;

[ExcludeFromCodeCoverage]
internal partial class CpdDocument(string id)
{
    [SimpleField(IsKey = true)]
    public string? Id { get; set; } = Base64UrlEncoder.Encode(id);

    [SimpleField()]
    public string? Title { get; set; }

    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public string? ContentType { get; set; }
    
    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string? Body { get; set; }
    
    [SimpleField(IsSortable = true, IsFilterable = true)]
    public DateTimeOffset? CreatedAt { get; set; }
    
    [SimpleField(IsSortable = true, IsFilterable = true)]
    public DateTimeOffset? UpdatedAt { get; set; }
    
    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public IEnumerable<string>? Tags { get; set; }
}