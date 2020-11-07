using KestrelMockServer.Settings;
using System.Collections.Generic;

namespace KestrelMockServer.Services
{
    public class UriPathReplaceService : IUriPathReplaceService
    {
        public string UriPathReplacements(string path, Response matchResult, string resultBody)
        {
            var matchesOnUri = new UriTemplate(matchResult.Replace.UriTemplate).Parse(path);

            foreach (var replacement in matchResult.Replace.UriPathReplacements)
            {
                var key = replacement.Value
                    .Replace("{", string.Empty)
                    .Replace("}", string.Empty);

                var valueToReplace =
                    HasMatch(matchesOnUri, key) ?
                    matchesOnUri[key] : replacement.Value;

                if (valueToReplace == ($"{{{key}}}"))
                {
                    // no match for uri path or string
                    continue;
                }

                resultBody = resultBody.RegexBodyRewrite(replacement.Key, valueToReplace);
            }

            return resultBody;
        }

        private static bool HasMatch(IDictionary<string, string> matchesOnUri, string key)
        {
            return matchesOnUri.ContainsKey(key) && !string.IsNullOrWhiteSpace(matchesOnUri[key]);
        }
    }

}
