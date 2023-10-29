using System.Linq;
using KestrelMockServer.Services;
using Xunit;

namespace KestrelMock.Tests;

public class UriPathReplaceServiceTests
{
    private readonly UriPathReplaceService service;

    public UriPathReplaceServiceTests()
    {
        service = new UriPathReplaceService();
    }

    [Theory,
     InlineData("api/test/{one}?two={two}", 
         "api/test/1?two=2", "one:{one}", "two:{two}"),
     InlineData("api/test/{one}?two={two}", 
         "api/test/1", "one:{one}", "two:2"),
     InlineData("api/test/{one}?two={two}", 
         "api/test/1?whatever=x", "one:{one}", "two:2"),
     InlineData("api/test/{one}?two={two}&three={three}", 
         "api/test/1?whatever=x&two=2", "one:{one}", "two:{two}", "three:{three}")]
    public void ReplaceOk(string uriTemplate, string pathAndQuery, params string[] replacements)
    {
        var body = @"{""one"": 0, ""two"":""a"", ""three"":""hello""}";

        var uriReplacements = replacements.ToDictionary(s => s.Split(":")[0],
            s => s.Split(":")[1]);

        var finalBody = service.UriPathReplacements(pathAndQuery, new KestrelMockServer.Settings.Response
        {
            Body = body,
            Replace =
                new KestrelMockServer.Settings.Replace{
                    UriPathReplacements = uriReplacements,
                    UriTemplate = uriTemplate
                }
        }, body);

        var expectedBody = @"{""one"":1, ""two"":""2"", ""three"":""hello""}";

        Assert.Equal(expectedBody, finalBody);
    }
}