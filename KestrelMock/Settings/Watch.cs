using System;

namespace KestrelMockServer.Settings
{
    public class Watch
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int NumberOfRequests { get; set; } = 10;
    }
}
