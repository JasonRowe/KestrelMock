﻿using System.Collections.Generic;

namespace KestrelMockServer.Settings
{
    public class Replace
    {
		public string UriTemplate { get; set; }
		public Dictionary<string, string> BodyReplacements { get; set; }
		public Dictionary<string, string> UriPathReplacements { get; set; }
		public Dictionary<string, string> RegexUriReplacements { get; set; }
		public ContentType ContentType { get; set; }
	}
}