using Contentful.Core.Configuration;

namespace Childrens_Social_Care_CPD_Indexer.Core;

internal class EntityResolver : IContentTypeResolver
{
    public Type? Resolve(string? contentTypeId)
    {
        return contentTypeId switch
        {
            "content" => typeof(Content),
            _ => null
        };
    }
}

