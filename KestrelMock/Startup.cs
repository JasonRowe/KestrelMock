using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KestrelMock
{
	public class Startup
	{
		private readonly IConfiguration _config;
		private ConcurrentDictionary<string, HttpMockSetting> _pathMappings;
		private ConcurrentDictionary<string, HttpMockSetting> _pathStartsWithMappings;
		private ConcurrentDictionary<string, List<HttpMockSetting>> _bodyCheckMappings;

		public Startup(IConfiguration config)
		{
			_config = config;
			_pathMappings = new ConcurrentDictionary<string, HttpMockSetting>();
			_pathStartsWithMappings = new ConcurrentDictionary<string, HttpMockSetting>();
			_bodyCheckMappings = new ConcurrentDictionary<string, List<HttpMockSetting>>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.Run(async (context) =>
			{
				if (_pathMappings.IsEmpty)
				{
					SetupPathMappings();
				}

				var path = context.Request.Path + context.Request.QueryString.ToString();
				string body = null;

				if (context.Request.Body != null)
				{
					using (StreamReader reader = new StreamReader(context.Request.Body))
					{
						body = reader.ReadToEnd();
					}
				}

				var matchResult = FindMatches(path, body);

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

					await context.Response.WriteAsync(matchResult.Body);
				}
			});
		}

		private Response FindMatches(string path, string body)
		{
			Response result = null;

			if (_pathMappings.ContainsKey(path))
			{
				result = _pathMappings[path].Response;
			}

			if (result == null && _pathStartsWithMappings != null)
			{
				foreach (var pathStart in _pathStartsWithMappings)
				{
					if (path.StartsWith(pathStart.Key))
					{
						result = pathStart.Value.Response;
					}
				}
			}

			if (result == null && _bodyCheckMappings != null && _bodyCheckMappings.ContainsKey(path))
			{
				var possibleResults = _bodyCheckMappings[path];

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

		private void SetupPathMappings()
		{
			var mockSettingsConfigSection = _config.GetSection("MockSettings");
			var httpMockSettings = mockSettingsConfigSection.Get<List<HttpMockSetting>>();

			if (httpMockSettings == null || !httpMockSettings.Any())
			{
				return;
			}

			foreach (var httpMockSetting in httpMockSettings)
			{
				if (!string.IsNullOrEmpty(httpMockSetting.Request.Path))
				{
					if (!string.IsNullOrEmpty(httpMockSetting.Request.BodyContains) || !string.IsNullOrEmpty(httpMockSetting.Request.BodyDoesNotContain))
					{
						if (_bodyCheckMappings.ContainsKey(httpMockSetting.Request.Path))
						{
							var bodyContainesList = _bodyCheckMappings[httpMockSetting.Request.Path];
							bodyContainesList.Add(httpMockSetting);
						}
						else
						{
							_bodyCheckMappings.TryAdd(httpMockSetting.Request.Path, new List<HttpMockSetting> { httpMockSetting });
						}
					}
					else
					{
						_pathMappings.TryAdd(httpMockSetting.Request.Path, httpMockSetting);
					}
				}
				else if (!string.IsNullOrEmpty(httpMockSetting.Request.PathStartsWith))
				{
					_pathStartsWithMappings.TryAdd(httpMockSetting.Request.PathStartsWith, httpMockSetting);
				}
			}
		}
	}
}
