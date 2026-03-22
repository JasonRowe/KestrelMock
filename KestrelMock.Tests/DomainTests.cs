using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using KestrelMockServer.Domain;
using KestrelMockServer.Settings;
using Xunit;

namespace KestrelMock.Tests
{
    public class DomainTests
    {
        [Fact]
        public void PathMappingRegexKey_Equals_ReturnsTrue_ForSameValues()
        {
            var regex = new Regex("test");
            var key1 = new PathMappingRegexKey { Regex = regex, Method = "GET" };
            var key2 = new PathMappingRegexKey { Regex = regex, Method = "GET" };

            key1.Equals(key2).Should().BeTrue();
            key1.GetHashCode().Should().Be(key2.GetHashCode());
        }

        [Fact]
        public void PathMappingRegexKey_Equals_ReturnsFalse_ForDifferentValues()
        {
            var key1 = new PathMappingRegexKey { Regex = new Regex("test1"), Method = "GET" };
            var key2 = new PathMappingRegexKey { Regex = new Regex("test2"), Method = "GET" };
            var key3 = new PathMappingRegexKey { Regex = new Regex("test1"), Method = "POST" };

            key1.Equals(key2).Should().BeFalse();
            key1.Equals(key3).Should().BeFalse();
            key1.Equals(null).Should().BeFalse();
            key1.Equals("not a key").Should().BeFalse();
        }

        [Fact]
        public void PathMappingKey_Equals_ReturnsFalse_ForDifferentValues()
        {
            var key1 = new PathMappingKey { Path = "/path1", Method = "GET" };
            var key2 = new PathMappingKey { Path = "/path2", Method = "GET" };
            
            key1.Equals(key2).Should().BeFalse();
            key1.Equals(null).Should().BeFalse();
            key1.Equals("not a key").Should().BeFalse();
        }

        [Fact]
        public void PathStartsWithMappingKey_Equals_ReturnsTrue_ForSameValues()
        {
            var key1 = new PathStartsWithMappingKey { PathStartsWith = "/test", Method = "GET" };
            var key2 = new PathStartsWithMappingKey { PathStartsWith = "/test", Method = "GET" };

            key1.Equals(key2).Should().BeTrue();
            key1.GetHashCode().Should().Be(key2.GetHashCode());
        }

        [Fact]
        public void PathStartsWithMappingKey_Equals_ReturnsFalse_ForDifferentValues()
        {
            var key1 = new PathStartsWithMappingKey { PathStartsWith = "/test1", Method = "GET" };
            var key2 = new PathStartsWithMappingKey { PathStartsWith = "/test2", Method = "GET" };
            var key3 = new PathStartsWithMappingKey { PathStartsWith = "/test1", Method = "POST" };

            key1.Equals(key2).Should().BeFalse();
            key1.Equals(key3).Should().BeFalse();
            key1.Equals(null).Should().BeFalse();
            key1.Equals("not a key").Should().BeFalse();
        }

        [Fact]
        public void Watcher_Log_RespectsLimit()
        {
            var watcher = new Watcher();
            var watchId = Guid.NewGuid();
            var watch = new Watch { Id = watchId, RequestLogLimit = 2 };

            watcher.Log("/path1", "body1", "GET", watch);
            watcher.Log("/path2", "body2", "GET", watch);
            watcher.Log("/path3", "body3", "GET", watch);

            var logs = watcher.GetWatchLogs(watchId);
            logs.Length.Should().Be(2);
            logs[0].Path.Should().Be("/path2");
            logs[1].Path.Should().Be("/path3");
        }

        [Fact]
        public void Watcher_Remove_HandlesExistentId()
        {
            var watcher = new Watcher();
            var watchId = Guid.NewGuid();
            var watch = new Watch { Id = watchId };
            watcher.Log("/path", "body", "GET", watch);

            watcher.Remove(watchId);
            
            var logs = watcher.GetWatchLogs(watchId);
            logs.Should().BeEmpty();
        }

        [Fact]
        public void Watcher_Remove_HandlesNonExistentId()
        {
            var watcher = new Watcher();
            var watchId = Guid.NewGuid();
            
            // Should not throw
            watcher.Remove(watchId);
            
            var logs = watcher.GetWatchLogs(watchId);
            logs.Should().BeEmpty();
        }

        [Fact]
        public void Watcher_GetWatchLogs_ReturnsEmpty_ForNonExistentId()
        {
            var watcher = new Watcher();
            var logs = watcher.GetWatchLogs(Guid.NewGuid());
            logs.Should().BeEmpty();
        }
        
        [Fact]
        public void WatchLog_Properties_ReturnExpectedValues()
        {
            var log = new WatchLog("/path", "body", "POST");
            log.Path.Should().Be("/path");
            log.Body.Should().Be("body");
            log.Method.Should().Be("POST");
        }
    }
}
