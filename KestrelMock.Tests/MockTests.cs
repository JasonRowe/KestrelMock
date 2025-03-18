using FluentAssertions;
using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace KestrelMock.Tests;

public class MockTests : IClassFixture<MockTestApplicationFactory>
{

    [Fact]
    public void ValidateConfiguration()
    {
        try
        {
            KestrelMockServer.KestrelMock.Run(new ConfigurationBuilder().Build());
        }
        catch (Exception ex)
        {
            Assert.Contains("Configuration must include 'MockSettings' section", ex.Message);
        }
    }

    [Fact]
    public void RunAsyncReturns()
    {
        var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", optional: false);
        var configuration = builder.Build();
        var runAsyncResult = KestrelMockServer.KestrelMock.RunAsync(configuration);
        Assert.NotNull(runAsyncResult);
    }

    private readonly MockTestApplicationFactory _factory;

    public MockTests(MockTestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/starts/with/xhsythf")]
    public async Task CanMockResponseUsingPathStartsWith(string url)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure<MockConfiguration>(opts =>
                {
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            PathStartsWith = url,
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            }
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "banana_x"
                        }
                    };
                    opts.TryAdd(setting.Id, setting);
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var message = await response.Content.ReadAsStringAsync();

        Assert.Contains("banana_x", message);
    }

    [Theory]
    [InlineData("/starts/with/xhsythf")]
    public async Task CanMockResponseUsingPathStartsWithAndBodyContains(string url)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure<MockConfiguration>(opts =>
                {
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            PathStartsWith = url,
                            BodyContains = "test",
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "POST"
                            }
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "banana_x"
                        }
                    };
                    opts.TryAdd(setting.Id, setting);
                });
            });
        }).CreateClient();

        var body = new StringContent("test");

        // Act
        var response = await client.PostAsync(url, body);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var message = await response.Content.ReadAsStringAsync();

        Assert.Contains("banana_x", message);
    }


    [Theory]
    [InlineData("/starts/with/xhsythf")]
    public async Task CanMockResponseUsingPathStartsWithAndBodyContains_WithMultipleBodyContains(string url)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure<MockConfiguration>(opts =>
                {
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Path = url,
                            BodyContains = "test",
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "POST"
                            }
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "banana_x"
                        }
                    };
                    opts.TryAdd(setting.Id, setting);
                    var settingSamePath = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Path = url,
                            BodyContains = "different-body",
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "POST"
                            }
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "banana_y"
                        }
                    };
                    opts.TryAdd(settingSamePath.Id, settingSamePath);
                });
            });
        }).CreateClient();

        var body = new StringContent("different-body");

        // Act
        var response = await client.PostAsync(url, body);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var message = await response.Content.ReadAsStringAsync();

        Assert.Contains("banana_y", message);
    }

    [Theory]
    [InlineData("/starts/with/xhsythf")]
    public async Task CanMockResponseUsingPathStartsWithAndBodyDoesNotContain(string url)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure<MockConfiguration>(opts =>
                {
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            PathStartsWith = url,
                            BodyDoesNotContain = "test",
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "POST"
                            }
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "banana_x"
                        }
                    };
                    opts.TryAdd(setting.Id, setting);
                });
            });
        }).CreateClient();

        var body = new StringContent("doesNotContain");

        // Act
        var response = await client.PostAsync(url, body);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var message = await response.Content.ReadAsStringAsync();

        Assert.Contains("banana_x", message);
    }

    [Theory]
    [InlineData("/test/1234/xyz")]
    public async Task CanMockResponseUsingPathRegex_Matches(string url)
    {

        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            },
                            PathMatchesRegex = ".+\\d{4}.+"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "regex_is_working"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("regex_is_working", message);
    }

    [Theory]
    [InlineData("/test/1234/xyz")]
    public async Task CanMockResponseUsingPathRegex_Matches_AndBodyContains(string url)
    {

        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "POST"
                            },
                            BodyContains = "test",
                            PathMatchesRegex = ".+\\d{4}.+"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "regex_is_working"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var content = new StringContent("test");

        // Act
        var response = await client.PostAsync(url, content);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("regex_is_working", message);
    }

    [Theory]
    [InlineData("/test/1234/xyz")]
    public async Task CanMockResponseUsingPathRegex_Matches_AndBodyDoesNotContain(string url)
    {

        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "POST"
                            },
                            BodyDoesNotContain = "test",
                            PathMatchesRegex = ".+\\d{4}.+"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "regex_is_working"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var content = new StringContent("notContain");

        // Act
        var response = await client.PostAsync(url, content);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("regex_is_working", message);
    }

    [Fact]
    public async Task CanMockResponseUsingPathRegex_NoMatch()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            },
                            PathMatchesRegex = ".+\\d{4}.+"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "regex_is_working"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/abcd/xyz");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CanMockGetOrPostResponseUsingExactPath()
    {
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET", "POST"
                            },
                            Path = "/hello/world"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "hello"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var responseGet = await client.GetAsync("hello/world");

        Assert.Contains("hello", await responseGet.Content.ReadAsStringAsync());
        Assert.Equal(200, (int)responseGet.StatusCode);

        var responsePost = await client.PostAsync("hello/world", new StringContent("test"));

        Assert.Contains("hello", await responsePost.Content.ReadAsStringAsync());
        Assert.Equal(200, (int)responsePost.StatusCode);
    }

    [Theory,
     InlineData(true, 200),
     InlineData(false, 404)]
    public async Task CanMockBodyContainsResponse(bool bodyContains, int statusCode)
    {
        var expectedBodyContent = "000000";

        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "POST"
                            },
                            Path = "/api/estimate",
                            BodyContains = expectedBodyContent
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "BodyContains Works"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var content = bodyContains ? new StringContent(expectedBodyContent) : new StringContent("X");

        var response = await client.PostAsync("api/estimate", content);

        if (bodyContains)
        {
            Assert.Contains("BodyContains Works", await response.Content.ReadAsStringAsync());
        }

        Assert.Equal(statusCode, (int)response.StatusCode);
    }

    [Theory,
     InlineData(true, 404),
     InlineData(false, 200)]
    public async Task CanMockBodyDoesNotContainResponse(bool bodyContains, int statusCode)
    {
        var unwantedBodyContent = "000000";

        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "POST"
                            },
                            Path = "/api/estimate",
                            BodyDoesNotContain = unwantedBodyContent
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "BodyDoesNotContain Works"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var content = bodyContains ? new StringContent(unwantedBodyContent) : new StringContent("X");

        var response = await client.PostAsync("api/estimate", content);

        if (!bodyContains)
        {
            Assert.Contains("BodyDoesNotContain Works", await response.Content.ReadAsStringAsync());
        }

        Assert.Equal(statusCode, (int)response.StatusCode);
    }


    [Theory]
    [InlineData("CHIANTI", "RED", ""),
     InlineData("CHIANTI", "RED", "?year=1978")]
    public async Task CanReplaceBodyFromUriParameters(string wine, string color, string extraQuery)
    {
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            },
                            PathStartsWith = "/api/wines/"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "{ \"wine\" : \"123\", \"color\" : \"abcde\" }",
                            Replace = new Replace
                            {
                                UriTemplate = @"/api/wines/{wine}/{color}",
                                UriPathReplacements = new System.Collections.Generic.Dictionary<string, string>
                                {
                                    { "wine", "{wine}" },
                                    { "color", "{color}"}
                                }
                            }
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var response = await client.GetAsync($"/api/wines/{wine}/{color}{extraQuery}");

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains($"\"wine\":\"{wine}\"", body);
        Assert.Contains($"\"color\":\"{color}\"", body);
        Assert.Equal(200, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("CHIANTI", "RED", "?year=1978")]
    public async Task CanReplaceBodyFromUriWithQuery(string wine, string color, string extraQuery)
    {
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            },
                            PathStartsWith = "/api/wines/"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "{ \"wine\" : \"123\", \"color\" : \"abcde\", \"year\":\"0\" }",
                            Replace = new Replace
                            {
                                UriTemplate = @"/api/wines/{wine}/{color}?year={year}",
                                UriPathReplacements = new System.Collections.Generic.Dictionary<string, string>
                                {
                                    { "wine", "{wine}" },
                                    { "color", "{color}" },
                                    { "year", "{year}" }
                                }
                            }
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var response = await client.GetAsync($"/api/wines/{wine}/{color}{extraQuery}");

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains($"\"wine\":\"{wine}\"", body);
        Assert.Contains($"\"color\":\"{color}\"", body);
        Assert.Contains($"\"year\":\"1978\"", body);

        Assert.Equal(200, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("CHIANTI", "RED", "?doesnt=matter")]
    public async Task CanReplaceBodyWhenMultiple(string wine, string color, string extraQuery)
    {
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            },
                            PathStartsWith = "/api/wines/"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "{ \"wine\" : \"123\", \"color\" : \"abcde\", \"year\": 1978," +
                                   " \"nested\" : { \"color\" : \"x\" }" +
                                   " }",
                            Replace = new Replace
                            {
                                UriTemplate = @"/api/wines/{wine}/{color}?year={year}",
                                UriPathReplacements = new System.Collections.Generic.Dictionary<string, string>
                                {
                                    { "wine", wine },
                                    { "color", color }
                                }
                            }
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var response = await client.GetAsync($"/api/wines/{wine}/{color}{extraQuery}");

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains($"\"wine\":\"{wine}\"", body);
        Assert.Contains($", \"color\":\"{color}\"", body);
        Assert.Contains($"{{ \"color\":\"{color}\"", body);

        Assert.Equal(200, (int)response.StatusCode);
    }


    [Theory]
    [InlineData("CHIANTI", "RED", "?year=1978")]
    public async Task CanReplaceFromUriNumbersInBody(string wine, string color, string extraQuery)
    {
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            },
                            PathStartsWith = "/api/wines/"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "{ \"wine\" : \"123\", \"color\" : \"abcde\", \"year\":0 }",
                            Replace = new Replace
                            {
                                UriTemplate = @"/api/wines/{wine}/{color}?year={year}",
                                UriPathReplacements = new System.Collections.Generic.Dictionary<string, string>
                                {
                                    { "wine", wine },
                                    { "color", color },
                                    { "year", "1978" }
                                }
                            }
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var response = await client.GetAsync($"/api/wines/{wine}/{color}{extraQuery}");

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains($"\"wine\":\"{wine}\"", body);
        Assert.Contains($"\"color\":\"{color}\"", body);
        Assert.Contains($"\"year\":1978", body);

        Assert.Equal(200, (int)response.StatusCode);
    }


    [Fact]
    public async Task CanReplaceBodySingleFieldFromSettings()
    {
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            },
                            PathStartsWith = "/api/replace/"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "{ \"replace\" : \"123\" }",
                            //TODO: noted bug, replacement does not work correctly for numbers in json
                            Replace = new Replace
                            {
                                BodyReplacements = new System.Collections.Generic.Dictionary<string, string>
                                {
                                    { "replace", "modified" }
                                }
                            }
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var response = await client.GetAsync($"/api/replace/");

        Assert.Equal($"{{ \"replace\":\"modified\" }}", await response.Content.ReadAsStringAsync());
        Assert.Equal(200, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("Body Contains", new[] { "Body" }, 200, "Because 'Body' exists in 'Body Contains'")]
    [InlineData("Body Contains", new[] { "Contains" }, 200, "Because 'Contains' exists in 'Body Contains'")]
    [InlineData("Body Contains", null, 200, "Because BodyContainsArray is null and path still matches")]
    [InlineData("Body Contains", new string[] { }, 404, "Because BodyContainsArray is empty so body does not match")]
    [InlineData("Body Contains", new string[] { "Something" }, 404, "Because 'Something' does not exist in 'Body Contains'")]
    [InlineData("Body Contains", new string[] { "Body", "Something" }, 404, "Because 'Something' does not exist in 'Body Contains'")]
    public async Task CanMockBodyContainsArrayResponse(string postBody, string[]? bodyContains, int statusCode, string message)
    {
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new List<string>
                            {
                                "POST"
                            },
                            Path = "/api/estimate",
                            BodyContainsArray = bodyContains?.ToList() ?? null,
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "BodyContains Works"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var content = new StringContent(postBody);

        var response = await client.PostAsync("api/estimate", content);

        if (statusCode == 200)
        {
            var bodyResponse = await response.Content.ReadAsStringAsync();

            bodyResponse.Should().Be("BodyContains Works");
        }

        statusCode.Should().Be((int)response.StatusCode, $"Because {message}");
    }

    [Fact]
    public async Task CanMockPathStartsWithAndBodyContainsWithVaryingPostUrls()
    {
        var sameUrlDifferentObjects = new List<KeyValuePair<string, dynamic>>()
        {
            new KeyValuePair<string, dynamic>("/api/estimate", new { expectedId = Guid.NewGuid(), postId = Guid.NewGuid() }),
            new KeyValuePair<string, dynamic>("/api/estimate", new { expectedId = Guid.NewGuid(), postId = Guid.NewGuid() }),
            new KeyValuePair<string, dynamic>("/api/estimate", new { expectedId = Guid.NewGuid(), postId = Guid.NewGuid() }),
            new KeyValuePair<string, dynamic>("/api/estimate", new { expectedId = Guid.NewGuid(), postId = Guid.NewGuid() }),
        };

        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    foreach (var url in sameUrlDifferentObjects)
                    {
                        var setting = new HttpMockSetting
                        {
                            Request = new Request
                            {
                                Methods = new List<string>
                                {
                                    "POST"
                                },
                                PathStartsWith = url.Key,
                                BodyContains = $"{url.Value.postId}",
                            },
                            Response = new Response
                            {
                                Status = 200,
                                Body = $"Found Mock for url {url.Value.expectedId}"
                            }
                        };

                        opts.TryAdd(setting.Id, setting);
                    }
                }));
            });
        }).CreateClient();

        foreach (var url in sameUrlDifferentObjects)
        {
            var response = await client.PostAsync($"{url.Key}?id={Guid.NewGuid()}", new StringContent($"{url.Value.postId}"));

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();

            body.Should().Be($"Found Mock for url {url.Value.expectedId}");
        }
    }

    [Fact]
    public async Task LoadBodyFromRelativePath()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("api/fromfile", new StringContent(""));

        // note: to work on all os, you should specify body from file only in unix-compliant relative path
        // so : ./this/path/file.x and not like .\\this\\file.windows

        var content = await response.Content.ReadAsStringAsync();

        Assert.True(content == "Body loaded from file");
        Assert.Equal(200, (int)response.StatusCode);
    }

    [Fact]
    public async Task CanReturnErrorStatus()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("errors/502", new StringContent("foo"));
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(502, (int)response.StatusCode);
    }

    [Fact]
    public async Task MockInternalError_JsonErrorResponse()
    {
        var client = _factory.WithWebHostBuilder(h =>
        {
            h.ConfigureTestServices(services =>
            {
                var inputMappingParserMock = new Mock<IInputMappingParser>();
                inputMappingParserMock.Setup(s => s.ParsePathMappings()).Throws(new Exception("error"));
                services.AddTransient<IInputMappingParser>(_ => inputMappingParserMock.Object);
            });
        }).CreateClient();

        var response = await client.PostAsync("test", new StringContent("x"));
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("\"error\":\"System.Exception: error", content);
    }

    [Fact]
    public async Task KestralMock_works_with_Refit()
    {
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            },
                            Path = "/hello/world"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "{ \"hello\": \"world\" }"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var testApi = RestService.For<IKestralMockTestApi>(client);

        var helloWorld = await testApi.GetHelloWorldWorld();

        Assert.Contains("world", helloWorld.Hello);
    }


    [Theory]
    [InlineData("/hello/world", "NotFound", "", "DELETE")]
    [InlineData("/hello/world", "OK", "foo3", "PUT")]
    public async Task KestralMock_matchesPath_using_verb(string url, string statusCode, string body, string method)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                method
                            },
                            Path = url
                        },
                        Response = new Response
                        {
                            Status = (int)Enum.Parse(typeof(HttpStatusCode), statusCode, true),
                            Body = body
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        HttpResponseMessage response;
        if (string.IsNullOrWhiteSpace(body))
        {
            response = await client.DeleteAsync(url);
        }
        else
        {
            response = await client.PutAsync(url, new StringContent(body));
        }

        Assert.Equal(response.StatusCode.ToString(), statusCode);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains(body, message, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    [Theory]
    [InlineData("/starts/with/but_does_not_match_verb", "NotFound", "", "DELETE")]
    [InlineData("/starts/with/matches_put_method", "OK", "foo1", "PUT")]
    public async Task KestralMock_matchesPathStartsWith_using_verb(string url, string statusCode, string body, string method)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                method
                            },
                            PathStartsWith = url
                        },
                        Response = new Response
                        {
                            Status = (int)Enum.Parse(typeof(HttpStatusCode), statusCode, true),
                            Body = body
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        HttpResponseMessage response;
        if (string.IsNullOrWhiteSpace(body))
        {
            response = await client.DeleteAsync(url);
        }
        else
        {
            response = await client.PutAsync(url, new StringContent(body));
        }

        Assert.Equal(response.StatusCode.ToString(), statusCode);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains(body, message, StringComparison.InvariantCultureIgnoreCase);
        }
    }


    [Theory]
    [InlineData("/test/1234/xyz", "NotFound", "", "DELETE")]
    [InlineData("/test/1234/xyz", "OK", "foo2", "PUT")]
    public async Task KestralMock_matchesRegex_using_verb(string url, string statusCode, string body, string method)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                method
                            },
                            PathMatchesRegex = ".+\\d{4}.+"
                        },
                        Response = new Response
                        {
                            Status = (int)Enum.Parse(typeof(HttpStatusCode), statusCode, true),
                            Body = body
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        HttpResponseMessage response;
        if (string.IsNullOrWhiteSpace(body))
        {
            response = await client.DeleteAsync(url);
        }
        else
        {
            response = await client.PutAsync(url, new StringContent(body));
        }

        Assert.Equal(response.StatusCode.ToString(), statusCode);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains(body, message, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    [Fact]
    public async Task Can_Retrieve_Mocks_With_Id()
    {
        var url = "kestrelmock/mocks/";
        var mockId = Guid.NewGuid();
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    var setting = new HttpMockSetting
                    {
                        Id = mockId.ToString(),
                        Request = new Request
                        {
                            Methods = new List<string>
                            {
                                "GET"
                            },
                            Path = "/hello/world"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "{ \"hello\": \"world\" }"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        HttpResponseMessage response = await client.GetAsync(url);


        Assert.Equal("OK", response.StatusCode.ToString());

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var message = await response.Content.ReadAsStringAsync();

            var mocks = JsonSerializer.Deserialize<List<HttpMockSetting>>(message);
            Assert.Equal(3, mocks.Count);

            var currentMock = mocks.FirstOrDefault(m => m.Id == mockId.ToString());
            Assert.NotNull(currentMock);
        }
    }

    [Fact]
    public async Task Can_Add_Mocks_With_Id()
    {
        var url = "kestrelmock/mocks/";
        var mockId = Guid.NewGuid().ToString();
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Id = mockId,
                        Request = new Request
                        {
                            Methods = new List<string>
                            {
                                "GET"
                            },
                            Path = "/hello/world"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "{ \"hello\": \"world\" }"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();

        var settingToAddMockId = Guid.NewGuid().ToString();
        var settingToAdd = new HttpMockSetting
        {
            Id = settingToAddMockId,
            Request = new Request
            {
                Methods = new List<string>
                {
                    "GET"
                },
                Path = "/hello/world"
            },
            Response = new Response
            {
                Status = 200,
                Body = "{ \"hello\": \"world\" }"
            }
        };

        HttpResponseMessage response = await client.PostAsync(url, new StringContent(JsonSerializer.Serialize(settingToAdd)));
        Assert.Equal("OK", response.StatusCode.ToString());

        HttpResponseMessage getResponse = await client.GetAsync(url);

        Assert.Equal("OK", getResponse.StatusCode.ToString());
        if (getResponse.StatusCode == HttpStatusCode.OK)
        {
            var message = await getResponse.Content.ReadAsStringAsync();
            Assert.Contains(settingToAddMockId, message, StringComparison.InvariantCultureIgnoreCase);
        }

    }

    [Fact]
    public async Task Can_Delete_Mocks_by_Id()
    {
        var url = "kestrelmock/mocks/";
        var mockId = Guid.NewGuid().ToString();
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {

                services.Configure((Action<MockConfiguration>)(opts =>
                {
                    opts.Clear();
                    var setting = new HttpMockSetting
                    {
                        Id = mockId,
                        Request = new Request
                        {
                            Methods = new System.Collections.Generic.List<string>
                            {
                                "GET"
                            },
                            Path = "/hello/world"
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "{ \"hello\": \"world\" }"
                        }
                    };

                    opts.TryAdd(setting.Id, setting);
                }));

            });
        }).CreateClient();


        HttpResponseMessage getResponse = await client.GetAsync(url);

        Assert.Equal("OK", getResponse.StatusCode.ToString());
        if (getResponse.StatusCode == HttpStatusCode.OK)
        {
            var message = await getResponse.Content.ReadAsStringAsync();
            Assert.Contains(mockId, message, StringComparison.InvariantCultureIgnoreCase);
        }

        HttpResponseMessage deleteResponse = await client.DeleteAsync(url + mockId);

        HttpResponseMessage checkResultsResponse = await client.GetAsync(url);

        Assert.Equal("OK", checkResultsResponse.StatusCode.ToString());
        if (getResponse.StatusCode == HttpStatusCode.OK)
        {
            var message = await checkResultsResponse.Content.ReadAsStringAsync();
            Assert.DoesNotContain(mockId, message, StringComparison.InvariantCultureIgnoreCase);
        }

    }
}