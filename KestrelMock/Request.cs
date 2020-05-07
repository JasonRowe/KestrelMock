using System.Collections.Generic;

namespace KestrelMock
{
	public class Request
	{
		public List<string> Methods { get; set; }

		public string Path { get; set; }

		public string PathStartsWith { get; set; }

		public string BodyContains { get; set; }

		public string BodyDoesNotContain { get; set; }

		public string PathMatchesRegex { get; set; }
	}
}