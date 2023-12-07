using Childrens_Social_Care_CPD_Indexer.Core;
using Microsoft.IdentityModel.Tokens;

namespace Childrens_Social_Care_CPD_Indexer.Tests.Core;

public class CpdDocumentTests
{
    [Test]
    public void Constructor_UrlEncodes_The_Id()
    {
        // arrange
        var id = "foo/foo";
        var encodedId = Base64UrlEncoder.Encode(id);

        // act
        var document = new CpdDocument(id);
        
        // assert
        document.Id.Should().Be(encodedId);
    } 
}