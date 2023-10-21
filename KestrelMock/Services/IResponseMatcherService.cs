using KestrelMockServer.Domain;
using KestrelMockServer.Settings;

namespace KestrelMockServer.Services
{
    public interface IResponseMatcherService
    {
        Response FindMatchingResponseMock(string path, string body, string method, InputMappings mapping, Watcher watcher);
    }
}