using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KestrelMock.Services
{
    public class MockService
    {
        private ConcurrentDictionary<string, HttpMockSetting> _pathMappings;
        private ConcurrentDictionary<string, HttpMockSetting> _pathStartsWithMappings;
        private ConcurrentDictionary<string, List<HttpMockSetting>> _bodyCheckMappings;
        private ConcurrentDictionary<Regex, HttpMockSetting> _pathMatchesRegex;
        private readonly MockConfiguration _mockConfiguration;
        private readonly RequestDelegate _next;

        public MockService(IOptions<MockConfiguration> options, RequestDelegate next)
        {
            _pathMappings = new ConcurrentDictionary<string, HttpMockSetting>();
            _pathStartsWithMappings = new ConcurrentDictionary<string, HttpMockSetting>();
            _bodyCheckMappings = new ConcurrentDictionary<string, List<HttpMockSetting>>();
            _pathMatchesRegex = new ConcurrentDictionary<Regex, HttpMockSetting>();
            _mockConfiguration = options.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_pathMappings.IsEmpty && _pathStartsWithMappings.IsEmpty && _bodyCheckMappings.IsEmpty)
            {
                SetupPathMappings(_mockConfiguration);
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

                    if (matchResult.ReplaceDictionary?.Any() == true)
                    {
                        
                        foreach(var keyVal in matchResult.ReplaceDictionary)
                        {
                            var pathRegexMatch = Regex.Match(path, keyVal.Value);

                            if (pathRegexMatch.Success)
                            {
                                var regex = "\":\\s*\"(?<field>[\\w\\d]+)\"}";
                                var relaceRegex = new Regex(keyVal.Key + regex, RegexOptions.Compiled);

                                var replacement = pathRegexMatch.Groups.Count == 2 ?
                                    pathRegexMatch.Groups[1].Value : pathRegexMatch.Value;

                                if (!relaceRegex.IsMatch(matchResult.Body))
                                {
                                    throw new Exception("no match");
                                }

                                resultBody = relaceRegex.ReplaceGroup(matchResult.Body, "field", replacement);
                            }
                        }
                    }

                    await context.Response.WriteAsync(resultBody);
                }
                
            }

            //breakes execution
            //await _next(context);
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

        private void SetupPathMappings(MockConfiguration httpMockSettings)
        {
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
                        _pathMappings.TryAdd(httpMockSetting.Request.Path, httpMockSetting);
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

    public static class RegexExtensions
    {
        public static string ReplaceGroup(
            this Regex regex, string input, string groupName, string replacement)
        {
            return regex.Replace(
                input,
                m =>
                {
                    var group = m.Groups[groupName];
                    var sb = new StringBuilder();
                    var previousCaptureEnd = 0;
                    foreach (var capture in group.Captures.Cast<Capture>())
                    {
                        var currentCaptureEnd =
                            capture.Index + capture.Length - m.Index;
                        var currentCaptureLength =
                            capture.Index - m.Index - previousCaptureEnd;
                        sb.Append(
                            m.Value.Substring(
                                previousCaptureEnd, currentCaptureLength));
                        sb.Append(replacement);
                        previousCaptureEnd = currentCaptureEnd;
                    }
                    sb.Append(m.Value.Substring(previousCaptureEnd));

                    return sb.ToString();
                });
        }
    }

}
