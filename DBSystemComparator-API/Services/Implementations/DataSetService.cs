using DBSystemComparator_API.Constants;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using DBSystemComparator_API.Services.Interfaces;
using System.Security.Claims;

namespace DBSystemComparator_API.Services.Implementations
{
    public class DataSetService : IDataSetService
    {
        private readonly IDataSetRepository _dataSetRepository;
        public DataSetService(IDataSetRepository dataSetRepository)
        {
            _dataSetRepository = dataSetRepository;
        }

        //public async Task<ResponseDTO> UploadAsync(DataSetDTO dataSetDTO)
        //{
        //    var user = _httpContextAccessor.HttpContext?.User;
        //    var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (dataSetDTO is null || dataSetDTO.File is null)
        //    {
        //        throw new Exception(ERROR.GETTING_ACCOUNT_DATA_FAILED);
        //    }

        //    var modelFolder = "MLModels";
        //    Directory.CreateDirectory(modelFolder);

        //    return new ResponseDTO(SUCCESS.DATA_SET_ADDED);
        //}
    }
}