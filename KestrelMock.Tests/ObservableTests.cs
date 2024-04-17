using System;
using System.Net.Http;
using System.Threading.Tasks;
using KestrelMockServer.Settings;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace KestrelMock.Tests;

public class ObservableTests : IClassFixture<MockTestApplicationFactory>
{
    private readonly MockTestApplicationFactory _factory;

    public ObservableTests(MockTestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/api/sendsomestuff")]
    public async Task CanObservePut(string url)
    {
        var watchId = Guid.NewGuid();

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
                                "PUT"
                            }
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Body = "banana_x"
                        },
                        Watch = new Watch()
                        {
                            Id = watchId
                        }
                    };
                    opts.TryAdd(setting.Id, setting);
                });

            });
        }).CreateClient();

        // Act
        var response = await client.PutAsync(url, new StringContent(JsonConvert.SerializeObject(new { Blah = "blah" })));

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("banana_x", message);

        //I can observe the data PUT to the mock
        var observe = await client.GetAsync($"/kestrelmock/observe/{watchId}");

        var observeContent = await observe.Content.ReadAsStringAsync();
        Assert.Contains("PUT", observeContent);
        Assert.Contains("Blah", observeContent);
        Assert.Contains("blah", observeContent);

        //It is cleared after observation meaning a second call does NOT return the data
        observe = await client.GetAsync($"/kestrelmock/observe/{watchId}");

        observeContent = await observe.Content.ReadAsStringAsync();
        Assert.DoesNotContain("PUT", observeContent);
        Assert.DoesNotContain("Blah", observeContent);
        Assert.DoesNotContain("blah", observeContent);
    }
}