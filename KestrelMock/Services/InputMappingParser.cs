using KestrelMockServer.Domain;
using KestrelMockServer.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KestrelMockServer.Services
{
    public class InputMappingParser : IInputMappingParser
    {
        private MockConfiguration mockConfiguration;

        public InputMappingParser(IOptions<MockConfiguration> mockConfiguration)
        {
            this.mockConfiguration = mockConfiguration.Value;
        }

        public async Task<InputMappings> ParsePathMappings()
        {
            var inputMappings = new InputMappings();

            if (mockConfiguration?.Any() != true)
            {
                return inputMappings;
            }

            foreach (var httpMockSetting in mockConfiguration)
            {
                if (!string.IsNullOrEmpty(httpMockSetting.Request.Path))
                {
                    if (!string.IsNullOrEmpty(httpMockSetting.Request.BodyContains)
                        || !string.IsNullOrEmpty(httpMockSetting.Request.BodyDoesNotContain))
                    {
                        foreach (var method in httpMockSetting.Request.Methods)
                        {
                            var key = new PathMappingKey
                            {
                                Path = httpMockSetting.Request.Path,
                                Method = method,
                            };

                            if (inputMappings.BodyCheckMapping.ContainsKey(key))
                            {
                                var bodyContainesList = inputMappings.BodyCheckMapping[key];
                                bodyContainesList.Add(httpMockSetting);
                            }
                            else
                            {
                                inputMappings.BodyCheckMapping.TryAdd(key, new List<HttpMockSetting> { httpMockSetting });
                            }
                        }
                    }
                    else
                    {
                        foreach (var method in httpMockSetting.Request.Methods)
                        {
                            var key = new PathMappingKey
                            {
                                Path = httpMockSetting.Request.Path,
                                Method = method
                            };

                            inputMappings.PathMapping.TryAdd(key, httpMockSetting);
                        }

                    }
                }
                else if (!string.IsNullOrEmpty(httpMockSetting.Request.PathStartsWith))
                {
                    foreach (var method in httpMockSetting.Request.Methods)
                    {
                        var key = new PathMappingKey
                        {
                            Path = httpMockSetting.Request.PathStartsWith,
                            Method = method
                        };

                        inputMappings.PathStartsWithMapping.TryAdd(key, httpMockSetting);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(httpMockSetting.Request.PathMatchesRegex))
                {
                    foreach (var method in httpMockSetting.Request.Methods)
                    {
                        var key = new PathMappingRegexKey
                        {
                            Regex = new Regex(httpMockSetting.Request.PathMatchesRegex, RegexOptions.Compiled),
                            Method = method,
                        };

                        inputMappings.PathMatchesRegexMapping.TryAdd(key, httpMockSetting);
                    }
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
                    using var reader = File.OpenText(setting.Response.BodyFromFilePath);

                    var bodyFromFile = await reader.ReadToEndAsync();

                    setting.Response.Body = bodyFromFile;
                }
                else
                {
                    throw new Exception($"Path in BodyFromFilePath not found: {setting.Response.BodyFromFilePath}");
                }
            }
        }

    }

}
