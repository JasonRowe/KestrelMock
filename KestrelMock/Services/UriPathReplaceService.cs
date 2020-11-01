using KestrelMockServer.Settings;

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

                resultBody = resultBody.RegexBodyRewrite(keyVal.Key, valueToReplace);
            }

            return resultBody;
        }
    }

}
