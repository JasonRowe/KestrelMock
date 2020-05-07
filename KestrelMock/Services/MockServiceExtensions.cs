using Microsoft.AspNetCore.Builder;

namespace KestrelMock.Services
{
    public static class MockServiceExtensions
    {
        public static IApplicationBuilder UseMockService(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MockService>();
        }
    }
}
