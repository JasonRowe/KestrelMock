using System.Collections.Concurrent;
using KestrelMockServer.Settings;

namespace KestrelMockServer.Domain
{
    public sealed class BodyCheckMapping : ConcurrentDictionary<PathMappingKey, ConcurrentBag<HttpMockSetting>>
    {

    }

}
