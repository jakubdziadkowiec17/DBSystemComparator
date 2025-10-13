using DBSystemComparator_API.Constants;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DBSystemComparator_API.Controllers
{
    [ApiController]
    [Route("api/scenario")]
    public class ScenarioController : ControllerBase
    {
        private readonly IScenarioService _scenarioService;
        private readonly IErrorLogService _errorLogService;
        public ScenarioController(IScenarioService scenarioService, IErrorLogService errorLogService)
        {
            _scenarioService = scenarioService;
            _errorLogService = errorLogService;
        }

        [HttpPost]
        public async Task<ActionResult<MetricsDTO>> CheckScenario([FromBody] SelectedScenarioDTO selectedScenarioDTO)
        {
            try
            {
                return Ok(await _scenarioService.CheckScenarioAsync(selectedScenarioDTO));
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case ERROR.SELECTED_INCORRECT_SCENARIO:
                        return BadRequest(_errorLogService.CreateErrorLogAsync(ERROR.SELECTED_INCORRECT_SCENARIO));
                    default:
                        return StatusCode(500, _errorLogService.CreateErrorLogAsync(ERROR.CHECKING_SCENARIO_FAILED));
                }
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ScenarioDTO>>> GetScenarios()
        {
            try
            {
                return Ok(_scenarioService.GetSceanarios());
            }
            catch (Exception ex)
            {
                return StatusCode(500, _errorLogService.CreateErrorLogAsync(ERROR.GETTING_SCENARIOS_FAILED));
            }
        }
    }
}