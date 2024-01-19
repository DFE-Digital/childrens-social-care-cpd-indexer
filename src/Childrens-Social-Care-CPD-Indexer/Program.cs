using Azure.Search.Documents.Indexes;
using Azure;
using Childrens_Social_Care_CPD_Indexer;
using Childrens_Social_Care_CPD_Indexer.Core;
using Contentful.Core.Configuration;
using Contentful.Core;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.ApplicationInsights;
using System.Diagnostics.CodeAnalysis;
using Azure.Identity;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var applicationConfiguration = new ApplicationConfiguration();

if (!builder.Configuration.GetValue<bool>("LOCAL_ENVIRONMENT"))
{
    var keyVaultUri = new Uri($"https://{builder.Configuration.GetValue<string>("CPD_KEY_VAULT_NAME") ?? string.Empty}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}

builder.Configuration.Bind(builder.Configuration.GetValue<string>("CPD_CONFIG_SECTION_NAME") ?? string.Empty, applicationConfiguration);
builder.Services.AddSingleton<IApplicationConfiguration>(applicationConfiguration);
// Logging

var options = new ApplicationInsightsServiceOptions()
{
    ApplicationVersion = applicationConfiguration.ApplicationVersion,
    ConnectionString = applicationConfiguration.ApplicationInsights.ConnectionString,
};

builder.Services.AddApplicationInsightsTelemetryWorkerService(options);

// Code dependencies
builder.Services.AddTransient<HttpClient, HttpClient>();
builder.Services.AddTransient<IContentfulClient>(servicesProvider => {
    var httpClient = servicesProvider.GetRequiredService<HttpClient>();
    var applicationConfiguration = servicesProvider.GetRequiredService<IApplicationConfiguration>();
    var contentfulOptions = new ContentfulOptions()
    {
        DeliveryApiKey = applicationConfiguration.Contentful.DeliveryKey,
        SpaceId = applicationConfiguration.Contentful.SpaceId,
        Environment = applicationConfiguration.Contentful.Environment
    };
    return new ContentfulClient(httpClient, contentfulOptions);
});

builder.Services.AddTransient<IDocumentFetcher, DocumentFetcher>();
builder.Services.AddTransient<IResourcesIndexer>(servicesProvider => {
    var logger = servicesProvider.GetRequiredService<ILogger<ResourcesIndexer>>();
    var applicationConfiguration = servicesProvider.GetRequiredService<IApplicationConfiguration>();
    var documentFetcher = servicesProvider.GetRequiredService<IDocumentFetcher>();
    var searchEndpointUri = new Uri(applicationConfiguration.SearchIndexing.Endpoint);
    var searchIndexClient = new SearchIndexClient(searchEndpointUri, new AzureKeyCredential(applicationConfiguration.SearchIndexing.ApiKey));
    var telemtryClient = servicesProvider.GetRequiredService<TelemetryClient>();
    return new ResourcesIndexer(searchIndexClient, documentFetcher, logger, telemtryClient);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program() { }