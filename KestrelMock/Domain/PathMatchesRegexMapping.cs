using KestrelMock.Settings;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace KestrelMock.Domain
{
    public sealed class PathMatchesRegexMapping : ConcurrentDictionary<PathMappingRegexKey, HttpMockSetting>
    {

    }

}
