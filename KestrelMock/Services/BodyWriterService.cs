using KestrelMockServer.Settings;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KestrelMockServer.Services
{
    public class BodyWriterService : IBodyWriterService
    {
        private readonly IUriPathReplaceService uriPathReplaceService;

        public BodyWriterService(IUriPathReplaceService uriPathReplaceService)
        {
            this.uriPathReplaceService = uriPathReplaceService;
        }

        public string UpdateBody(string path, Response matchResult, string resultBody)
        {
            if (matchResult.Replace.RegexUriReplacements?.Any() == true)
            {
                foreach (var keyVal in matchResult.Replace.RegexUriReplacements)
                {
                    resultBody = BodyWriterService.RegexUriReplace(path, resultBody, keyVal);
                }
            }

            if (matchResult.Replace.BodyReplacements?.Any() == true)
            {
                foreach (var keyVal in matchResult.Replace.BodyReplacements)
                {
                    resultBody = BodyWriterService.RegexBodyRewrite(resultBody, keyVal.Key, keyVal.Value);
                }
            }

            if (matchResult.Replace.UriPathReplacements?.Any() == true
                && matchResult.Replace.UriTemplate != null)
            {
                resultBody = uriPathReplaceService.UriPathReplacements(path, matchResult, resultBody);
            }

            return resultBody;
        }

        private static string RegexUriReplace(string path, string resultBody, KeyValuePair<string, string> keyVal)
        {
            var pathRegexMatch = Regex.Match(path, keyVal.Value);

            if (pathRegexMatch.Success)
            {
                var replacement = pathRegexMatch.Groups.Count == 2 ?
                    pathRegexMatch.Groups[1].Value : pathRegexMatch.Value;

                resultBody = RegexBodyRewrite(resultBody, keyVal.Key, replacement);
            }

            return resultBody;
        }

        private static string RegexBodyRewrite(string input, string propertyName, string replacement)
        {
            var regex = $"\"{propertyName}\"\\s*:\\s*\"(?<value>.+?)\"";

            var finalReplacement = $"\"{propertyName}\":\"{replacement}\"";

            return Regex.Replace(input, regex, $"{finalReplacement}");
        }
    }

}
