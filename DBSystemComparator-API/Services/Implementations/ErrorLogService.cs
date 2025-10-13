using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Services.Interfaces;

namespace DBSystemComparator_API.Services.Implementations
{
    public class ErrorLogService : IErrorLogService
    {
        public ErrorLogService()
        {
        }

        public ResponseDTO CreateErrorLogAsync(string message)
        {
            return new ResponseDTO(message);
        }
    }
}