using KestrelMock.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KestrelMock.Services
{
    /// <summary>
    /// this is the mock aspnetcore middleware
    /// </summary>
    public class MockService
    {
        private readonly MockConfiguration _mockConfiguration;
        private readonly RequestDelegate _next;
        private readonly IInputMappingParser _inputMappingParser;
        private readonly IResponseMatcherService _responseMatcher;
        private readonly IBodyWriterService _bodyWriterService;

        public MockService(IOptions<MockConfiguration> options, 
            RequestDelegate next,
            IInputMappingParser inputMappingParser,
            IResponseMatcherService responseMatcher,
            IBodyWriterService bodyWriterService)
        {
            _mockConfiguration = options.Value;
            _next = next;
            _inputMappingParser = inputMappingParser;
            _responseMatcher = responseMatcher;
            _bodyWriterService = bodyWriterService;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await InvokeMock(context, _inputMappingParser);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var errorResponse = new
                {
                    error = ex.ToString()
                };

                await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
            }
            
            //breakes execution
            //await _next(context);
        }

        protected async Task<bool> InvokeMock(HttpContext context, IInputMappingParser inputMappingParser)
        {
            // TODO we may want to cache this instead of loading mappings with each request.
            var mappings = await inputMappingParser.ParsePathMappings();

            string path = context.Request.GetEncodedPathAndQuery();

            string body = null;

            if (context.Request.Body != null)
            {
                using StreamReader reader = new StreamReader(context.Request.Body);
                body = await reader.ReadToEndAsync();
            }

            var method = context.Request.Method;

            var matchResult = _responseMatcher.FindMatchingResponseMock(path, body, method, mappings);

            if (matchResult is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return true;
            }

            if (matchResult.Headers?.Any() == true)
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

                if (matchResult.Replace != null)
                {
                    resultBody = _bodyWriterService.UpdateBody(path, matchResult, resultBody);
                }

                await context.Response.WriteAsync(resultBody);
            }

            return true;
        }
    }

}
