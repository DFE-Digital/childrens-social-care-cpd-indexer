using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;
using Childrens_Social_Care_CPD_Indexer.Core;

namespace Childrens_Social_Care_CPD_Indexer;

internal class IndexingService(IResourcesIndexer resourcesIndexer, ILogger<IndexingService> logger, IResourcesIndexerConfig config) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Indexing started at: {startTime}", DateTime.Now);
        try
        {
            if (config.RecreateIndex)
            {
                await resourcesIndexer.DeleteIndexAsync(config.IndexName, cancellationToken);
            }

            await resourcesIndexer.CreateIndexAsync(config.IndexName, cancellationToken);
            await resourcesIndexer.PopulateIndexAsync(config.IndexName, config.BatchSize, cancellationToken);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occured");
        }
        finally
        {
            logger.LogInformation("Indexing finished at: {finishTime}", DateTime.Now);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
