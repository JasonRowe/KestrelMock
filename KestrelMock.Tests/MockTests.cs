using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KestrelMock.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
		}

		[Fact]
		public void CanMockGetResponseUsingExactPath()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Get(HTTP_TEST_HOST + "hello/world");
			Assert.Contains("hello", response.Content);
		}

		[Fact]
		public void CanMockPosttResponseUsingExactPath()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Post(HTTP_TEST_HOST + "hello/world");
			Assert.Contains("hello", response.Content);
		}

		[Fact]
		public void CanMockBodyContainsResponse()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Post(HTTP_TEST_HOST + "api/estimate", "00000");
			Assert.True(response.Content == "BodyContains Works!");
		}

		[Fact]
		public void CanMockBodyDoesNotContainsResponse()
		{
			KestrelMock.Run(BuildConfiguration());
			var response = HttpHelper.Post(HTTP_TEST_HOST + "api/estimate", "foo");
			Assert.True(response.Content == "BodyDoesNotContain works!!");
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
