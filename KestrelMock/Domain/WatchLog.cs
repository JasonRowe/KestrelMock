namespace KestrelMockServer.Domain
{
    public class WatchLog
    {
        public WatchLog(string path, string body, string method)
        {
            Path = path;
            Body = body;
            Method = method;
        }

        public string Path { get; }

        public string Body { get; }

        public string Method { get; }
    }
}
