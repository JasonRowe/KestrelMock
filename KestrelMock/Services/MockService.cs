using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KestrelMock.Services
{

    public sealed class PathMapping : ConcurrentDictionary<string, HttpMockSetting>
    {

    }

    public sealed class PathStartsWithMapping : ConcurrentDictionary<string, HttpMockSetting>
    {

    }

    public sealed class BodyCheckMapping : ConcurrentDictionary<string, List<HttpMockSetting>>
    {

    }

    public sealed class PathMatchesRegexMapping : ConcurrentDictionary<Regex, HttpMockSetting>
    {

    }

    public class InputMappings
    {
        public PathMapping PathMapping { get; set; }
        public PathStartsWithMapping PathStartsWithMapping { get; set; }
        public BodyCheckMapping BodyCheckMapping { get; set; }
        public PathMatchesRegexMapping PathMatchesRegexMapping { get; set; }
    }


    public class MockService
    {
        private ConcurrentDictionary<string, HttpMockSetting> _pathStartsWithMappings;
        private ConcurrentDictionary<string, List<HttpMockSetting>> _bodyCheckMappings;
        private ConcurrentDictionary<Regex, HttpMockSetting> _pathMatchesRegex;

        private static readonly Regex UriTemplateParameterParser = new Regex(@"\{(?<parameter>[^{}?]*)\}", RegexOptions.Compiled);

        private readonly MockConfiguration _mockConfiguration;
        private readonly RequestDelegate _next;

        public MockService(IOptions<MockConfiguration> options, RequestDelegate next)
        {
            _pathStartsWithMappings = new ConcurrentDictionary<string, HttpMockSetting>();
            _bodyCheckMappings = new ConcurrentDictionary<string, List<HttpMockSetting>>();
            _pathMatchesRegex = new ConcurrentDictionary<Regex, HttpMockSetting>();
            _mockConfiguration = options.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var pathMappings = new ConcurrentDictionary<string, HttpMockSetting>();

            if (pathMappings.IsEmpty && _pathStartsWithMappings.IsEmpty && _bodyCheckMappings.IsEmpty)
            {
                ParsePathMappings(_mockConfiguration);
                await LoadBodyFromFile();
            }

            var path = context.Request.Path + context.Request.QueryString.ToString();
            string body = null;

            if (context.Request.Body != null)
            {
                using (StreamReader reader = new StreamReader(context.Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
            }

            var matchResult = FindMatchingResponseMock(path, body);

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

        private async Task LoadBodyFromFile()
        {
            List<Task> toBeAwaited = new List<Task>();
            foreach (var mockSettings in _bodyCheckMappings.Values)
            {
                foreach (var setting in mockSettings)
                {
                    toBeAwaited.Add(ReadBodyFromFile(setting));
                }
            }

            foreach (var mockPathSettings in _pathMappings.Values)
            {
                toBeAwaited.Add(ReadBodyFromFile(mockPathSettings));
            }

            foreach (var mockStartsWithSettings in _pathStartsWithMappings.Values)
            {
                toBeAwaited.Add(ReadBodyFromFile(mockStartsWithSettings));
            }

            foreach (var mockRegexSettings in _pathMatchesRegex.Values)
            {
                toBeAwaited.Add(ReadBodyFromFile(mockRegexSettings));
            }

            await Task.WhenAll(toBeAwaited);

        }

        private async Task ReadBodyFromFile(HttpMockSetting setting)
        {
            if (!string.IsNullOrEmpty(setting.Response.BodyFromFilePath) && string.IsNullOrEmpty(setting.Response.Body))
            {
                if (File.Exists(setting.Response.BodyFromFilePath))
                {
                    using (var reader = File.OpenText(setting.Response.BodyFromFilePath))
                    {
                        var bodyFromFile = await reader.ReadToEndAsync();
                        setting.Response.Body = bodyFromFile;
                    }
                }
                else
                {
                    throw new Exception($"Path in BodyFromFilePath not found {setting.Response.BodyFromFilePath}");
                }
            }
        }

        private Response FindMatchingResponseMock(string path, string body)
        {
            Response result = null;

            if (_pathMappings.ContainsKey(path))
            {
                result = _pathMappings[path].Response;
            }

            if (result == null && _pathStartsWithMappings != null)
            {
                foreach (var pathStart in _pathStartsWithMappings)
                {
                    if (path.StartsWith(pathStart.Key))
                    {
                        result = pathStart.Value.Response;
                    }
                }
            }

            if (result == null && _pathMatchesRegex != null)
            {
                foreach (var pathRegex in _pathMatchesRegex)
                {
                    if (pathRegex.Key.IsMatch(path))
                    {
                        result = pathRegex.Value.Response;
                    }
                }
            }

            if (result == null && _bodyCheckMappings != null && _bodyCheckMappings.ContainsKey(path))
            {
                var possibleResults = _bodyCheckMappings[path];

                foreach (var possibleResult in possibleResults)
                {
                    if (!string.IsNullOrEmpty(possibleResult.Request.BodyContains))
                    {
                        if (body.Contains(possibleResult.Request.BodyContains))
                        {
                            result = possibleResult.Response;
                        }
                    }
                    else if (!string.IsNullOrEmpty(possibleResult.Request.BodyDoesNotContain))
                    {
                        if (!body.Contains(possibleResult.Request.BodyDoesNotContain))
                        {
                            result = possibleResult.Response;
                        }
                    }
                }
            }

            return result;
        }

        private void ParsePathMappings(MockConfiguration httpMockSettings)
        {
            var pathMappings = new ConcurrentDictionary<string, HttpMockSetting>();

            if (httpMockSettings == null || !httpMockSettings.Any())
            {
                return;
            }

            foreach (var httpMockSetting in httpMockSettings)
            {
                if (!string.IsNullOrEmpty(httpMockSetting.Request.Path))
                {
                    if (!string.IsNullOrEmpty(httpMockSetting.Request.BodyContains) || !string.IsNullOrEmpty(httpMockSetting.Request.BodyDoesNotContain))
                    {
                        if (_bodyCheckMappings.ContainsKey(httpMockSetting.Request.Path))
                        {
                            var bodyContainesList = _bodyCheckMappings[httpMockSetting.Request.Path];
                            bodyContainesList.Add(httpMockSetting);
                        }
                        else
                        {
                            _bodyCheckMappings.TryAdd(httpMockSetting.Request.Path, new List<HttpMockSetting> { httpMockSetting });
                        }
                    }
                    else
                    {
                        pathMappings.TryAdd(httpMockSetting.Request.Path, httpMockSetting);
                    }
                }
                else if (!string.IsNullOrEmpty(httpMockSetting.Request.PathStartsWith))
                {
                    _pathStartsWithMappings.TryAdd(httpMockSetting.Request.PathStartsWith, httpMockSetting);
                }
                else if (!string.IsNullOrWhiteSpace(httpMockSetting.Request.PathMatchesRegex))
                {
                    var regex = new Regex(httpMockSetting.Request.PathMatchesRegex, RegexOptions.Compiled);
                    _pathMatchesRegex.TryAdd(regex, httpMockSetting);
                }
            }
        }
    }

}
