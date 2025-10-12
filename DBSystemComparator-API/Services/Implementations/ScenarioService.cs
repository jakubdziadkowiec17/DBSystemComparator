using DBSystemComparator_API.Constants;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Services.Interfaces;

namespace DBSystemComparator_API.Services.Implementations
{
    public class ScenarioService : IScenarioService
    {
        public ScenarioService() {}

        public List<ScenarioDTO> GetSceanarios()
        {
            return Scenarios.ALL;
        }
    }
}