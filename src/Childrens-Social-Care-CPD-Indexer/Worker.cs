using Childrens_Social_Care_CPD_Indexer.Core;

namespace Childrens_Social_Care_CPD_Indexer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IResourcesIndexer _resourcesIndexer;
    private readonly IResourcesIndexerConfig _config;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public Worker(ILogger<Worker> logger, IResourcesIndexer resourcesIndexer, IResourcesIndexerConfig config, IHostApplicationLifetime hostApplicationLifetime)
    {
        _logger = logger;
        _resourcesIndexer = resourcesIndexer;
        _config = config;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => 
        await DoWork(stoppingToken).ContinueWith(task => _hostApplicationLifetime.StopApplication(), stoppingToken);

    private async Task DoWork(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Indexing started at: {startTime}", DateTime.Now);
        try
        {
            if (_config.RecreateIndex)
            {
                await _resourcesIndexer.DeleteIndexAsync(_config.IndexName, stoppingToken);
            }
            await _resourcesIndexer.CreateIndexAsync(_config.IndexName, stoppingToken);
            await _resourcesIndexer.PopulateIndexAsync(_config.IndexName, _config.BatchSize, stoppingToken);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");
        }
        finally
        {
            _logger.LogInformation("Indexing finished at: {finishTime}", DateTime.Now);
        }
    }
}
