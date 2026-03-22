using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace KestrelMockServer.Services
{
    public class TrafficHub : Hub
    {
        // Clients only receive, they don't invoke methods
    }
}
