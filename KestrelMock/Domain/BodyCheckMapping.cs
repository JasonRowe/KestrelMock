using KestrelMockServer.Settings;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KestrelMockServer.Domain
{
    public sealed class BodyCheckMapping : ConcurrentDictionary<PathMappingKey, List<HttpMockSetting>>
    {

    }

}
