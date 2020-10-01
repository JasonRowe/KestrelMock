using KestrelMockServer.Domain;

namespace KestrelMockServer.Settings
{
	public class HttpMockSetting
	{
		public Request Request { get; set; }

		public Response Response { get; set; }
	}
}