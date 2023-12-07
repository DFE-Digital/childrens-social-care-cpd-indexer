namespace Childrens_Social_Care_CPD_Indexer.Core;

internal interface IResourcesIndexer
{
    Task CreateIndexAsync(string indexName, CancellationToken cancellationToken);
    Task DeleteIndexAsync(string indexName, CancellationToken cancellationToken);
    Task PopulateIndexAsync(string indexName, int batchSize, CancellationToken cancellationToken);
}
