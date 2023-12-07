using Childrens_Social_Care_CPD_Indexer.Core;
using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Contentful.Core.Search;

namespace Childrens_Social_Care_CPD_Indexer.Tests.Core;

public class DocumentFetcherTests
{
    private IContentfulClient _contentfulClient;
    private DocumentFetcher _sut;  
    
    [SetUp]
    public void Setup()
    {
        _contentfulClient = Substitute.For<IContentfulClient>();
        _sut = new DocumentFetcher(_contentfulClient);
    }

    [Test]
    public async Task FetchBatchAsync_Applies_Limit()
    {
        // arrange
        const int limit = 12;
        QueryBuilder<Content>? queryBuilder = null;
        _contentfulClient
            .GetEntries(Arg.Do<QueryBuilder<Content>>(qb => queryBuilder = qb), Arg.Any<CancellationToken>())
            .Returns(new ContentfulCollection<Content> { Items = new List<Content>() });

        // act
        await _sut.FetchBatchAsync(limit, 0);

        // assert
        queryBuilder.Should().NotBeNull();
        queryBuilder!.Build().Should().Contain($"limit={limit}");
    }
    
    [Test]
    public async Task FetchBatchAsync_Applies_Skip()
    {
        // arrange
        const int skip = 12;
        QueryBuilder<Content>? queryBuilder = null;
        _contentfulClient
            .GetEntries(Arg.Do<QueryBuilder<Content>>(qb => queryBuilder = qb), Arg.Any<CancellationToken>())
            .Returns(new ContentfulCollection<Content> { Items = new List<Content>() });

        // act
        await _sut.FetchBatchAsync(100, skip);

        // assert
        queryBuilder.Should().NotBeNull();
        queryBuilder!.Build().Should().Contain($"skip={skip}");
    }
    
    [Test]
    public async Task FetchBatchAsync_Fetches_Only_A_Single_Level_Of_Content()
    {
        // arrange
        QueryBuilder<Content>? queryBuilder = null;
        _contentfulClient
            .GetEntries(Arg.Do<QueryBuilder<Content>>(qb => queryBuilder = qb), Arg.Any<CancellationToken>())
            .Returns(new ContentfulCollection<Content> { Items = new List<Content>() });

        // act
        await _sut.FetchBatchAsync(100, 0);

        // assert
        queryBuilder.Should().NotBeNull();
        queryBuilder!.Build().Should().Contain($"include=1");
    }
    
    [Test]
    public async Task FetchBatchAsync_Fetches_Only_Resources()
    {
        // arrange
        QueryBuilder<Content>? queryBuilder = null;
        _contentfulClient
            .GetEntries(Arg.Do<QueryBuilder<Content>>(qb => queryBuilder = qb), Arg.Any<CancellationToken>())
            .Returns(new ContentfulCollection<Content> { Items = new List<Content>() });

        // act
        await _sut.FetchBatchAsync(100, 0);

        // assert
        queryBuilder.Should().NotBeNull();
        queryBuilder!.Build().Should().Contain($"fields.contentType=Resource");
    }
    
    [Test]
    public async Task FetchBatchAsync_Returns_Documents()
    {
        // arrange
        var contentItem = new Content()
        {
            Id = "Id",
            ContentTitle = "Content title",
            ContentType = "Content type",
            SearchSummary = "Search summary",
            Metadata = new ContentfulMetadata()
            {
                Tags = new List<Reference>
                {
                    new() { Sys = new ReferenceProperties { Id = "Tag id" } }
                }
            },
            Sys = new SystemProperties()
            {
                CreatedAt = null,
                UpdatedAt = DateTime.Now
            }
        };
        
        _contentfulClient
            .GetEntries(Arg.Any<QueryBuilder<Content>>(), Arg.Any<CancellationToken>())
            .Returns(new ContentfulCollection<Content> { Items = new List<Content> { contentItem } });

        // act
        var documents = await _sut.FetchBatchAsync(100, 0);

        // assert
        documents.Length.Should().Be(1);
        documents[0].Title.Should().Be(contentItem.ContentTitle);
        documents[0].ContentType.Should().Be(contentItem.ContentType);
        documents[0].Body.Should().Be(contentItem.SearchSummary);
        documents[0].CreatedAt.Should().BeNull();
        documents[0].UpdatedAt.Should().NotBeNull();
        documents[0].Tags.Should().NotBeNull();
        documents[0].Tags?.Count().Should().Be(1);
        documents[0].Tags?.First().Should().Be(contentItem.Metadata.Tags.First().Sys.Id);
    }
}