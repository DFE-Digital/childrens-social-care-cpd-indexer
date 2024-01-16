using Azure.Search.Documents.Indexes;
using Azure;
using Childrens_Social_Care_CPD_Indexer;
using Childrens_Social_Care_CPD_Indexer.Core;
using Contentful.Core.Configuration;
using Contentful.Core;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.ApplicationInsights;
using System.Diagnostics.CodeAnalysis;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<IResourcesIndexerConfig, ResourcesIndexerConfig>();
var config = new ResourcesIndexerConfig(builder.Configuration);

// Logging
var options = new ApplicationInsightsServiceOptions()
{
    ApplicationVersion = config.ApplicationVersion,
    ConnectionString = config.AppInsightsConnectionString,
};

builder.Services.AddApplicationInsightsTelemetryWorkerService(options);

// Code dependencies
builder.Services.AddTransient<HttpClient, HttpClient>();
builder.Services.AddTransient<IContentfulClient>(servicesProvider => {
    var httpClient = servicesProvider.GetRequiredService<HttpClient>();
    var resourcesIndexerConfig = servicesProvider.GetRequiredService<IResourcesIndexerConfig>();
    var contentfulOptions = new ContentfulOptions()
    {
        DeliveryApiKey = resourcesIndexerConfig.ContentfulApiKey,
        SpaceId = resourcesIndexerConfig.ContentfulSpaceId,
        Environment = resourcesIndexerConfig.ContentfulEnvironmentId
    };
    return new ContentfulClient(httpClient, contentfulOptions);
});

builder.Services.AddTransient<IDocumentFetcher, DocumentFetcher>();
builder.Services.AddTransient<IResourcesIndexer>(servicesProvider => {
    var logger = servicesProvider.GetRequiredService<ILogger<ResourcesIndexer>>();
    var config = servicesProvider.GetRequiredService<IResourcesIndexerConfig>();
    var documentFetcher = servicesProvider.GetRequiredService<IDocumentFetcher>();
    var searchEndpointUri = new Uri(config.Endpoint);
    var searchIndexClient = new SearchIndexClient(searchEndpointUri, new AzureKeyCredential(config.ApiKey));
    var telemtryClient = servicesProvider.GetRequiredService<TelemetryClient>();
    return new ResourcesIndexer(searchIndexClient, documentFetcher, logger, telemtryClient);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program() { }