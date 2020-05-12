using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using KestrelMock.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;
using Refit;
using Xunit;

namespace KestrelMock.Tests
{
	public class MockTests
	{
		private const string HTTP_TEST_HOST = "http://localhost:60001/";

		public MockTests()
		{
			KestrelMock.Run(BuildConfiguration(), new List<string> { HTTP_TEST_HOST });
		}

		[Fact]
		public async Task CanStartup()
		{
			var response = await HttpHelper.GetAsync(HTTP_TEST_HOST);
			Assert.True(response.HttpStatusCode == HttpStatusCode.NotFound);
		}

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

		[Fact]
		public async Task CanMockResponseUsingPathStartsWith()
		{
			var response = await HttpHelper.GetAsync(HTTP_TEST_HOST + "starts/with/" + Guid.NewGuid());
			Assert.Contains("banana_x", response.Content);
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public async Task CanMockResponseUsingPathRegex_Matches()
		{
			var response = await HttpHelper.GetAsync(HTTP_TEST_HOST + "/test/1234/xyz");
			Assert.Contains("regex_is_working", response.Content);
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public async Task CanMockResponseUsingPathRegex_NoMatch()
		{
			var response = await HttpHelper.GetAsync(HTTP_TEST_HOST + "/test/abcd/xyz");

			Assert.Equal(HttpStatusCode.NotFound, response.HttpStatusCode);
		}

		[Fact]
		public async Task CanMockGetResponseUsingExactPath()
		{
			var response = await HttpHelper.GetAsync(HTTP_TEST_HOST + "hello/world");
			Assert.Contains("hello", response.Content);
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public async Task CanMockPosttResponseUsingExactPath()
		{
			var response = await HttpHelper.PostAsync(HTTP_TEST_HOST + "hello/world");
			Assert.Contains("hello", response.Content);
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public async Task CanMockBodyContainsResponse()
		{
			var response = await HttpHelper.PostAsync(HTTP_TEST_HOST + "api/estimate", "00000");
			Assert.True(response.Content == "BodyContains Works!");
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public async Task CanMockBodyDoesNotContainsResponse()
		{
			var response = await HttpHelper.PostAsync(HTTP_TEST_HOST + "api/estimate", "foo");
			Assert.True(response.Content == "BodyDoesNotContain works!!");
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public async Task LoadBodyFromRelativePath()
		{
			var response = await HttpHelper.PostAsync(HTTP_TEST_HOST + "api/fromfile", "foo");
			Assert.True(response.Content == "Body loaded from file");
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public async Task CanReturnErrorStatus()
		{
			var response = await HttpHelper.PostAsync(HTTP_TEST_HOST + "errors/502", "foo");
			Assert.Equal(502, (int)response.HttpStatusCode);
		}

		[Fact]
		public async Task KestralMock_works_with_Refit()
		{
			var testApi = RestService.For<IKestralMockTestApi>(HTTP_TEST_HOST);
			var helloWorld = await testApi.GetHelloWorldWorld();
			Assert.Contains("world", helloWorld.Hello);
		}

		private static IConfigurationRoot BuildConfiguration()
		{
			return new ConfigurationBuilder()
						.AddJsonFile("appsettings.json", optional: false)
						.Build();
		}
	}
}
