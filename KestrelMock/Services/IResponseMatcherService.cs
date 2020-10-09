using KestrelMock.Domain;
using KestrelMock.Settings;

namespace KestrelMock.Services
{
    public interface IResponseMatcherService
    {
        Response FindMatchingResponseMock(string path, string body, string method, InputMappings mapping);
    }
}