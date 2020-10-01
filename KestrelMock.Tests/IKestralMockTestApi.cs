using System.Threading.Tasks;
using Refit;

namespace KestrelMockServer.Tests
{
	public interface IKestralMockTestApi
	{
		[Get("/hello/world")]
		Task<HelloWorld> GetHelloWorldWorld();
	}
}
