using KestrelMockServer.Settings;

namespace KestrelMockServer.Services
{
    public class ObservableResponse
    {
        public ObservableResponse(Response response, Watch watch)
        {
            Response = response;
            Watch = watch;
        }

        public Response Response { get; }

        public Watch Watch { get; }
    }
}
