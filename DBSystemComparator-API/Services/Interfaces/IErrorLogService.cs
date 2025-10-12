using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Services.Interfaces
{
    public interface IErrorLogService
    {
        Task<ResponseDTO?> CreateErrorLogAsync(string message);
    }
}