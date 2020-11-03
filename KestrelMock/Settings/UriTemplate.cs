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
        public string PathAndQuery { get; }
        public string Path { get; }
        public NameValueCollection QueryParameters { get; }

        private static readonly Regex ParameterRegex = 
            new Regex(
                @"\{(?<parameter>[^{}?]*)\}", 
                RegexOptions.Compiled);

        public UriTemplate(string uriTemplate)
        {
            var uri = new Uri(new Uri("http://localhost"), uriTemplate);
            Path = Uri.UnescapeDataString(uri.AbsolutePath);
            QueryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
        }

        public IDictionary<string, string> Parse(string requestPathAndQuery)
        {
            var outputParameters = new List<string>();

            var inputUri = new Uri(new Uri("http://localhost"), requestPathAndQuery);

            string pathRegexString = Path.Replace("/", @"\/"); 

            foreach (Match match in ParameterRegex.Matches(pathRegexString))
            {
                var parameterName = match.Groups["parameter"].Value;
                outputParameters.Add(parameterName);
                pathRegexString = pathRegexString.Replace(match.Value, $"(?<{parameterName}>[^{{}}?]*)");
            }

            var inputPath = Uri.UnescapeDataString(inputUri.AbsolutePath);
            var pathMatches = Regex.Match(inputPath, pathRegexString);

            var result = new Dictionary<string, string>();

            foreach(var parameter in outputParameters)
            {
                result.Add(parameter, pathMatches.Groups[parameter].Value);
            }

            var inputQueryParametersKeyValues = System.Web.HttpUtility.ParseQueryString(inputUri.Query);

            var currentQueryParameters = inputQueryParametersKeyValues.AllKeys
                .ToDictionary(s => s, s => inputQueryParametersKeyValues[s]);

            if (currentQueryParameters.Any())
            {
                foreach (var key in QueryParameters.AllKeys)
                {
                    if(currentQueryParameters.ContainsKey(key))
                    {
                        result.Add(key, currentQueryParameters[key]);
                    }
                    else
                    {
                        result.Add(key, string.Empty);
                    }
                }
            }

            return result;
        }
    }
}