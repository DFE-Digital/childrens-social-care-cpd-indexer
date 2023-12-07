using Contentful.Core.Configuration;
using Contentful.Core;
using Azure.Search.Documents.Indexes;
using Azure;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;
using Childrens_Social_Care_CPD_Indexer.Core;
using CoreIndexer = Childrens_Social_Care_CPD_Indexer.Core.ResourcesIndexer;

namespace Childrens_Social_Care_CPD_Indexer;

internal class Indexer(TelemetryClient telemetryClient, ILogger<Indexer> logger, ILogger<CoreIndexer> coreLogger, IResourcesIndexerConfig config) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Indexing started at: {startTime}", DateTime.Now);
        try
        {
            var documentFetcher = CreateDocumentFetcher(config);
            var searchEndpointUri = new Uri(config.Endpoint);
            var searchIndexClient = new SearchIndexClient(searchEndpointUri, new AzureKeyCredential(config.ApiKey));
            var indexer = new CoreIndexer(searchIndexClient, documentFetcher, coreLogger);

            if (config.RecreateIndex)
            {
                using (telemetryClient.StartOperation<RequestTelemetry>("DeleteIndex"))
                {
                    await indexer.DeleteIndexAsync(config.IndexName, cancellationToken);
                    await indexer.CreateIndexAsync(config.IndexName, cancellationToken);
                }
            }

            await indexer.PopulateIndexAsync(config.IndexName, config.BatchSize, cancellationToken);

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

    private static DocumentFetcher CreateDocumentFetcher(IResourcesIndexerConfig config)
    {
        var contentfulOptions = new ContentfulOptions()
        {
            SpaceId = config.ContentfulSpaceId,
            Environment = config.ContentfulEnvironmentId,
            DeliveryApiKey = config.ContentfulApiKey,
        };

        var contentfulClient = new ContentfulClient(new HttpClient(), contentfulOptions);

        return new DocumentFetcher(contentfulClient);
    }
}
