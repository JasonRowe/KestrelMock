using KestrelMockServer.Domain;
using KestrelMockServer.Settings;

namespace KestrelMockServer.Services
{
    public class ResponseMatcherService : IResponseMatcherService
    {
        public Response FindMatchingResponseMock(string path, string body, string method, InputMappings mapping)
        {
            Response result = null;

            var pathMappingKey = new PathMappingKey
            {
                Path = path,
                Method = method,
            };

            if (mapping.PathMapping.ContainsKey(pathMappingKey) && mapping.PathMapping[pathMappingKey].Request.Methods.Contains(method))
            {
                result = mapping.PathMapping[pathMappingKey].Response;
            }

            if (result == null && mapping.PathStartsWithMapping != null)
            {
                foreach (var pathStart in mapping.PathStartsWithMapping)
                {
                    if (path.StartsWith(pathStart.Key.Path) && pathStart.Value.Request.Methods.Contains(method))
                    {
                        result = pathStart.Value.Response;
                    }
                }
            }

            if (result == null && mapping.PathMatchesRegexMapping != null)
            {
                foreach (var pathRegex in mapping.PathMatchesRegexMapping)
                {
                    if (pathRegex.Key.Regex.IsMatch(path) && pathRegex.Value.Request.Methods.Contains(method))
                    {
                        result = pathRegex.Value.Response;
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
                            result = possibleResult.Response;
                        }
                    }
                    else if (!string.IsNullOrEmpty(possibleResult.Request.BodyDoesNotContain) && possibleResult.Request.Methods.Contains(method))
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
    }
}
