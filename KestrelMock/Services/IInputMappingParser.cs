using KestrelMock.Domain;
using System.Threading.Tasks;

namespace KestrelMock.Services
{
    public interface IInputMappingParser
    {
        Task<InputMappings> ParsePathMappings();
    }
}