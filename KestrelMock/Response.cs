using System.Collections.Generic;
using System.Net;

namespace KestrelMock
{
    public class Response
	{
		public int Status { get; set; }

		public List<Dictionary<string, string>> Headers { get; set; }

		public string Body { get; set; }

		public string BodyFromFilePath { get; set; }

		public Replace Replace { get; set; }
	}
}