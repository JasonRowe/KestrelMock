using KestrelMockServer.Settings;

namespace KestrelMockServer.Services
{
    public class DynamicMockAddedResponse
    {
        public string Message { get; set; }

        public Watch Watch { get; set; }

        public static DynamicMockAddedResponse Create(HttpMockSetting settings)
        {
            if (settings == null)
            {
                return new DynamicMockAddedResponse()
                {
                    Message = "Dynamic mock settings were null, please check your request."
                };
            }

            if (settings.Watch == null)
            {
                return new DynamicMockAddedResponse
                {
                    Message = "Dynamic mock added without observability."
                };
            }

            return new DynamicMockAddedResponse
            {
                Message = $@"Dynamic mock added with observability, call /kestrelmock/observe/{settings.Watch.Id}",
                Watch = settings.Watch
            };
        }
    }
}
