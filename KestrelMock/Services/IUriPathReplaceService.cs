using KestrelMockServer.Settings;

namespace KestrelMockServer.Services
{
    public interface IUriPathReplaceService
    {
        string UriPathReplacements(string path, Response matchResult, string resultBody);
    }
}