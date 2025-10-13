using DBSystemComparator_API.Constants;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DBSystemComparator_API.Controllers
{
    [ApiController]
    [Route("api/data-count")]
    public class DataCountController : ControllerBase
    {
        private readonly IDataCountService _dataCountService;
        private readonly IErrorLogService _errorLogService;
        public DataCountController(IDataCountService dataCountService, IErrorLogService errorLogService)
        {
            _dataCountService = dataCountService;
            _errorLogService = errorLogService;
        }

        [HttpGet]
        public async Task<ActionResult<DataCountDTO>> GetDataCount()
        {
            try
            {
                return Ok(await _dataCountService.GetDataCountAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, _errorLogService.CreateErrorLogAsync(ERROR.GETTING_DATA_COUNT_FAILED));
            }
        }
    }
}