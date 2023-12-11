using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Childrens_Social_Care_CPD_Indexer.Core;

internal class ResourcesIndexer(SearchIndexClient searchIndexClient, IDocumentFetcher documentFetcher, ILogger<ResourcesIndexer> logger, TelemetryClient telemetryClient) : IResourcesIndexer
{
    private async Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken)
    {
        using var operation = telemetryClient.StartOperation<RequestTelemetry>("GetIndexAsync");
        try
        {
            var index = await searchIndexClient.GetIndexAsync(indexName, cancellationToken);
            return index.HasValue;
        }
        catch (RequestFailedException rf)
        {
            if (rf.Status == 404)
            {
                return false;
            }
            telemetryClient.TrackException(rf);
            throw;
        }
    }

    public async Task CreateIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        using var operation = telemetryClient.StartOperation<RequestTelemetry>("CreateIndexAsync");
        var indexExists = await IndexExistsAsync(indexName, cancellationToken);
        if (indexExists)
        {
            logger.LogInformation("Index already exists, skipping creation.");
            return;
        }

        logger.LogInformation("Creating index...");
        var fieldBuilder = new FieldBuilder();
        var searchFields = fieldBuilder.Build(typeof(CpdDocument));
        var searchIndex = new SearchIndex(indexName, searchFields);
        await searchIndexClient.CreateIndexAsync(searchIndex, cancellationToken);
        logger.LogInformation("Finished index creation");
    }

    public async Task DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        using var operation = telemetryClient.StartOperation<RequestTelemetry>("DeleteIndexAsync");
        logger.LogInformation("Deleting index...");
        var indexExists = await IndexExistsAsync(indexName, cancellationToken);
        if (indexExists)
        {
            var deleteResponse = await searchIndexClient.DeleteIndexAsync(indexName, cancellationToken);
            if (deleteResponse.IsError)
            {
                logger.LogError("Failed to delete the index");
            }
        }
        logger.LogInformation("Finished index deletion");
    }
    
    public async Task PopulateIndexAsync(string indexName, int batchSize, CancellationToken cancellationToken = default)
    {
        using var operation = telemetryClient.StartOperation<RequestTelemetry>("PopulateIndexAsync");
        logger.LogInformation("Populating index...");
        var searchClient = searchIndexClient.GetSearchClient(indexName);
        var skip = 0;
        var batch = await documentFetcher.FetchBatchAsync(batchSize, skip, cancellationToken);
        
        while (batch.Length > 0 && !cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Fetched batch of {documentCount} documents", batch.Length);
            await searchClient.UploadDocumentsAsync(batch, null, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Uploaded batch to index");
            telemetryClient.TrackEvent("Uploaded batch");
            skip += batch.Length;
            batch = await documentFetcher.FetchBatchAsync(batchSize, skip, cancellationToken);
        }

        logger.LogInformation("Finished populating index");
    }
}