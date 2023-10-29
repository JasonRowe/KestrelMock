using KestrelMockServer.Settings;
using System.Collections.Concurrent;

namespace KestrelMockServer.Domain
{
    public sealed class PathMapping : ConcurrentDictionary<PathMappingKey, HttpMockSetting>
    {

    }
}