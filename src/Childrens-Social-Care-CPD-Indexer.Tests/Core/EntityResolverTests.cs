
using Childrens_Social_Care_CPD_Indexer.Core;

namespace Childrens_Social_Care_CPD_Indexer.Tests.Core;

[TestFixture]
public class EntityResolverTests
{
    [TestCase("content", typeof(Content))]
    public void Resolves_Correctly(string contentTypeId, Type expectedType)
    { 
        var resolver = new EntityResolver();

        var actual = resolver.Resolve(contentTypeId);

        actual.Should().Be(expectedType);
    }

    [Test]
    [TestCase("", null)]
    [TestCase(null, null)]
    [TestCase("doesNotExist", null)]
    public void Does_Not_Resolve_Unknown_Content(string? contentTypeId, Type? expectedType)
    {
        var resolver = new EntityResolver();

        var actual = resolver.Resolve(contentTypeId);

        actual.Should().Be(expectedType);
    }
}
