using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Services.Interfaces
{
    public interface IErrorLogService
    {
        ResponseDTO CreateErrorLogAsync(string message);
    }
}