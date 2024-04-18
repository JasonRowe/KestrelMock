using System;

namespace KestrelMockServer.Settings
{
    public class HttpMockSetting
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public Request Request { get; set; }

        public Response Response { get; set; }

        public Watch Watch { get; set; }
    }
}