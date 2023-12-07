using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace Childrens_Social_Care_CPD_Indexer.Core;

internal class ResourcesIndexer(SearchIndexClient searchIndexClient, IDocumentFetcher documentFetcher, ILogger logger): IResourcesIndexer
{
    public async Task CreateIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating index...");
        var fieldBuilder = new FieldBuilder();
        var searchFields = fieldBuilder.Build(typeof(CpdDocument));
        var searchIndex = new SearchIndex(indexName, searchFields);
        await searchIndexClient.CreateIndexAsync(searchIndex, cancellationToken);
        logger.LogInformation("Finished index creation");
    }

    public async Task DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting index...");

        var index = await searchIndexClient.GetIndexAsync(indexName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        if (index.HasValue)
        {
            var deleteResponse = await searchIndexClient.DeleteIndexAsync(indexName, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (deleteResponse.IsError)
            {
                logger.LogError("Failed to delete the index");
            }
        }
        logger.LogInformation("Finished index deletion");
    }
    
    public async Task PopulateIndexAsync(string indexName, int batchSize, CancellationToken cancellationToken = default)
    {
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
            skip += batch.Length;
            batch = await documentFetcher.FetchBatchAsync(batchSize, skip, cancellationToken);
        }

        logger.LogInformation("Finished populating index");
    }
}