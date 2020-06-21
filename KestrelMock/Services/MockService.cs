using KestrelMock.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KestrelMock.Services
{
    public class MockService
    {
        private static readonly Regex UriTemplateParameterParser = new Regex(@"\{(?<parameter>[^{}?]*)\}", RegexOptions.Compiled);
        private readonly MockConfiguration _mockConfiguration;
        private readonly RequestDelegate _next;

        public MockService(IOptions<MockConfiguration> options, RequestDelegate next)
        {
            _mockConfiguration = options.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var mappings = await InputMappingParser.ParsePathMappings(_mockConfiguration);


            var path = context.Request.Path + context.Request.QueryString.ToString();
            string body = null;

            if (context.Request.Body != null)
            {
                using (StreamReader reader = new StreamReader(context.Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
            }

            var matchResult = ResponseMatcher.FindMatchingResponseMock(path, body, mappings);

            if (matchResult is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            if (matchResult != null)
            {
                if (matchResult.Headers != null || !matchResult.Headers.Any())
                {
                    foreach (var header in matchResult.Headers)
                    {
                        foreach (var key in header.Keys)
                        {
                            context.Response.Headers.Add(key, header[key]);
                        }
                    }
                }

                context.Response.StatusCode = matchResult.Status;

                if (!string.IsNullOrWhiteSpace(matchResult.Body))
                {
                    string resultBody = matchResult.Body;

                    if (matchResult.Replace is null)
                    {
                        await context.Response.WriteAsync(resultBody);
                        return;
                    }

                    if (matchResult.Replace.RegexUriReplacements?.Any() == true)
                    {
                        var options = new JsonWriterOptions
                        {
                            Indented = false,
                        };

                        var documentOptions = new JsonDocumentOptions
                        {
                            CommentHandling = JsonCommentHandling.Skip
                        };

                        foreach (var keyVal in matchResult.Replace.RegexUriReplacements)
                        {
                            var pathRegexMatch = Regex.Match(path, keyVal.Value);

                            if (pathRegexMatch.Success)
                            {
                                var regex = $"{{.*\"{keyVal.Key}\":\\s*\"(?<field>[^/.]+)\".*}}";
                                var relaceRegex = new Regex(regex, RegexOptions.Compiled);

                                var replacement = pathRegexMatch.Groups.Count == 2 ?
                                    pathRegexMatch.Groups[1].Value : pathRegexMatch.Value;


                                if (!relaceRegex.IsMatch(matchResult.Body))
                                {
                                    throw new Exception("no match");
                                }

                                //we assume it's json...
                                // though with regex would be much more flexible...

                                using JsonDocument jsonDocument = JsonDocument.Parse(resultBody, documentOptions);
                                using var stream = new MemoryStream();
                                using var writer = new Utf8JsonWriter(stream, options);

                                JsonElement root = jsonDocument.RootElement;

                                if (root.ValueKind == JsonValueKind.Object)
                                {
                                    writer.WriteStartObject();
                                }
                                else
                                {
                                    return;
                                }

                                foreach (JsonProperty property in root.EnumerateObject())
                                {
                                    if (property.Name == keyVal.Key)
                                    {
                                        writer.WriteString(keyVal.Key, replacement);
                                    }
                                    else
                                    {
                                        property.WriteTo(writer);
                                    }
                                }

                                //var matchingProp = root.EnumerateObject().First(o => o.Name == keyVal.Key);
                                //matchingProp.WriteTo(writer);
                                writer.WriteEndObject();
                                writer.Flush();

                                string finalJson = Encoding.UTF8.GetString(stream.ToArray());
                                resultBody = finalJson;

                                //var match = relaceRegex.Match(matchResult.Body).Groups["field"].Value;
                                //resultBody = resultBody.Replace(match, replacement);
                                //relaceRegex.ReplaceGroup(matchResult.Body, "field", replacement);
                            }
                        }
                    }


                    if (matchResult.Replace.BodyReplacements?.Any() == true)
                    {
                        foreach (var keyVal in matchResult.Replace.BodyReplacements)
                        {
                            resultBody = RegexBodyRewrite(resultBody, keyVal.Key, keyVal.Value);
                        }
                    }

                    if (matchResult.Replace.UriPathReplacements?.Any() == true
                        && !String.IsNullOrWhiteSpace(matchResult.Replace.UriTemplate))
                    {
                        resultBody = UriPathReplacements(path, matchResult, resultBody);
                    }

                    await context.Response.WriteAsync(resultBody);
                }
            }

            //breakes execution
            //await _next(context);
        }

        private string UriPathReplacements(string path, Response matchResult, string resultBody)
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

        private string RegexBodyRewrite(string input, string propertyName, string replacement)
        {
            var regex = $"\"{propertyName}\"\\s*:\\s*\"(?<value>.+?)\"";

            var finalReplacement = $"\"{propertyName}\":\"{replacement}\"";

            return Regex.Replace(input, regex, $"{finalReplacement}");
        }


        
    }

}
