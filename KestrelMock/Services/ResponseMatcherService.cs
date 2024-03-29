﻿using KestrelMockServer.Domain;
using KestrelMockServer.Settings;

namespace KestrelMockServer.Services
{
    public class ResponseMatcherService : IResponseMatcherService
    {
        public Response FindMatchingResponseMock(string path, string body, string method, InputMappings mapping, Watcher watcher)
        {
            var observableResponse = FindMatchingObservableResponse(path, body, method, mapping);

            if (observableResponse?.Watch != null)
            {
                watcher.Log(path, body, method, observableResponse.Watch);
            }

            return observableResponse?.Response;
        }

        private ObservableResponse FindMatchingObservableResponse(string path, string body, string method, InputMappings mapping)
        {
            ObservableResponse result = null;

            var pathMappingKey = new PathMappingKey
            {
                Path = path,
                Method = method,
            };

            if (mapping.PathMapping.ContainsKey(pathMappingKey) && mapping.PathMapping[pathMappingKey].Request.Methods.Contains(method))
            {
                result = new ObservableResponse(mapping.PathMapping[pathMappingKey].Response, mapping.PathMapping[pathMappingKey].Watch);
            }

            if (result == null && mapping.PathStartsWithMapping != null)
            {
                foreach (var pathStart in mapping.PathStartsWithMapping)
                {
                    if (path.StartsWith(pathStart.Key.Path) && pathStart.Value.Request.Methods.Contains(method))
                    {
                        result = new ObservableResponse(pathStart.Value.Response, pathStart.Value.Watch);
                    }
                }
            }

            if (result == null && mapping.PathMatchesRegexMapping != null)
            {
                foreach (var pathRegex in mapping.PathMatchesRegexMapping)
                {
                    if (pathRegex.Key.Regex.IsMatch(path) && pathRegex.Value.Request.Methods.Contains(method))
                    {
                        result = new ObservableResponse(pathRegex.Value.Response, pathRegex.Value.Watch);
                    }
                }
            }

            if (result == null && mapping.BodyCheckMapping?.ContainsKey(pathMappingKey) == true)
            {
                var possibleResults = mapping.BodyCheckMapping[pathMappingKey];

                foreach (var possibleResult in possibleResults)
                {
                    if (!string.IsNullOrEmpty(possibleResult.Request.BodyContains))
                    {
                        if (body.Contains(possibleResult.Request.BodyContains) && possibleResult.Request.Methods.Contains(method))
                        {
                            result = new ObservableResponse(possibleResult.Response, possibleResult.Watch);
                        }
                    }
                    else if (!string.IsNullOrEmpty(possibleResult.Request.BodyDoesNotContain) && possibleResult.Request.Methods.Contains(method))
                    {
                        if (!body.Contains(possibleResult.Request.BodyDoesNotContain))
                        {
                            result = new ObservableResponse(possibleResult.Response, possibleResult.Watch);
                        }
                    }
                }
            }

            return result;
        }
    }
}
