using KestrelMock.Domain;
using KestrelMock.Settings;

namespace KestrelMock.Services
{
    public static class ResponseMatcher
    {
        public static Response FindMatchingResponseMock(string path, string body, InputMappings mapping)
        {
            Response result = null;

            if (mapping.PathMapping.ContainsKey(path))
            {
                result = mapping.PathMapping[path].Response;
            }

            if (result == null && mapping.PathStartsWithMapping != null)
            {
                foreach (var pathStart in mapping.PathStartsWithMapping)
                {
                    if (path.StartsWith(pathStart.Key))
                    {
                        result = pathStart.Value.Response;
                    }
                }
            }

            if (result == null && mapping.PathMatchesRegexMapping != null)
            {
                foreach (var pathRegex in mapping.PathMatchesRegexMapping)
                {
                    if (pathRegex.Key.IsMatch(path))
                    {
                        result = pathRegex.Value.Response;
                    }
                }
            }

            if (result == null && mapping.BodyCheckMapping?.ContainsKey(path) == true)
            {
                var possibleResults = mapping.BodyCheckMapping[path];

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

    }

}
