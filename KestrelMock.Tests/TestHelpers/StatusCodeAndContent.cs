using System.Net;

namespace KestrelMock.Tests.TestHelpers
{
	public class StatusCodeAndContent
	{
		public HttpStatusCode HttpStatusCode { get; set; }

		public string Content { get; set; }
	}
}
