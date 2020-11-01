using KestrelMockServer.Settings;

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
                
                var valueToReplace = matchesOnUri.ContainsKey(key) ? 
                    matchesOnUri[key] : 
                    replacement.Value;

                resultBody = resultBody.RegexBodyRewrite(replacement.Key, valueToReplace);
            }

            return resultBody;
        }
    }

}
