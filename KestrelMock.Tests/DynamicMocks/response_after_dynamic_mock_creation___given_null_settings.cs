using FluentAssertions;
using KestrelMockServer.Services;
using Xunit;

namespace KestrelMock.Tests.DynamicMocks;

public class response_after_dynamic_mock_creation___given_null_settings
{
    private readonly DynamicMockAddedResponse response;

    public response_after_dynamic_mock_creation___given_null_settings()
    {
        this.response = DynamicMockAddedResponse.Create(null);
    }

    [Fact]
    public void then_watch_is_null() =>
        this.response.Watch.Should().BeNull();

    [Fact]
    public void then_the_issue_is_stated() =>
        this.response.Message.Should().Be("Dynamic mock settings were null, please check your request.");
}