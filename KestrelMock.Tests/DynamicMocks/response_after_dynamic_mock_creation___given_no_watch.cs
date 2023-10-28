using FluentAssertions;
using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Xunit;

namespace KestrelMock.Tests.DynamicMocks;

public class response_after_dynamic_mock_creation___given_no_watch
{
    private readonly DynamicMockAddedResponse response;

    public response_after_dynamic_mock_creation___given_no_watch()
    {
        this.response = DynamicMockAddedResponse.Create(new HttpMockSetting() { Watch = null });
    }

    [Fact]
    public void then_watch_is_null() =>
        this.response.Watch.Should().BeNull();

    [Fact]
    public void then_success_without_observability_is_stated() =>
        this.response.Message.Should().Be("Dynamic mock added without observability.");
}