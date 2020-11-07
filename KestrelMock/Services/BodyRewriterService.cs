using System.Text.RegularExpressions;

namespace KestrelMockServer.Services
{
    public static class BodyRewriterService
    {
        public static string RegexBodyRewrite(this string input, string propertyName, string replacement)
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
