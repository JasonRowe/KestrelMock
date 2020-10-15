using KestrelMockServer.Settings;
using System.Collections.Concurrent;

namespace KestrelMockServer.Domain
{
    public sealed class PathStartsWithMapping : ConcurrentDictionary<PathMappingKey, HttpMockSetting>
    {

    }

}
