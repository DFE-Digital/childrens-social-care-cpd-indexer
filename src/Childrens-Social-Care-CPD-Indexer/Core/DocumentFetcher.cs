
using Contentful.Core;
using Contentful.Core.Search;

namespace Childrens_Social_Care_CPD_Indexer.Core;

internal class DocumentFetcher(IContentfulClient client) : IDocumentFetcher
{
    public async Task<CpdDocument[]> FetchBatchAsync(int limit, int skip, CancellationToken cancellationToken = default)
    {
        var queryBuilder = QueryBuilder<Content>.New
            .ContentTypeIs("content")
            .FieldEquals("fields.contentType", "Resource")
            .Skip(skip)
            .Limit(limit)
            .Include(1);

        var contentfulCollection = await client.GetEntries(queryBuilder, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return contentfulCollection.Select(BuildResourceDocument).ToArray();
    }

    private static CpdDocument BuildResourceDocument(Content content)
    {
        var tags = content.Metadata?.Tags.Select(x => x.Sys.Id);
        return new CpdDocument(content.Id!)
        {
            Title = content.ContentTitle,
            ContentType = content.ContentType,
            Body = content.SearchSummary,
            CreatedAt = content.Sys!.CreatedAt.HasValue ? new DateTimeOffset(content.Sys.CreatedAt.Value) : null,
            UpdatedAt = content.Sys!.UpdatedAt.HasValue ? new DateTimeOffset(content.Sys.UpdatedAt.Value) : null, 
            EstimatedReadingTime = content.EstimatedReadingTime,
            Tags = tags
        };
    }
}