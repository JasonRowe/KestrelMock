using KestrelMock.Settings;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KestrelMock.Domain
{
    public sealed class BodyCheckMapping : ConcurrentDictionary<string, List<HttpMockSetting>>
    {

    }

}
