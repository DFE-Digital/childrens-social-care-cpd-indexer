using System.Diagnostics.CodeAnalysis;

namespace Childrens_Social_Care_CPD_Indexer;

public interface IApplicationInsightsConfig
{
    string ConnectionString { get; set; }
}

public interface IContentfulConfig
{
    string DeliveryKey { get; set; }
    string Environment { get; set; }
    string SpaceId { get; set; }
}

public interface ISearchIndexingConfig
{
    string ApiKey { get; set; }
    int BatchSize { get; set; }
    string Endpoint { get; set; }
    string IndexName { get; set; }
    bool RecreateIndex { get; set; }
}

public interface IApplicationConfiguration
{
    string ApplicationVersion { get; set; }
    IApplicationInsightsConfig ApplicationInsights { get; set; }
    IContentfulConfig Contentful { get; set; }
    ISearchIndexingConfig SearchIndexing { get; set; }
}

[ExcludeFromCodeCoverage]
internal class ApplicationInsightsConfig : IApplicationInsightsConfig
{
    public string ConnectionString { get; set; } = string.Empty;
}

[ExcludeFromCodeCoverage]
internal class ContentfulConfig: IContentfulConfig
{
    public string DeliveryKey { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string SpaceId { get; set; } = string.Empty;
}

[ExcludeFromCodeCoverage]
internal class SearchIndexingConfig: ISearchIndexingConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 25;
    public string Endpoint { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public bool RecreateIndex { get; set; } = false;
}

[ExcludeFromCodeCoverage]
internal class ApplicationConfiguration: IApplicationConfiguration
{
    public IApplicationInsightsConfig ApplicationInsights { get; set; } = new ApplicationInsightsConfig();
    public string ApplicationVersion { get; set; } = string.Empty;
    public IContentfulConfig Contentful { get; set; } = new ContentfulConfig();
    public ISearchIndexingConfig SearchIndexing { get; set; } = new SearchIndexingConfig();
}