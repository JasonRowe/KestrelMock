using System.Collections.Generic;

namespace KestrelMock
{
	public class Response
	{
		public int Status { get; set; }

		public List<Dictionary<string, string>> Headers { get; set; }

		public string Body { get; set; }
	}
}