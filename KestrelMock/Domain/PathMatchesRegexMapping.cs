using KestrelMockServer.Settings;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace KestrelMockServer.Domain
{
    public sealed class PathMatchesRegexMapping : ConcurrentDictionary<PathMappingRegexKey, HttpMockSetting>
    {

    }

}
