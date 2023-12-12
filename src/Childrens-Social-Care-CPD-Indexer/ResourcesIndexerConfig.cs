using Childrens_Social_Care_CPD_Indexer.Core;
using Microsoft.Extensions.Configuration;

namespace Childrens_Social_Care_CPD_Indexer;

internal class ResourcesIndexerConfig(IConfiguration configuration) : IResourcesIndexerConfig
{
    public string ApiKey => configuration.GetValue("CPD_SEARCH_API_KEY", string.Empty)!;
    public string AppInsightsConnectionString => configuration.GetValue("CPD_INSTRUMENTATION_CONNECTIONSTRING", string.Empty)!;
    public string ApplicationVersion => configuration.GetValue("VCS-TAG", string.Empty)!;
    public int BatchSize => configuration.GetValue("CPD_SEARCH_BATCH_SIZE", 20);
    public string Endpoint => configuration.GetValue("CPD_SEARCH_ENDPOINT", string.Empty)!;
    public string IndexName => configuration.GetValue("CPD_SEARCH_INDEX_NAME", string.Empty)!;
    public string ContentfulApiKey => configuration.GetValue("CPD_DELIVERY_KEY", string.Empty)!;
    public string ContentfulEnvironmentId => configuration.GetValue("CPD_CONTENTFUL_ENVIRONMENT", string.Empty)!;
    public string ContentfulSpaceId => configuration.GetValue("CPD_SPACE_ID", string.Empty)!;
    public bool RecreateIndex => configuration.GetValue("CPD_SEARCH_RECREATE_INDEX_ON_REBUILD", true);
}
