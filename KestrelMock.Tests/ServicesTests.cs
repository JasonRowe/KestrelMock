using System.Collections.Generic;
using FluentAssertions;
using KestrelMockServer.Domain;
using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Xunit;

namespace KestrelMock.Tests
{
    public class ServicesTests
    {
        [Fact]
        public void BodyRewriterService_RegexBodyRewrite_HandlesNoMatch()
        {
            var input = "{\"name\": \"old\"}";
            var result = input.RegexBodyRewrite("age", "31");
            result.Should().Be("{\"name\": \"old\"}");
        }

        [Fact]
        public void BodyRewriterService_RegexBodyRewrite_ReplacesStringValues()
        {
            var input = "{\"name\": \"old\"}";
            var result = input.RegexBodyRewrite("name", "new");
            result.Should().Be("{\"name\":\"new\"}");
        }

        [Fact]
        public void BodyRewriterService_RegexBodyRewrite_ReplacesNumericValues()
        {
            var input = "{\"age\": 30}";
            var result = input.RegexBodyRewrite("age", "31");
            result.Should().Be("{\"age\":31}");
        }

        [Fact]
        public void BodyRewriterService_RegexBodyRewrite_HandlesDecimalValues()
        {
            var input = "{\"price\": 19.99}";
            var result = input.RegexBodyRewrite("price", "20.00");
            result.Should().Be("{\"price\":20.00}");
        }

        [Fact]
        public void ResponseMatcherService_FindMatchingResponseMock_ReturnsNull_WhenNoMatch()
        {
            var mapping = new InputMappings();
            var service = new ResponseMatcherService();
            var result = service.FindMatchingResponseMock("/unknown", "GET", null, mapping, new Watcher());
            result.Should().BeNull();
        }

        [Fact]
        public void ResponseMatcherService_FindMatchingResponseMock_MatchesPathStartsWith_WrongMethod()
        {
            var mapping = new InputMappings();
            var setting = new HttpMockSetting
            {
                Request = new Request
                {
                    PathStartsWith = "/test",
                    Methods = new List<string> { "POST" }
                },
                Response = new Response { Status = 200 }
            };
            var bag = new System.Collections.Concurrent.ConcurrentBag<HttpMockSetting>();
            bag.Add(setting);
            mapping.PathStartsWithMapping.TryAdd(new PathStartsWithMappingKey { PathStartsWith = "/test", Method = "POST" }, bag);
            
            var service = new ResponseMatcherService();
            var result = service.FindMatchingResponseMock("/test/path", "GET", null, mapping, new Watcher());
            result.Should().BeNull();
        }

        [Fact]
        public void ResponseMatcherService_CheckBodyMapping_MatchesBodyContains_WrongMethod()
        {
            var mapping = new InputMappings();
            var setting = new HttpMockSetting
            {
                Request = new Request
                {
                    Path = "/test",
                    Methods = new List<string> { "POST" },
                    BodyContains = "match"
                },
                Response = new Response { Status = 200 }
            };
            var key = new PathMappingKey { Path = "/test", Method = "POST" };
            var bag = new System.Collections.Concurrent.ConcurrentBag<HttpMockSetting>();
            bag.Add(setting);
            mapping.BodyCheckMapping.TryAdd(key, bag);

            var service = new ResponseMatcherService();
            // We use POST in the key but GET in the call to simulate finding the key but failing the method check inside CheckBodyMapping
            var result = service.FindMatchingResponseMock("/test", "GET", "match", mapping, new Watcher());
            result.Should().BeNull();
        }
    }
}
