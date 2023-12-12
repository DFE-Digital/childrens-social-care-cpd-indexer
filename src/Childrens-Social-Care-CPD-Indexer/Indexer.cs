using Childrens_Social_Care_CPD_Indexer.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Childrens_Social_Care_CPD_Indexer;

public class Indexer(ILoggerFactory loggerFactory, IResourcesIndexer resourcesIndexer, IResourcesIndexerConfig config)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Indexer>();

    [Function("IndexResources")]
    public async Task Run([TimerTrigger("0 0 * * SUN"
    #if DEBUG
            , RunOnStartup= true
    #endif
        )] TimerInfo myTimer, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Indexing started at: {startTime}", DateTime.Now);
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
            _logger.LogError(ex, "Unhandled exception occured");
        }
        finally
        {
            _logger.LogInformation("Indexing finished at: {finishTime}", DateTime.Now);
        }
    }
}
