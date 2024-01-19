using Childrens_Social_Care_CPD_Indexer.Core;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace Childrens_Social_Care_CPD_Indexer.Tests;

public class WorkerTests
{
    private ILogger<Worker> _logger;
    private IApplicationConfiguration _config;
    private IResourcesIndexer _indexer;
    private IHostApplicationLifetime _hostingApplicationLifetime;
    private TelemetryClient _telemetryClient;
    private Worker _sut;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<Worker>>();
        _config = Substitute.For<IApplicationConfiguration>();
        _indexer = Substitute.For<IResourcesIndexer>();
        _hostingApplicationLifetime = Substitute.For<IHostApplicationLifetime>();

        var configuration = new TelemetryConfiguration();
        var sendItems = new List<ITelemetry>();
        var channel = Substitute.For<ITelemetryChannel>();
        configuration.TelemetryChannel = channel;
        configuration.ConnectionString = $"InstrumentationKey={Guid.NewGuid()};IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/";
        configuration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
        _telemetryClient = new TelemetryClient(configuration);

        _sut = new Worker(_logger, _indexer, _config, _hostingApplicationLifetime, _telemetryClient);
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
        _config.SearchIndexing.RecreateIndex.Returns(true);
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
        _config.SearchIndexing.RecreateIndex.Returns(false);
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
        _config.SearchIndexing.RecreateIndex.Returns(true);
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