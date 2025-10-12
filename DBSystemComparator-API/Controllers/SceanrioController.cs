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

        [HttpGet]
        public async Task<ActionResult<List<ScenarioDTO>>> GetScenarios()
        {
            try
            {
                return Ok(_scenarioService.GetSceanarios());
            }
            catch (Exception ex)
            {
                return StatusCode(500, await _errorLogService.CreateErrorLogAsync(ERROR.GETTING_SCENARIOS_FAILED));
            }
        }
    }
}