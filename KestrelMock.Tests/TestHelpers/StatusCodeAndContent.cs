using System.Net;

namespace KestrelMockServer.Tests.TestHelpers
{
	public class StatusCodeAndContent
	{
		public HttpStatusCode HttpStatusCode { get; set; }

		public string Content { get; set; }
	}
}
