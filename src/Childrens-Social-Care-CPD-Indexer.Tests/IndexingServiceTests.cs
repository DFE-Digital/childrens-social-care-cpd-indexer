using Childrens_Social_Care_CPD_Indexer.Core;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace Childrens_Social_Care_CPD_Indexer.Tests;

public class IndexingServiceTests
{
    private ILogger<Indexer> _logger;
    private IResourcesIndexerConfig _config;
    private IResourcesIndexer _indexer;
    private Indexer _sut;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<Indexer>>();
        _config = Substitute.For<IResourcesIndexerConfig>();
        _indexer = Substitute.For<IResourcesIndexer>();
        _sut = new Indexer(_logger, _indexer, _config);
    }

    [Test]
    public async Task StartAsync_Deletes_Index_If_Configured()
    {
        // arrange
        _config.RecreateIndex.Returns(true);

        // act
        await _sut.Run(new TimerInfo());

        // assert
        await _indexer.Received(1).DeleteIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _indexer.Received(1).CreateIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task StartAsync_Populates_Index()
    {
        // arrange
        _config.RecreateIndex.Returns(false);

        // act
        await _sut.Run(new TimerInfo());

        // assert
        await _indexer.Received(1).PopulateIndexAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task StartAsync_Logs_Exception()
    {
        // arrange
        var exception = new InvalidOperationException();
        _config.RecreateIndex.Returns(true);
        _indexer.DeleteIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Throws(exception);

        // act
        await _sut.Run(new TimerInfo());

        // assert
        _logger.Received(1).LogError(exception, "Unhandled exception occured");
        await _indexer.Received(0).CreateIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _indexer.Received(0).PopulateIndexAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}