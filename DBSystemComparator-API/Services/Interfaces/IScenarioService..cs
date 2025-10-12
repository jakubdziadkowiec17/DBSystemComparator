using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Services.Interfaces
{
    public interface IScenarioService
    {
        List<ScenarioDTO> GetSceanarios();
    }
}