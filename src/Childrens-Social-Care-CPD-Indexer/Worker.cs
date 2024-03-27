using Childrens_Social_Care_CPD_Indexer.Core;
using Microsoft.ApplicationInsights;

namespace Childrens_Social_Care_CPD_Indexer;

public class Worker(ILogger<Worker> logger, IResourcesIndexer resourcesIndexer, IApplicationConfiguration applicationConfiguration, IHostApplicationLifetime hostApplicationLifetime, TelemetryClient telemetryClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await DoWork(stoppingToken).ContinueWith(task => hostApplicationLifetime.StopApplication(), stoppingToken);

    private async Task DoWork(CancellationToken stoppingToken)
    {
        logger.LogInformation("Indexing started at: {startTime}", DateTime.Now);
        try
        {
            if (applicationConfiguration.SearchIndexing.RecreateIndex)
            {
                await resourcesIndexer.DeleteIndexAsync(applicationConfiguration.SearchIndexing.IndexName, stoppingToken);
            }
            await resourcesIndexer.CreateIndexAsync(applicationConfiguration.SearchIndexing.IndexName, stoppingToken);
            await resourcesIndexer.PopulateIndexAsync(applicationConfiguration.SearchIndexing.IndexName, applicationConfiguration.SearchIndexing.BatchSize, stoppingToken);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occured");
        }
        finally
        {
            logger.LogInformation("Indexing finished at: {finishTime}", DateTime.Now);
            await telemetryClient.FlushAsync(stoppingToken);
        }
    }
}
