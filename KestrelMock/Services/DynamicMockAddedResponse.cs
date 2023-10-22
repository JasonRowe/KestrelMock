using KestrelMockServer.Settings;

namespace KestrelMockServer.Services
{
    public class DynamicMockAddedResponse
    {
        public string Message { get; set; }

        public Watch Watch { get; set; }

        public static DynamicMockAddedResponse Create(Watch watch)
        {
            return new DynamicMockAddedResponse
            {
                Message = watch == null
                    ? "Dynamic mock added without observability."
                    : $@"Dynamic mock added with observability, call /kestrelmock/observe/{watch.Id}",
                Watch = watch
            };
        }
    }
}
