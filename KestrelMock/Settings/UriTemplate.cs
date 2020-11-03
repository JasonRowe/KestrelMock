using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KestrelMockServer.Settings
{
    public class UriTemplate
    {
        private readonly Uri uri;

        public string PathAndQuery { get; }
        public string Path { get; }
        public string Query { get; }
        public NameValueCollection QueryParameters { get; }

        private readonly List<string> parameters;

        private static readonly Regex ParameterRegex = 
            new Regex(
                @"\{(?<parameter>[^{}?]*)\}", 
                RegexOptions.Compiled);

        public UriTemplate(string uriTemplate)
        {
            uri = new Uri(new Uri("http://localhost"), uriTemplate);
            PathAndQuery = Uri.UnescapeDataString(uri.PathAndQuery);
            Path = Uri.UnescapeDataString(uri.AbsolutePath);
            Query = Uri.UnescapeDataString(uri.Query);
            QueryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
            parameters = new List<string>();
        }

        public IDictionary<string, string> Parse(string requestPathAndQuery)
        {
            var inputUri = new Uri(new Uri("http://localhost"), requestPathAndQuery);

            string pathRegexString = Path.Replace("/", @"\/"); 

            foreach (Match match in ParameterRegex.Matches(pathRegexString))
            {
                var parameterName = match.Groups["parameter"].Value;
                parameters.Add(parameterName);
                pathRegexString = pathRegexString.Replace(match.Value, $"(?<{parameterName}>[^{{}}?]*)");
            }

            var inputPath = Uri.UnescapeDataString(inputUri.AbsolutePath);
            var pathMatches = Regex.Match(inputPath, pathRegexString);

            var result = new Dictionary<string, string>();

            foreach(var parameter in parameters)
            {
                result.Add(parameter, pathMatches.Groups[parameter].Value);
            }

            var inputQueryParametersKeyValues = System.Web.HttpUtility.ParseQueryString(inputUri.Query);

            var currentQueryParameters = inputQueryParametersKeyValues.AllKeys.ToDictionary(s => s,
                s => inputQueryParametersKeyValues[s]);

            if (currentQueryParameters.Any())
            {
                foreach (var key in QueryParameters.AllKeys)
                {
                    if(currentQueryParameters.ContainsKey(key))
                    {
                        result.Add(key, currentQueryParameters[key]);
                    }
                }
            }

            return result;
        }
    }
}