using System;
using System.Collections.Generic;

namespace KestrelMock.BlazorUI.Models
{
    public class HttpMockSetting
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Request? Request { get; set; }
        public Response? Response { get; set; }
        public Watch? Watch { get; set; }
    }

    public class Request
    {
        public List<string>? Methods { get; set; }
        public string? Path { get; set; }
        public string? PathStartsWith { get; set; }
        public string? PathMatchesRegex { get; set; }
        
        // Helper to get a displayable path for the UI
        public string GetDisplayPath()
        {
            if (!string.IsNullOrEmpty(Path)) return Path;
            if (!string.IsNullOrEmpty(PathStartsWith)) return PathStartsWith + "*";
            if (!string.IsNullOrEmpty(PathMatchesRegex)) return "Regex: " + PathMatchesRegex;
            return "Unknown Path";
        }
        
        // Helper to get a display method
        public string GetDisplayMethod()
        {
            if (Methods != null && Methods.Count > 0) return string.Join(", ", Methods);
            return "ANY";
        }
    }

    public class Response
    {
        public int Status { get; set; }
        public string? Body { get; set; }
    }

    public class Watch
    {
        public Guid Id { get; set; }
    }
}
