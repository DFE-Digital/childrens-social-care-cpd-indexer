using Childrens_Social_Care_CPD_Indexer.Core;
using Childrens_Social_Care_CPD_Indexer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.WorkerService;
using Azure.Search.Documents.Indexes;
using Azure;
using Contentful.Core.Configuration;
using Contentful.Core;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<IResourcesIndexerConfig, ResourcesIndexerConfig>();
        var config = new ResourcesIndexerConfig(context.Configuration);


        // Logging
        var options = new ApplicationInsightsServiceOptions()
        {
            ApplicationVersion = config.ApplicationVersion,
            ConnectionString = config.AppInsightsConnectionString,
        };

        services.AddApplicationInsightsTelemetryWorkerService(options);
        services.ConfigureFunctionsApplicationInsights();

        // Code dependencies
        services.TryAddTransient<HttpClient, HttpClient>();
        services.AddTransient<IContentfulClient>(servicesProvider => {
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
        services.AddTransient<IDocumentFetcher, DocumentFetcher>();
        services.AddTransient<IResourcesIndexer>(servicesProvider => {
            var logger = servicesProvider.GetRequiredService<ILogger<ResourcesIndexer>>();
            var config = servicesProvider.GetRequiredService<IResourcesIndexerConfig>();
            var documentFetcher = servicesProvider.GetRequiredService<IDocumentFetcher>();
            var searchEndpointUri = new Uri(config.Endpoint);
            var searchIndexClient = new SearchIndexClient(searchEndpointUri, new AzureKeyCredential(config.ApiKey));
            var telemtryClient = servicesProvider.GetRequiredService<TelemetryClient>();
            return new ResourcesIndexer(searchIndexClient, documentFetcher, logger, telemtryClient);
        });
    })
    .Build();

host.Run();
