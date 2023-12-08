using Azure;
using Azure.Core;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Childrens_Social_Care_CPD_Indexer.Core;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using System.Diagnostics.CodeAnalysis;

namespace Childrens_Social_Care_CPD_Indexer.Tests.Core;

internal sealed class MockResponse : Response
{
    public override int Status => 404;
    public override string ReasonPhrase => string.Empty;

    public override Stream? ContentStream
    {
        get => new MemoryStream();
        set => throw new NotImplementedException();
    }
    public override string ClientRequestId
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override void Dispose() => throw new NotImplementedException();
    protected override bool ContainsHeader(string name) => false;
    protected override IEnumerable<HttpHeader> EnumerateHeaders() => Array.Empty<HttpHeader>();
    protected override bool TryGetHeader(string name, [NotNullWhen(true)] out string? value) {
        value = null;
        return false;
    }
    protected override bool TryGetHeaderValues(string name, [NotNullWhen(true)] out IEnumerable<string>? values) => throw new NotImplementedException();
}

public class ResourcesIndexerTests
{
    private ILogger _logger;
    private SearchIndexClient _client;
    private IDocumentFetcher _documentFetcher;
    private ResourcesIndexer _sut;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<ResourcesIndexer>>();
        _client = Substitute.For<SearchIndexClient>();
        _documentFetcher = Substitute.For<IDocumentFetcher>();
        
        _sut = new ResourcesIndexer(_client, _documentFetcher, _logger);
    }
    
    [Test]
    public async Task DeleteIndexAsync_Skips_Deletion_If_Index_Does_Not_Exist()
    {
        // arrange
        var exception = new RequestFailedException(new MockResponse());
        _client.GetIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Throws(exception);

        // act
        await _sut.DeleteIndexAsync("foo");

        // assert
        await _client.Received(0).DeleteIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
    
    [Test]
    public async Task DeleteIndexAsync_Deletes_The_Index()
    {
        // arrange
        var response = Substitute.For<Response<SearchIndex>>();
        response.HasValue.Returns(true);
        _client.GetIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));

        // act
        await _sut.DeleteIndexAsync("foo");

        // assert
        await _client.Received(1).DeleteIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
    
    [Test]
    public async Task DeleteIndexAsync_Logs_Failure_To_Delete_Index()
    {
        // arrange
        var getIndexResult = Substitute.For<Response<SearchIndex>>();
        getIndexResult.HasValue.Returns(true);
        
        var deleteIndexResult = Substitute.For<Response>();
        deleteIndexResult.IsError.Returns(true);
        _client.GetIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getIndexResult));
        _client.DeleteIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(deleteIndexResult));

        // act
        await _sut.DeleteIndexAsync("foo");

        // assert
        _logger.Received(1).LogError("Failed to delete the index");
    }
    
    [Test]
    public async Task CreateIndexAsync_Creates_The_Index()
    {
        // arrange
        var exception = new RequestFailedException(new MockResponse());
        _client.GetIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Throws(exception);

        SearchIndex? searchIndex = null;
        await _client.CreateIndexAsync(Arg.Do<SearchIndex>(x => searchIndex = x), Arg.Any<CancellationToken>());

        // act
        await _sut.CreateIndexAsync("foo");

        // assert
        await _client.Received(1).CreateIndexAsync(Arg.Any<SearchIndex>(), Arg.Any<CancellationToken>());
        searchIndex.Should().NotBeNull();
        searchIndex!.Name.Should().Be("foo");
    }

    [Test]
    public async Task CreateIndexAsync_Skips_Index_Creation()
    {
        // arrange
        var getIndexResult = Substitute.For<Response<SearchIndex>>();
        getIndexResult.HasValue.Returns(true);
        _client.GetIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getIndexResult));
        await _client.CreateIndexAsync(Arg.Any<SearchIndex>(), Arg.Any<CancellationToken>());

        // act
        await _sut.CreateIndexAsync("foo");

        // assert
        await _client.Received(0).CreateIndexAsync(Arg.Any<SearchIndex>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task PopulateIndexAsync_Uploads_Documents()
    {
        var documents = new CpdDocument[]
        {
            new ("foo")
        };

        var client = Substitute.For<SearchClient>();
        _client.GetSearchClient(Arg.Any<string>()).Returns(client);
        _documentFetcher
            .FetchBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(documents), Task.FromResult(Array.Empty<CpdDocument>()));

        await _sut.PopulateIndexAsync("foo", 10);

        await client.Received(1)
            .UploadDocumentsAsync(documents, Arg.Any<IndexDocumentsOptions>(), Arg.Any<CancellationToken>());
    }
    
    [Test]
    public async Task PopulateIndexAsync_Uploads_Documents_In_Multiple_Batches()
    {
        var documents = new[] { new CpdDocument("foo") };
        var client = Substitute.For<SearchClient>();
        _client.GetSearchClient(Arg.Any<string>()).Returns(client);
        _documentFetcher
            .FetchBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(documents), Task.FromResult(documents), Task.FromResult(Array.Empty<CpdDocument>()));

        await _sut.PopulateIndexAsync("foo", 10);

        await client.Received(2)
            .UploadDocumentsAsync(documents, Arg.Any<IndexDocumentsOptions>(), Arg.Any<CancellationToken>());
    }
}