using KestrelMockServer.Settings;

namespace KestrelMockServer.Services
{
    public interface IBodyWriterService
    {
        string UpdateBody(string path, Response matchResult, string resultBody);
    }
}