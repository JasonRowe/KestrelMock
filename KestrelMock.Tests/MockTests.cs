using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using KestrelMock.Services;
using KestrelMock.Settings;
using KestrelMock.Tests.TestHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Refit;
using Xunit;

namespace KestrelMock.Tests
{
    public class MockTests : IClassFixture<MockTestApplicationFactory>
    {

        [Fact]
        public void ValidateConfiguration()
        {
            try
            {
                KestrelMock.Run(new ConfigurationBuilder().Build());
            }
            catch (Exception ex)
            {
                Assert.Contains("Configuration must include 'MockSettings' section", ex.Message);
            }
        }

        private readonly MockTestApplicationFactory _factory;

        public MockTests(MockTestApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("starts/with/xhsythf")]
        public async Task CanMockResponseUsingPathStartsWith(string url)
        {

            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("banana_x", message);
        }

        [Theory]
        [InlineData("/test/1234/xyz")]
        public async Task CanMockResponseUsingPathRegex_Matches(string url)
        {

            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("regex_is_working", message);
        }

        [Fact]
        public async Task CanMockResponseUsingPathRegex_NoMatch()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/test/abcd/xyz");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CanMockGetResponseUsingExactPath()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("hello/world");

            Assert.Contains("hello", await response.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task CanMockPosttResponseUsingExactPath()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsync("hello/world", new StringContent("test"));

            Assert.Contains("hello", await response.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task CanMockBodyContainsResponse()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("api/estimate", new StringContent("00000"));

            Assert.Contains("BodyContains Works for PUT and POST!", await response.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Theory]
        [InlineData("BANANA", "SPLIT")]
        public async Task CanReplaceBodyFromUri(string order, string product)
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/orders/{order}/{product}");

            Assert.Equal($"{{\"order\":\"{order}\", \"product\":\"{product}\"}}", await response.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Theory]
        [InlineData("CHIANTI", "RED", ""),
        InlineData("CHIANTI", "RED", "?text=2")]
        public async Task CanReplaceBodyFromUriWithUriParameters(string wine, string color, string extraQuery)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/wines/{wine}/{color}{extraQuery}");

            var body = await response.Content.ReadAsStringAsync();

            Assert.Contains($"\"wine\":\"{wine}\"", body);
            Assert.Contains($"\"color\":\"{color}\"", body);
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task CanReplaceBodySingleFieldFromSettings()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/replace/");

            Assert.Equal($"{{\"replace\":\"modified\"}}", await response.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task CanMockBodyDoesNotContainsResponse()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("api/estimate", new StringContent("foo"));

            Assert.Contains("BodyDoesNotContain works for PUT and POST!!", await response.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task LoadBodyFromRelativePath()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("api/fromfile", new StringContent(""));

            // note: to work on all os, you should specify body from file only in unix-compliant relative path
            // so : ./this/path/file.x and not like .\\this\\file.windows

            var content = await response.Content.ReadAsStringAsync();

            Assert.True(content == "Body loaded from file");
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task CanReturnErrorStatus()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("errors/502", new StringContent("foo"));
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(502, (int)response.StatusCode);
        }

        [Fact]
        public async Task MockInternalError_JsonErrorResponse()
        {
            var client = _factory.WithWebHostBuilder(h =>
            {
                h.Configure(app =>
                {
                    app.UseMiddleware<TestErrorMock>();
                });
            }).CreateClient();

            var response = await client.PostAsync("test", new StringContent("x"));
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("\"error\":\"System.Exception: error", content);
        }

        [Fact]
        public async Task KestralMock_works_with_Refit()
        {
            var client = _factory.CreateClient();

            var testApi = RestService.For<IKestralMockTestApi>(client);

            var helloWorld = await testApi.GetHelloWorldWorld();

            Assert.Contains("world", helloWorld.Hello);
        }

        [Theory]
        [InlineData("starts/with/but_does_not_match_verb", "NotFound", "")]
        [InlineData("starts/with/matches_put_method", "OK", "foo")]
        [InlineData("/test/1234/xyz", "NotFound", "")]
        [InlineData("/test/1234/xyz", "OK", "foo")]
        [InlineData("/hello/world", "NotFound", "")]
        [InlineData("/hello/world", "OK", "foo")]
        [InlineData("api/estimate", "OK", "00000")]
        [InlineData("api/estimate", "OK", "foo")]
        public async Task KestralMock_matches_using_verb(string url, string statusCode, string body)
        {
            var client = _factory.CreateClient();
            HttpResponseMessage response;
            if (string.IsNullOrWhiteSpace(body))
            {
                response = await client.DeleteAsync(url);
            }
            else
            {
                response = await client.PutAsync(url, new StringContent(body));
            }

            Assert.Equal(response.StatusCode.ToString(), statusCode);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var message = await response.Content.ReadAsStringAsync();
                Assert.Contains("put", message, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }

    public class TestErrorMock : MockService
    {
        public TestErrorMock(IOptions<MockConfiguration> options, RequestDelegate next) : base(options, next)
        {
        }

        protected override Task<bool> InvokeMock(HttpContext context)
        {
            throw new Exception("error");
        }
    }
}
