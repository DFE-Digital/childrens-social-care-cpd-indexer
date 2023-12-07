using Microsoft.Extensions.Configuration;

namespace Childrens_Social_Care_CPD_Indexer.Tests;

public class ResourcesIndexerConfigTests
{
    [TestCase("ApiKey", "CPD_SEARCH_API_KEY", "Api key", "Api key")]
    [TestCase("AppInsightsConnectionString", "CPD_INSTRUMENTATION_CONNECTIONSTRING", "Connection string", "Connection string")]
    [TestCase("ApplicationVersion", "VCS-TAG", "1.0.0", "1.0.0")]
    [TestCase("BatchSize", "CPD_SEARCH_BATCH_SIZE", "2", 2)]
    [TestCase("Endpoint", "CPD_SEARCH_ENDPOINT", "Endpoint", "Endpoint")]
    [TestCase("IndexName", "CPD_SEARCH_INDEX_NAME", "Index name", "Index name")]
    [TestCase("ContentfulApiKey", "CPD_DELIVERY_KEY", "Contentful api key", "Contentful api key")]
    [TestCase("ContentfulEnvironmentId", "CPD_CONTENTFUL_ENVIRONMENT", "Environment id", "Environment id")]
    [TestCase("ContentfulSpaceId", "CPD_SPACE_ID", "Space id", "Space id")]
    [TestCase("RecreateIndex", "CPD_SEARCH_RECREATE_INDEX_ON_REBUILD", "true", true)]
    [TestCase("RecreateIndex", "CPD_SEARCH_RECREATE_INDEX_ON_REBUILD", "false", false)]
    public void Config_Returns_Values(string propName, string key, string value, object expected)
    {
        // arrange
        var inMemorySettings = new Dictionary<string, string> {
            {key, value},
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
        var sut = new ResourcesIndexerConfig(configuration);
        var propertyInfo = typeof(ResourcesIndexerConfig).Properties().Single(x => x.Name == propName);

        // act
        var actual = propertyInfo.GetValue(sut);

        // assert
        actual.Should().Be(expected);
    }
    
    [TestCase("ApiKey", "")]
    [TestCase("AppInsightsConnectionString", "")]
    [TestCase("ApplicationVersion", "")]
    [TestCase("BatchSize", 20)]
    [TestCase("Endpoint", "")]
    [TestCase("IndexName", "")]
    [TestCase("ContentfulApiKey", "")]
    [TestCase("ContentfulEnvironmentId", "")]
    [TestCase("ContentfulSpaceId", "")]
    [TestCase("RecreateIndex", true)]
    public void Config_Returns_Default_Values(string propName, object expected)
    {
        // arrange
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()!).Build();
        var sut = new ResourcesIndexerConfig(configuration);
        var propertyInfo = typeof(ResourcesIndexerConfig).Properties().Single(x => x.Name == propName);

        // act
        var actual = propertyInfo.GetValue(sut);

        // assert
        actual.Should().Be(expected);
    }
}