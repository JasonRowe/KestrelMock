using System.Linq;
using KestrelMockServer.Domain;
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
                var pathMapping = mapping.PathMapping[pathMappingKey];

                result = new ObservableResponse(pathMapping.Response, pathMapping.Watch);
            }

            if (result == null && mapping.PathStartsWithMapping != null)
            {
                // this gets us closer to a match if we've mocked similar paths, but there may be matches that aren't quite there...
                // so we find all the mocks where path starts with the defined pathstartswith and try to match the longest url first
                var pathStartsWithList = mapping.PathStartsWithMapping
                    .Where(p => path.StartsWith(p.Key.PathStartsWith))
                    .OrderByDescending(p => p.Key.PathStartsWith.Length);

                foreach (var pathStart in pathStartsWithList)
                {
                    foreach (var mockSetting in pathStart.Value)
                    {
                        if (path.StartsWith(mockSetting.Request.PathStartsWith) && mockSetting.Request.Methods.Contains(method))
                        {
							result = CheckBodyMapping(body, method, mockSetting);

                            if (result == null)
                            {
                                result = CheckEmptyBodyMapping(body, mockSetting);
                            }

                            if (result != null)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            if (result == null && mapping.PathMatchesRegexMapping != null)
            {
                foreach (var pathRegex in mapping.PathMatchesRegexMapping)
                {
                    if (pathRegex.Key.Regex.IsMatch(path) && pathRegex.Value.Request.Methods.Contains(method))
                    {
                        result = CheckBodyMapping(body, method, pathRegex.Value);

                        if (result == null)
                        {
                            result = new ObservableResponse(pathRegex.Value.Response, pathRegex.Value.Watch);
                        }
                    }
                }
            }

            if (result == null && mapping.BodyCheckMapping?.ContainsKey(pathMappingKey) == true)
            {
                var possibleResults = mapping.BodyCheckMapping[pathMappingKey];

                foreach (var possibleResult in possibleResults)
                {
                    result = CheckBodyMapping(body, method, possibleResult);

                    // We found it, don't need to iterate anymore
                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        private static ObservableResponse CheckBodyMapping(string body, string method, HttpMockSetting possibleResult)
        {
            if (!string.IsNullOrEmpty(possibleResult.Request.BodyContains))
            {
                if (body.Contains(possibleResult.Request.BodyContains) && possibleResult.Request.Methods.Contains(method))
                {
                    return new ObservableResponse(possibleResult.Response, possibleResult.Watch);
                }
            }
            else if (!string.IsNullOrEmpty(possibleResult.Request.BodyDoesNotContain) && possibleResult.Request.Methods.Contains(method))
            {
                if (!body.Contains(possibleResult.Request.BodyDoesNotContain))
                {
                    return new ObservableResponse(possibleResult.Response, possibleResult.Watch);
                }
            }
            else if ((possibleResult.Request.BodyContainsArray?.Any() ?? false) && possibleResult.Request.Methods.Contains(method))
            {
                if (possibleResult.Request.BodyContainsArray.All(b => body.Contains(b)))
                {
                    return new ObservableResponse(possibleResult.Response, possibleResult.Watch);
                }
            }

            return null;
        }

        private static ObservableResponse CheckEmptyBodyMapping(string body, HttpMockSetting possibleResult)
        {
            // if we don't have a body to look at, we just match whatever is sent in because it's just url matching at this point
            if (string.IsNullOrEmpty(body))
            {
                return new ObservableResponse(possibleResult.Response, possibleResult.Watch);
            }
            // if body is present on request, but we haven't configured a body matcher, match what was sent in
            else if (!string.IsNullOrEmpty(body) &&
                string.IsNullOrEmpty(possibleResult.Request.BodyContains) &&
                string.IsNullOrEmpty(possibleResult.Request.BodyDoesNotContain) &&
                !(possibleResult.Request.BodyContainsArray?.Any() ?? false))
            {
                return new ObservableResponse(possibleResult.Response, possibleResult.Watch);
            }

            return null;
        }
    }
}
