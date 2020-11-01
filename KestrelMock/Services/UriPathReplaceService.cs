using KestrelMockServer.Settings;
using System.Text.RegularExpressions;

namespace KestrelMockServer.Services
{
    public class UriPathReplaceService : IUriPathReplaceService
    {
        public string UriPathReplacements(string path, Response matchResult, string resultBody)
        {
            var matchesOnUri = matchResult.Replace.UriTemplate.Parse(path);

            foreach (var keyVal in matchResult.Replace.UriPathReplacements)
            {
                var valueToReplace = matchesOnUri.ContainsKey(keyVal.Value) ? 
                    matchesOnUri[keyVal.Value] : 
                    keyVal.Value;

                resultBody = RegexBodyRewrite(resultBody, keyVal.Key, valueToReplace);
            }

            return resultBody;
        }

        private static string RegexBodyRewrite(string input, string propertyName, string replacement)
        {
            var regex = $"\"{propertyName}\"\\s*:\\s*(?<value>\".+?\")";
            var finalReplacement = $"\"{propertyName}\":\"{replacement}\"";
            var resultBody = Regex.Replace(input, regex, $"{finalReplacement}", RegexOptions.Multiline);

            //replaces numbers
            var numbersRegex = $"\"{propertyName}\"\\s*:\\s*(?<value>\\d+(.\\d+)?)";
            var numberFinalReplacement = $"\"{propertyName}\":{replacement}";
            var finalBody = Regex.Replace(resultBody, numbersRegex, $"{numberFinalReplacement}", RegexOptions.Multiline);

            return finalBody;
        }
    }

}
