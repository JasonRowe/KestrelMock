using FluentAssertions;
using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Xunit;

namespace KestrelMock.Tests.DynamicMocks;

public class response_after_dynamic_mock_creation___given_watch
{
    private readonly DynamicMockAddedResponse response;

    public response_after_dynamic_mock_creation___given_watch()
    {
        this.response = DynamicMockAddedResponse.Create(new HttpMockSetting() { Watch = new Watch() });
    }

    [Fact]
    public void then_watch_is_returned() =>
        this.response.Watch.Should().NotBeNull();

    [Fact]
    public void then_the_watch_id_is_returned() =>
        this.response.Watch.Id.Should().NotBeEmpty();

    [Fact]
    public void then_success_with_observability_is_stated() =>
        this.response.Message.Should().Contain("Dynamic mock added with observability, call /kestrelmock/observe/");

    [Fact]
    public void then_a_link_to_the_observable_mock_is_provided() =>
        this.response.Message.Should().Contain($"/kestrelmock/observe/{this.response.Watch.Id}");
}