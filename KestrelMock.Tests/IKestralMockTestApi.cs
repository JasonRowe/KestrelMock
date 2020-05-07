using System.Threading.Tasks;
using Refit;

namespace KestrelMock.Tests
{
	public interface IKestralMockTestApi
	{
		[Get("/hello/world")]
		Task<HelloWorld> GetHelloWorldWorld();
	}
}
