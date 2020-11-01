using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        public IDictionary<string, string> Parse(string inputPath)
        {
            string parameterRegexString = PathAndQuery
               .Replace("/", @"\/")
               .Replace("?", @"\?"); //for query prameters

            foreach (Match match in ParameterRegex.Matches(PathAndQuery))
            {
                var parameterName = match.Groups["parameter"].Value;
                parameters.Add(parameterName);

                parameterRegexString = 
                    parameterRegexString
                    .Replace(match.Value, 
                    $"(?<{parameterName}>[^{{}}?]*)");
            }

            if (!PathAndQuery.Contains("?"))
            {
                //accept any optional parameter string (don't care)
                parameterRegexString = $"{parameterRegexString}\\??";
            }

            parameterRegexString = $"{parameterRegexString}.*";

            var matches = Regex.Match(inputPath, parameterRegexString);

            var result = new Dictionary<string, string>();

            foreach(var parameter in parameters)
            {
                result.Add(parameter, matches.Groups[parameter].Value);
            }

            return result;
        }
    }
}