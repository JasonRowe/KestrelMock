using System.Collections.Concurrent;

namespace KestrelMockServer.Settings
{
    public class MockConfiguration : ConcurrentDictionary<string, HttpMockSetting>
	{
	}
}