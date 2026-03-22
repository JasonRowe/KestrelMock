using KestrelMockServer.Domain;
using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;

namespace KestrelMock.Tests
{
    public class ResponseMatcherServiceTests
    {
        [Fact]
        public void CheckEmptyBodyMapping_ReturnsNull_WhenBodyContainsIsConfiguredButNotMatched()
        {
            var body = "no-match";
            var setting = new HttpMockSetting
            {
                Request = new Request
                {
                    PathStartsWith = "/api",
                    BodyContains = "match",
                    Methods = new List<string> { "POST" }
                },
                Response = new Response
                {
                    Status = 200
                }
            };

            var method = typeof(ResponseMatcherService).GetMethod("CheckEmptyBodyMapping", BindingFlags.Static | BindingFlags.NonPublic);
            var result = method.Invoke(null, new object[] { body, setting });

            result.Should().BeNull();
        }

        [Fact]
        public void CheckEmptyBodyMapping_ReturnsNull_WhenBodyDoesNotContainIsConfiguredButNotMatched()
        {
            var body = "has-bad-word";
            var setting = new HttpMockSetting
            {
                Request = new Request
                {
                    PathStartsWith = "/api",
                    BodyDoesNotContain = "bad",
                    Methods = new List<string> { "POST" }
                },
                Response = new Response
                {
                    Status = 200
                }
            };

            var method = typeof(ResponseMatcherService).GetMethod("CheckEmptyBodyMapping", BindingFlags.Static | BindingFlags.NonPublic);
            var result = method.Invoke(null, new object[] { body, setting });

            result.Should().BeNull();
        }

        [Fact]
        public void CheckEmptyBodyMapping_ReturnsNull_WhenBodyContainsArrayIsConfiguredButNotMatched()
        {
            var body = "only-one";
            var setting = new HttpMockSetting
            {
                Request = new Request
                {
                    PathStartsWith = "/api",
                    BodyContainsArray = new List<string> { "one", "two" },
                    Methods = new List<string> { "POST" }
                },
                Response = new Response
                {
                    Status = 200
                }
            };

            var method = typeof(ResponseMatcherService).GetMethod("CheckEmptyBodyMapping", BindingFlags.Static | BindingFlags.NonPublic);
            var result = method.Invoke(null, new object[] { body, setting });

            result.Should().BeNull();
        }

        [Fact]
        public void CheckEmptyBodyMapping_ReturnsResponse_WhenNoBodyMatchersConfigured()
        {
            var body = "some-body";
            var setting = new HttpMockSetting
            {
                Request = new Request
                {
                    PathStartsWith = "/api",
                    Methods = new List<string> { "POST" }
                },
                Response = new Response
                {
                    Status = 200
                }
            };

            var method = typeof(ResponseMatcherService).GetMethod("CheckEmptyBodyMapping", BindingFlags.Static | BindingFlags.NonPublic);
            var result = method.Invoke(null, new object[] { body, setting });

            result.Should().NotBeNull();
        }

        [Fact]
        public void CheckEmptyBodyMapping_ReturnsResponse_WhenBodyIsEmpty()
        {
            string? body = null;
            var setting = new HttpMockSetting
            {
                Request = new Request
                {
                    PathStartsWith = "/api",
                    BodyContains = "match",
                    Methods = new List<string> { "POST" }
                },
                Response = new Response
                {
                    Status = 200
                }
            };

            var method = typeof(ResponseMatcherService).GetMethod("CheckEmptyBodyMapping", BindingFlags.Static | BindingFlags.NonPublic);
            var result = method.Invoke(null, new object[] { body, setting });

            result.Should().NotBeNull();
        }
    }
}
