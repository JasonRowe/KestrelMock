using KestrelMock.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KestrelMock.Services
{
    public class MockService
    {
        private readonly MockConfiguration _mockConfiguration;
        private readonly RequestDelegate _next;

        public MockService(IOptions<MockConfiguration> options, RequestDelegate next)
        {
            _mockConfiguration = options.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var mappings = await InputMappingParser.ParsePathMappings(_mockConfiguration);


            var path = context.Request.Path + context.Request.QueryString.ToString();
            string body = null;

            if (context.Request.Body != null)
            {
                using (StreamReader reader = new StreamReader(context.Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
            }

            var matchResult = ResponseMatcher.FindMatchingResponseMock(path, body, mappings);

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

                    if (matchResult.Replace is null)
                    {
                        await context.Response.WriteAsync(resultBody);
                        return;
                    }

                    if (matchResult.Replace.RegexUriReplacements?.Any() == true)
                    {
                        foreach (var keyVal in matchResult.Replace.RegexUriReplacements)
                        {
                            resultBody = BodyReplacementService.RegexUriReplace(path, resultBody, keyVal);
                        }
                    }

                    if (matchResult.Replace.BodyReplacements?.Any() == true)
                    {
                        foreach (var keyVal in matchResult.Replace.BodyReplacements)
                        {
                            resultBody = BodyReplacementService.RegexBodyRewrite(resultBody, keyVal.Key, keyVal.Value);
                        }
                    }

                    if (matchResult.Replace.UriPathReplacements?.Any() == true
                        && !String.IsNullOrWhiteSpace(matchResult.Replace.UriTemplate))
                    {
                        resultBody = BodyReplacementService.UriPathReplacements(path, matchResult, resultBody);
                    }

                    await context.Response.WriteAsync(resultBody);
                }
            }

            //breakes execution
            //await _next(context);
        }
    }

}
