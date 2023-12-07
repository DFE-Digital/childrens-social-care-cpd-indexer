using Microsoft.ApplicationInsights.WorkerService;
using Childrens_Social_Care_CPD_Indexer.Core;
using Childrens_Social_Care_CPD_Indexer;
using Contentful.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Contentful.Core.Configuration;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    var config = new ResourcesIndexerConfig(context.Configuration);
    services.AddTransient<IResourcesIndexerConfig, ResourcesIndexerConfig>();

    var options = new ApplicationInsightsServiceOptions
    {
        ApplicationVersion = config.ApplicationVersion,
        ConnectionString = config.AppInsightsConnectionString
    };

    services.AddApplicationInsightsTelemetryWorkerService(options);
    services.TryAddTransient<HttpClient, HttpClient>();
    services.AddTransient(typeof(IContentfulClient), servicesProvider => {
        var httpClient = servicesProvider.GetRequiredService<HttpClient>();
        var config = servicesProvider.GetRequiredService<IResourcesIndexerConfig>();

        var options = new ContentfulOptions()
        {
            DeliveryApiKey = config.ContentfulApiKey,
            SpaceId = config.ContentfulSpaceId,
            Environment = config.ContentfulEnvironmentId
        };
        return new ContentfulClient(httpClient, options);
    });
    services.AddTransient<IDocumentFetcher, DocumentFetcher>();
    services.AddHostedService<Indexer>();
});

using (var host = builder.Build())
{
    var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
    await host.StartAsync().ContinueWith(x => lifetime.StopApplication());
}