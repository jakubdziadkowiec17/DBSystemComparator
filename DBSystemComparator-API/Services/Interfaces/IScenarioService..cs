using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Services.Interfaces
{
    public interface IScenarioService
    {
        Task<MetricsDTO> TestScenarioAsync(SelectedScenarioDTO selectedScenarioDTO);
        List<ScenarioDTO> GetSceanarios();
    }
}