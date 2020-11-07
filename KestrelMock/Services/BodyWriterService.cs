using KestrelMockServer.Settings;
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
                    resultBody = resultBody.RegexBodyRewrite(keyVal.Key, keyVal.Value);
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

                resultBody = resultBody.RegexBodyRewrite(keyVal.Key, replacement);
            }

            return resultBody;
        }
    }

}
