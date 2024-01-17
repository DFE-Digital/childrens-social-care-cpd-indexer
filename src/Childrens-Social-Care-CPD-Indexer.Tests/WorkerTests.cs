using Childrens_Social_Care_CPD_Indexer.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace Childrens_Social_Care_CPD_Indexer.Tests;

public class WorkerTests
{
    private ILogger<Worker> _logger;
    private IResourcesIndexerConfig _config;
    private IResourcesIndexer _indexer;
    private IHostApplicationLifetime _hostingApplicationLifetime;
    private Worker _sut;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<Worker>>();
        _config = Substitute.For<IResourcesIndexerConfig>();
        _indexer = Substitute.For<IResourcesIndexer>();
        _hostingApplicationLifetime = Substitute.For<IHostApplicationLifetime>();

        _sut = new Worker(_logger, _indexer, _config, _hostingApplicationLifetime);
    }

    [TearDown]
    public void Teardown()
    {
        _sut.Dispose();
    }

    [Test]
    public async Task StartAsync_Deletes_Index_If_Configured()
    {
        // arrange
        _config.RecreateIndex.Returns(true);
        var cancellationTokenSource = new CancellationTokenSource();

        // act
        await _sut.StartAsync(cancellationTokenSource.Token);

        // assert
        await _indexer.Received(1).DeleteIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _indexer.Received(1).CreateIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task StartAsync_Populates_Index()
    {
        // arrange
        _config.RecreateIndex.Returns(false);
        var cancellationTokenSource = new CancellationTokenSource();

        // act
        await _sut.StartAsync(cancellationTokenSource.Token);

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
        var cancellationTokenSource = new CancellationTokenSource();

        // act
        await _sut.StartAsync(cancellationTokenSource.Token);

        // assert
        _logger.Received(1).LogError(exception, "Unhandled exception occured");
        await _indexer.Received(0).CreateIndexAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _indexer.Received(0).PopulateIndexAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}