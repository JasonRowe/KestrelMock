using KestrelMockServer.Settings;
using Xunit;

namespace KestrelMockServer.Tests
{
    public class UriTemplateTests
    {

        [Theory, 
            InlineData("/{name}/{surname}/x/{test}?query={query}&x={x}", 
                "/john/reds/x/something?query=hello&x=y")
        ]
        public void UriTemplate_MatchesIncomingPathWithQuery_Ok(string template, string path)
        {
            var x = new UriTemplate(template);

            var result = x.Parse(path);
            Assert.Equal("john", result["name"]);
            Assert.Equal("hello", result["query"]);
            Assert.Equal("reds", result["surname"]);
            Assert.Equal("something", result["test"]);
            Assert.Equal("y", result["x"]);
        }
    }
}
