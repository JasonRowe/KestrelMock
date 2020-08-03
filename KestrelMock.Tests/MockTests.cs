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

            Assert.Contains("BodyContains Works!", await response.Content.ReadAsStringAsync());
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

            Assert.Contains("BodyDoesNotContain works!", await response.Content.ReadAsStringAsync());
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
