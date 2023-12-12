namespace Childrens_Social_Care_CPD_Indexer.Core;

public interface IResourcesIndexerConfig
{
    string ApiKey { get; }
    string AppInsightsConnectionString { get; }
    string ApplicationVersion { get; }
    int BatchSize { get; }
    string Endpoint { get;  }
    string IndexName { get; }
    string ContentfulApiKey { get; }
    string ContentfulEnvironmentId { get; }
    string ContentfulSpaceId { get; }
    bool RecreateIndex { get; }
}