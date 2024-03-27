using System.Diagnostics.CodeAnalysis;
using Contentful.Core.Models;
using Newtonsoft.Json;

namespace Childrens_Social_Care_CPD_Indexer.Core;

[ExcludeFromCodeCoverage]
internal static class ContentTypes
{
    public const string Resource = "Resource";
}

[ExcludeFromCodeCoverage]
internal class Content : IContent
{
    public string? Id { get; set; }
    public string? ContentType { get; set; }
    public string? Title { get; set; }
    public string? ContentTitle { get; set; }
    public string? ContentSubtitle { get; set; }
    public string? SearchSummary { get; set; }
    public bool ShowContentHeader { get; set; }
    public string? Category { get; set; }
    public IContent? BackLink { get; set; }
    public List<IContent>? Items { get; set; }
    public IContent? Navigation { get; set; }
    public IContent? RelatedContent { get; set; }
    public int? EstimatedReadingTime { get; set; }

    [JsonProperty("$metadata")]
    public ContentfulMetadata? Metadata { get; set; }
    public SystemProperties? Sys { get; set; }
}