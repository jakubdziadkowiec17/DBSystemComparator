using DBSystemComparator_API.Constants;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DBSystemComparator_API.Controllers
{
    [ApiController]
    [Route("api/database")]
    public class DatabaseController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly IErrorLogService _errorLogService;
        public DatabaseController(IDatabaseService databaseService, IErrorLogService errorLogService)
        {
            _databaseService = databaseService;
            _errorLogService = errorLogService;
        }

        [HttpGet("tables-count")]
        public async Task<ActionResult<DataCountDTO>> GetTablesCountForDatabases()
        {
            try
            {
                return Ok(await _databaseService.GetTablesCountForDatabasesAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, _errorLogService.CreateErrorLogAsync(ERROR.GETTING_DATA_COUNT_FAILED));
            }
        }
    }
}