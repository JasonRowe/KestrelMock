using KestrelMockServer.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using KestrelMockServer.Domain;

namespace KestrelMockServer.Services
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

        private readonly Watcher _watcher = new Watcher();

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
                if (context.Request.Path.StartsWithSegments(new PathString("/kestrelmock/mocks")))
                {
                    await InvokeAdminApi(context, _watcher);
                }
                else if (context.Request.Path.StartsWithSegments(new PathString("/kestrelmock/observe")))
                {
                    await InvokeObserve(context, _watcher);
                }
                else
                {
                    await InvokeMock(context, _inputMappingParser, _watcher);
                }

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

        protected async Task<bool> InvokeMock(HttpContext context, IInputMappingParser inputMappingParser, Watcher watcher)
        {
            var mappings = await inputMappingParser.ParsePathMappings();

            string path = context.Request.GetEncodedPathAndQuery();

            string body = null;

            if (context.Request.Body != null)
            {
                using StreamReader reader = new StreamReader(context.Request.Body);
                body = await reader.ReadToEndAsync();
            }

            var method = context.Request.Method;

            var matchedResponse = _responseMatcher.FindMatchingResponseMock(path, body, method, mappings, watcher);

            if (matchedResponse is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return true;
            }

            if (matchedResponse.Headers?.Any() == true)
            {
                foreach (var header in matchedResponse.Headers)
                {
                    foreach (var key in header.Keys)
                    {
                        context.Response.Headers.Add(key, header[key]);
                    }
                }
            }

            context.Response.StatusCode = matchedResponse.Status;

            if (!string.IsNullOrWhiteSpace(matchedResponse.Body))
            {
                string resultBody = matchedResponse.Body;

                if (matchedResponse.Replace != null)
                {
                    resultBody = _bodyWriterService.UpdateBody(path, matchedResponse, resultBody);
                }

                await context.Response.WriteAsync(resultBody);
            }

            return true;
        }

        protected async Task<bool> InvokeAdminApi(HttpContext context, Watcher watcher)
        {

            if (context.Request.Method == HttpMethods.Get)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(_mockConfiguration));
            }
            else if (context.Request.Method == HttpMethods.Post)
            {
                using StreamReader reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var setting = JsonConvert.DeserializeObject<HttpMockSetting>(body);
                _mockConfiguration.Add(setting);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(DynamicMockAddedResponse.Create(setting.Watch)));
            }
            else if (context.Request.Method == HttpMethods.Delete)
            {
                var pathNotrailingString = context.Request.Path.ToString().TrimEnd('/');
                var id = pathNotrailingString.Split('/').Last();

                var watch = _mockConfiguration.FirstOrDefault(setting => setting.Id == id).Watch;
                if (watch != null)
                {
                    watcher.Remove(watch.Id);
                }

                _mockConfiguration.RemoveAll(setting => setting.Id == id);
            }

            return true;
        }

        protected async Task<bool> InvokeObserve(HttpContext context, Watcher watcher)
        {
            if (context.Request.Method == HttpMethods.Get)
            {
                var watchId = GetWatchGuid(context);
                if (watchId == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync("Please specify the WatchId Guid.");
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(watcher.GetWatchLogs(watchId.Value)));
                }
            }

            return true;
        }

        private static Guid? GetWatchGuid(HttpContext context)
        {
            var path = context.Request.Path.Value;
            var observePosition = path.IndexOf("observe", StringComparison.InvariantCultureIgnoreCase);
            var partialPath = path.Substring(observePosition);
            var requestPathSegments = partialPath.Split('/');
            if (requestPathSegments.Length > 1)
            {
                if (Guid.TryParse(requestPathSegments[1], out var watchId))
                {
                    return watchId;
                }
            }

            return null;
        }
    }

}
