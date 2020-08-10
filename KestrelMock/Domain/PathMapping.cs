using KestrelMock.Settings;
using System.Collections.Concurrent;

namespace KestrelMock.Domain
{
    public sealed class PathMapping : ConcurrentDictionary<PathMappingKey, HttpMockSetting>
    {

    }

}
