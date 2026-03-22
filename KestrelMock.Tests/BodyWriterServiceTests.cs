using System.Collections.Generic;
using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Moq;
using Xunit;

namespace KestrelMock.Tests
{
    public class BodyWriterServiceTests
    {
        private readonly Mock<IUriPathReplaceService> _uriPathReplaceServiceMock;
        private readonly BodyWriterService _service;

        public BodyWriterServiceTests()
        {
            _uriPathReplaceServiceMock = new Mock<IUriPathReplaceService>();
            _service = new BodyWriterService(_uriPathReplaceServiceMock.Object);
        }

        [Fact]
        public void UpdateBody_WithRegexUriReplacements_ReplacesBody()
        {
            // Arrange
            var path = "/api/user/123";
            var response = new Response
            {
                Replace = new Replace
                {
                    RegexUriReplacements = new Dictionary<string, string>
                    {
                        { "userId", @"/api/user/(\d+)" }
                    }
                }
            };
            var resultBody = "{\"userId\": 0, \"name\": \"John\"}";

            // Act
            var updatedBody = _service.UpdateBody(path, response, resultBody);

            // Assert
            Assert.Contains("\"userId\":123", updatedBody);
        }

        [Fact]
        public void UpdateBody_WithRegexUriReplacements_NoMatch_DoesNotReplace()
        {
            // Arrange
            var path = "/api/user/abc";
            var response = new Response
            {
                Replace = new Replace
                {
                    RegexUriReplacements = new Dictionary<string, string>
                    {
                        { "userId", @"/api/user/(\d+)" }
                    }
                }
            };
            var resultBody = "{\"userId\": 0, \"name\": \"John\"}";

            // Act
            var updatedBody = _service.UpdateBody(path, response, resultBody);

            // Assert
            Assert.Contains("\"userId\": 0", updatedBody);
        }

        [Fact]
        public void UpdateBody_WithBodyReplacements_ReplacesBody()
        {
            // Arrange
            var path = "/api/test";
            var response = new Response
            {
                Replace = new Replace
                {
                    BodyReplacements = new Dictionary<string, string>
                    {
                        { "name", "Jane" }
                    }
                }
            };
            var resultBody = "{\"userId\": 123, \"name\": \"John\"}";

            // Act
            var updatedBody = _service.UpdateBody(path, response, resultBody);

            // Assert
            Assert.Contains("\"name\":\"Jane\"", updatedBody);
        }

        [Fact]
        public void UpdateBody_WithUriPathReplacements_CallsService()
        {
            // Arrange
            var path = "/api/test/123";
            var response = new Response
            {
                Replace = new Replace
                {
                    UriTemplate = "api/test/{id}",
                    UriPathReplacements = new Dictionary<string, string>
                    {
                        { "id", "{id}" }
                    }
                }
            };
            var resultBody = "{\"id\": 0}";
            var expectedBody = "{\"id\": 123}";

            _uriPathReplaceServiceMock.Setup(s => s.UriPathReplacements(path, response, resultBody))
                .Returns(expectedBody);

            // Act
            var updatedBody = _service.UpdateBody(path, response, resultBody);

            // Assert
            Assert.Equal(expectedBody, updatedBody);
            _uriPathReplaceServiceMock.Verify(s => s.UriPathReplacements(path, response, resultBody), Times.Once);
        }
        
        [Fact]
        public void UpdateBody_WithRegexUriReplacements_MultipleMatches_ReplacesAll()
        {
            // Arrange
            var path = "/api/order/456/item/789";
            var response = new Response
            {
                Replace = new Replace
                {
                    RegexUriReplacements = new Dictionary<string, string>
                    {
                        { "orderId", @"/api/order/(\d+)/item/\d+" },
                        { "itemId", @"/api/order/\d+/item/(\d+)" }
                    }
                }
            };
            var resultBody = "{\"orderId\": 0, \"itemId\": 0}";

            // Act
            var updatedBody = _service.UpdateBody(path, response, resultBody);

            // Assert
            Assert.Contains("\"orderId\":456", updatedBody);
            Assert.Contains("\"itemId\":789", updatedBody);
        }

        [Fact]
        public void UpdateBody_WithRegexUriReplacements_NoCaptureGroups_UsesFullMatch()
        {
            // Arrange
            var path = "/api/v1/users";
            var response = new Response
            {
                Replace = new Replace
                {
                    RegexUriReplacements = new Dictionary<string, string>
                    {
                        { "version", @"v\d+" }
                    }
                }
            };
            var resultBody = "{\"version\": \"old\"}";

            // Act
            var updatedBody = _service.UpdateBody(path, response, resultBody);

            // Assert
            Assert.Contains("\"version\":\"v1\"", updatedBody);
        }
    }
}
