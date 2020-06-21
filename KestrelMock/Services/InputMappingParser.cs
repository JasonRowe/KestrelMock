using KestrelMock.Domain;
using KestrelMock.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KestrelMock.Services
{
    public static class InputMappingParser
    {
        public static async Task<InputMappings> ParsePathMappings(MockConfiguration httpMockSettings)
        {
            var inputMappings = new InputMappings();

            if (httpMockSettings == null || !httpMockSettings.Any())
            {
                return inputMappings;
            }

            foreach (var httpMockSetting in httpMockSettings)
            {
                if (!string.IsNullOrEmpty(httpMockSetting.Request.Path))
                {
                    if (!string.IsNullOrEmpty(httpMockSetting.Request.BodyContains) || !string.IsNullOrEmpty(httpMockSetting.Request.BodyDoesNotContain))
                    {
                        if (inputMappings.BodyCheckMapping.ContainsKey(httpMockSetting.Request.Path))
                        {
                            var bodyContainesList = inputMappings.BodyCheckMapping[httpMockSetting.Request.Path];
                            bodyContainesList.Add(httpMockSetting);
                        }
                        else
                        {
                            inputMappings.BodyCheckMapping.TryAdd(httpMockSetting.Request.Path, new List<HttpMockSetting> { httpMockSetting });
                        }
                    }
                    else
                    {
                        inputMappings.PathMapping.TryAdd(httpMockSetting.Request.Path, httpMockSetting);
                    }
                }
                else if (!string.IsNullOrEmpty(httpMockSetting.Request.PathStartsWith))
                {
                    inputMappings.PathStartsWithMapping.TryAdd(httpMockSetting.Request.PathStartsWith, httpMockSetting);
                }
                else if (!string.IsNullOrWhiteSpace(httpMockSetting.Request.PathMatchesRegex))
                {
                    var regex = new Regex(httpMockSetting.Request.PathMatchesRegex, RegexOptions.Compiled);
                    inputMappings.PathMatchesRegexMapping.TryAdd(regex, httpMockSetting);
                }
            }

            await LoadBodyFromFile(inputMappings);

            return inputMappings;
        }


        private static async Task LoadBodyFromFile(InputMappings mappings)
        {
            List<Task> toBeAwaited = new List<Task>();
            foreach (var mockSettings in mappings.BodyCheckMapping.Values)
            {
                foreach (var setting in mockSettings)
                {
                    toBeAwaited.Add(ReadBodyFromFile(setting));
                }
            }

            foreach (var mockPathSettings in mappings.PathMapping.Values)
            {
                toBeAwaited.Add(ReadBodyFromFile(mockPathSettings));
            }

            foreach (var mockStartsWithSettings in mappings.PathStartsWithMapping.Values)
            {
                toBeAwaited.Add(ReadBodyFromFile(mockStartsWithSettings));
            }

            foreach (var mockRegexSettings in mappings.PathMatchesRegexMapping.Values)
            {
                toBeAwaited.Add(ReadBodyFromFile(mockRegexSettings));
            }

            await Task.WhenAll(toBeAwaited);

        }

        private static async Task ReadBodyFromFile(HttpMockSetting setting)
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

    }

}
