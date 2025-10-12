using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Models.Entities;
using DBSystemComparator_API.Repositories.Interfaces;
using DBSystemComparator_API.Services.Interfaces;
using System.Security.Claims;

namespace DBSystemComparator_API.Services.Implementations
{
    public class ErrorLogService : IErrorLogService
    {
        public ErrorLogService()
        {
        }

        public async Task<ResponseDTO?> CreateErrorLogAsync(string message)
        {
            try
            {
                return new ResponseDTO(message);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}