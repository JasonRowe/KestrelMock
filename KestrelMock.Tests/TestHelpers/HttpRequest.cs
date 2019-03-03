using System.Net.Http;

namespace KestrelMock.Tests.TestHelpers
{
	public static class HttpHelper
	{
		public static StatusCodeAndContent Get(string url)
		{
			var httpClient = new HttpClient();
			var response = httpClient.GetAsync(url).Result;
			var content = response.Content.ReadAsStringAsync().Result;
			return new StatusCodeAndContent
			{
				Content = content,
				HttpStatusCode = response.StatusCode,
			};
		}

		public static StatusCodeAndContent Post(string url, string postContent = "")
		{
			var httpClient = new HttpClient();
			var response = httpClient.PostAsync(url, new StringContent(postContent)).Result;
			var content = response.Content.ReadAsStringAsync().Result;
			return new StatusCodeAndContent
			{
				Content = content,
				HttpStatusCode = response.StatusCode,
			};
		}
	}
}
