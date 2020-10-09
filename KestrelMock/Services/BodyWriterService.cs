using KestrelMock.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KestrelMock.Services
{
    public class BodyWriterService : IBodyWriterService
    {
        private static readonly Regex UriTemplateParameterParser = new Regex(@"\{(?<parameter>[^{}?]*)\}", RegexOptions.Compiled);

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
                && !string.IsNullOrWhiteSpace(matchResult.Replace.UriTemplate))
            {
                resultBody = BodyWriterService.UriPathReplacements(path, matchResult, resultBody);
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

        private static string UriPathReplacements(string path, Response matchResult, string resultBody)
        {
            string parameterRegexString = matchResult.Replace.UriTemplate.Replace("/", @"\/");

            foreach (Match match in UriTemplateParameterParser.Matches(matchResult.Replace.UriTemplate))
            {
                parameterRegexString = parameterRegexString
                            .Replace(match.Value, $"(?<{match.Groups["parameter"].Value}>[^{{}}?]*)");
            }

            parameterRegexString = $"{parameterRegexString}/??.*";

            var matchesOnUri = Regex.Match(path, parameterRegexString);

            foreach (var keyVal in matchResult.Replace.UriPathReplacements)
            {
                if (UriTemplateParameterParser.IsMatch(keyVal.Value))
                {

                    var parameterToReplace = UriTemplateParameterParser.Match(keyVal.Value)
                        .Groups["parameter"].Value;

                    if (matchesOnUri.Groups[parameterToReplace] != null)
                    {
                        var valueToReplace = matchesOnUri.Groups[parameterToReplace].Value;
                        resultBody = RegexBodyRewrite(resultBody, keyVal.Key, valueToReplace);
                    }
                }
                else
                {
                    resultBody = RegexBodyRewrite(resultBody, keyVal.Key, keyVal.Value);
                }
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
