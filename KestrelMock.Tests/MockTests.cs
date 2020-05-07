using System;
using KestrelMock.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace KestrelMock.Tests
{
    public class MockTests
	{
		private const string HTTP_TEST_HOST = "http://localhost:60000/";

		[Fact]
		public void CanStartup()
		{
			KestrelMock.Run(BuildConfiguration());
			Assert.True(HttpHelper.Get(HTTP_TEST_HOST).HttpStatusCode == System.Net.HttpStatusCode.OK);
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
		public void CanMockResponseUsingPathStartsWith()
		{
			KestrelMock.Run(new ConfigurationBuilder()
						.AddJsonFile("appsettings.json", optional: false)
						.Build());
			var response = HttpHelper.Get(HTTP_TEST_HOST + "starts/with/" + Guid.NewGuid());
			Assert.Contains("banana_x", response.Content);
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public void CanMockResponseUsingPathRegex_Matches()
		{
			KestrelMock.Run(new ConfigurationBuilder()
						.AddJsonFile("appsettings.json", optional: false)
						.Build());
			var response = HttpHelper.Get(HTTP_TEST_HOST + "/test/1234/xyz" + Guid.NewGuid());
			Assert.Contains("banana_x", response.Content);
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public void CanMockResponseUsingPathRegex_NoMatch()
		{
			KestrelMock.Run(new ConfigurationBuilder()
						.AddJsonFile("appsettings.json", optional: false)
						.Build());
			var response = HttpHelper.Get(HTTP_TEST_HOST + "/test/abcd/xyz" + Guid.NewGuid());
			Assert.Contains("banana_x", response.Content);
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public void CanMockGetResponseUsingExactPath()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Get(HTTP_TEST_HOST + "hello/world");
			Assert.Contains("hello", response.Content);
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public void CanMockPosttResponseUsingExactPath()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Post(HTTP_TEST_HOST + "hello/world");
			Assert.Contains("hello", response.Content);
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public void CanMockBodyContainsResponse()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Post(HTTP_TEST_HOST + "api/estimate", "00000");
			Assert.True(response.Content == "BodyContains Works!");
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public void CanMockBodyDoesNotContainsResponse()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Post(HTTP_TEST_HOST + "api/estimate", "foo");
			Assert.True(response.Content == "BodyDoesNotContain works!!");
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public void LoadBodyFromRelativePath()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Post(HTTP_TEST_HOST + "api/fromfile", "foo");
			Assert.True(response.Content == "Body loaded from file");
			Assert.Equal(200, (int)response.HttpStatusCode);
		}

		[Fact]
		public void CanReturnErrorStatus()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Post(HTTP_TEST_HOST + "errors/502", "foo");
			Assert.Equal(502, (int)response.HttpStatusCode);
		}

		private static IConfigurationRoot BuildConfiguration()
		{
			return new ConfigurationBuilder()
						.AddJsonFile("appsettings.json", optional: false)
						.Build();
		}

		private static IConfigurationRoot BuildBadConfiguration()
		{
			return new ConfigurationBuilder()
						.AddJsonFile("bad.appsettings.json", optional: false)
						.Build();
		}
	}
}
