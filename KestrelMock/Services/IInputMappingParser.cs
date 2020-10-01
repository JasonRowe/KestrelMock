using KestrelMockServer.Domain;
using System.Threading.Tasks;

namespace KestrelMockServer.Services
{
    public interface IInputMappingParser
    {
        Task<InputMappings> ParsePathMappings();
    }
}