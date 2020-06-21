using KestrelMock.Settings;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KestrelMock.Services
{
    public static class BodyReplacementService
    {
        private static readonly Regex UriTemplateParameterParser = new Regex(@"\{(?<parameter>[^{}?]*)\}", RegexOptions.Compiled);

        public static string RegexUriReplace(string path, string resultBody, KeyValuePair<string, string> keyVal)
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

        /// <summary>
        /// replacement using System.Text.Json... an alternative to regex, less flexible
        /// </summary>
        /// <param name="resultBody"></param>
        /// <param name="keyVal"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        private static string JsonWrite(string resultBody, string propertyName, string replacement)
        {
            var options = new JsonWriterOptions
            {
                Indented = false,
            };

            var documentOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip
            };
            //we assume it's json...
            // though with regex would be much more flexible...

            using var jsonDocument = JsonDocument.Parse(resultBody, documentOptions);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, options);
            JsonElement root = jsonDocument.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                writer.WriteStartObject();
            }
            else
            {
                return resultBody; //skip
            }

            foreach (JsonProperty property in root.EnumerateObject())
            {
                if (property.Name == propertyName)
                {
                    writer.WriteString(propertyName, replacement);
                }
                else
                {
                    property.WriteTo(writer);
                }
            }

            //var matchingProp = root.EnumerateObject().First(o => o.Name == propertyName);
            //matchingProp.WriteTo(writer);
            writer.WriteEndObject();
            writer.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static string UriPathReplacements(string path, Response matchResult, string resultBody)
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

        public static string RegexBodyRewrite(string input, string propertyName, string replacement)
        {
            var regex = $"\"{propertyName}\"\\s*:\\s*\"(?<value>.+?)\"";

            var finalReplacement = $"\"{propertyName}\":\"{replacement}\"";

            return Regex.Replace(input, regex, $"{finalReplacement}");
        }
    }

}
