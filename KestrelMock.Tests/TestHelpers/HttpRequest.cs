using System.Net.Http;
using System.Threading.Tasks;

namespace KestrelMockServer.Tests.TestHelpers
{
	public static class HttpHelper
	{
		private static HttpClient httpClient = new HttpClient();

		public static async Task<StatusCodeAndContent> GetAsync(string url)
		{
			using (var response = await httpClient.GetAsync(url))
			{
				var content = await response.Content.ReadAsStringAsync();
				return new StatusCodeAndContent
				{
					Content = content,
					HttpStatusCode = response.StatusCode,
				};
			}
		}

		public static async Task<StatusCodeAndContent> PostAsync(string url, string postContent = "")
		{
			using (var response = await httpClient.PostAsync(url, new StringContent(postContent)))
			{
				var content = await response.Content.ReadAsStringAsync();
				return new StatusCodeAndContent
				{
					Content = content,
					HttpStatusCode = response.StatusCode,
				};
			}
		}
	}
}
