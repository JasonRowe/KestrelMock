using FluentAssertions;
using KestrelMockServer.Settings;
using KestrelMockServer.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace KestrelMock.Tests;

public class MockServiceAdditionalTests : IClassFixture<MockTestApplicationFactory>
{
    private readonly MockTestApplicationFactory _factory;

    public MockServiceAdditionalTests(MockTestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task InvokeObserve_ReturnsBadRequest_WhenWatchIdMissing()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/kestrelmock/observe");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Please specify the WatchId Guid.");
    }

    [Fact]
    public async Task InvokeObserve_ReturnsBadRequest_WhenWatchIdInvalid()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/kestrelmock/observe/invalid-guid");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Please specify the WatchId Guid.");
    }

    [Fact]
    public async Task InvokeAdminApi_Delete_RemovesWatcher_WhenWatchIsPresent()
    {
        var watchId = Guid.NewGuid();
        var mockId = Guid.NewGuid().ToString();
        
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.Configure<MockConfiguration>(opts =>
                {
                    var setting = new HttpMockSetting
                    {
                        Id = mockId,
                        Request = new Request
                        {
                            Path = "/test-delete",
                            Methods = new List<string> { "GET" }
                        },
                        Response = new Response { Status = 200 },
                        Watch = new Watch { Id = watchId }
                    };
                    opts.TryAdd(setting.Id, setting);
                });
            });
        }).CreateClient();

        var deleteResponse = await client.DeleteAsync($"/kestrelmock/mocks/{mockId}");
        deleteResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task InputMappingParser_LoadBodyFromFile_Throws_WhenFileNotFound()
    {
        var config = new MockConfiguration();
        var setting = new HttpMockSetting
        {
            Request = new Request { Path = "/file", Methods = new List<string> { "GET" } },
            Response = new Response { BodyFromFilePath = "non-existent-file.txt" }
        };
        config.TryAdd(setting.Id, setting);

        var options = Options.Create(config);
        var parser = new InputMappingParser(options);

        Func<Task> action = async () => await parser.ParsePathMappings();
        await action.Should().ThrowAsync<Exception>().WithMessage("Path in BodyFromFilePath not found: non-existent-file.txt");
    }

    [Fact]
    public void UriTemplate_Properties_ReturnsExpectedValues()
    {
        var template = new UriTemplate("/test/{id}?query=1");
        template.PathAndQuery.Should().BeNull();
        template.Path.Should().Be("/test/{id}");
        template.QueryParameters["query"].Should().Be("1");
    }

    [Fact]
    public void UriPathReplaceService_SkipsReplacement_WhenNoMatchFoundForPlaceholder()
    {
        var service = new UriPathReplaceService();
        var response = new Response
        {
            Replace = new Replace
            {
                UriTemplate = "/api/test/{id}",
                UriPathReplacements = new Dictionary<string, string>
                {
                    { "prop", "{id}" }
                }
            }
        };
        var resultBody = "{\"prop\": 0}";
        var updatedBody = service.UriPathReplacements("/api/other/path", response, resultBody);
        
        updatedBody.Should().Be(resultBody);
    }

    [Fact]
    public async Task InputMappingParser_ParsePathMappings_ReturnsEmpty_WhenConfigEmpty()
    {
        var options = Options.Create(new MockConfiguration());
        var parser = new InputMappingParser(options);
        var mappings = await parser.ParsePathMappings();
        mappings.PathMapping.Should().BeEmpty();
    }

    [Fact]
    public async Task InputMappingParser_DuplicatePathStartsWith_AddsToBag()
    {
        var config = new MockConfiguration();
        var setting1 = new HttpMockSetting
        {
            Request = new Request { PathStartsWith = "/start", Methods = new List<string> { "GET" } },
            Response = new Response { Body = "1" }
        };
        var setting2 = new HttpMockSetting
        {
            Request = new Request { PathStartsWith = "/start", Methods = new List<string> { "GET" } },
            Response = new Response { Body = "2" }
        };
        config.TryAdd(setting1.Id, setting1);
        config.TryAdd(setting2.Id, setting2);

        var options = Options.Create(config);
        var parser = new InputMappingParser(options);
        var mappings = await parser.ParsePathMappings();
        
        mappings.PathStartsWithMapping.Should().HaveCount(1);
        var bag = mappings.PathStartsWithMapping.Values.GetEnumerator();
        bag.MoveNext();
        bag.Current.Should().HaveCount(2);
    }

    [Fact]
    public void Replace_ContentType_PropertyAccess()
    {
        var replace = new Replace { ContentType = ContentType.JSON };
        replace.ContentType.Should().Be(ContentType.JSON);
    }
}
