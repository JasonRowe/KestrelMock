using KestrelMock.Settings;

namespace KestrelMock.Services
{
    public interface IBodyWriterService
    {
        string UpdateBody(string path, Response matchResult, string resultBody);
    }
}