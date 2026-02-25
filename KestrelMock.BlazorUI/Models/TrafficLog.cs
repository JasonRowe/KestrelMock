using System;

namespace KestrelMock.BlazorUI.Models
{
    /// <summary>
    /// Represents a single live traffic entry received from the KestrelMock SignalR hub.
    /// Shape must match the server-side <c>WatchLog</c> that is broadcast via <c>ReceiveTraffic</c>.
    /// </summary>
    public class TrafficLog
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int StatusCode { get; set; } = 200;
    }
}
