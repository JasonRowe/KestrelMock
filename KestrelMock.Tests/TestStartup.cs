using KestrelMockServer;
using Microsoft.Extensions.Configuration;

namespace KestrelMock.Tests;

/// <summary>
/// Needed because test cannot access a separate assembly
/// </summary>
public class TestStartup : Startup
{
    public TestStartup(IConfiguration configuration) :
        base(configuration)
    {
    }
}