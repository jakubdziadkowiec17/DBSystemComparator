using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Services.Interfaces
{
    public interface IDataCountService
    {
        Task<DataCountDTO> GetDataCountAsync();
    }
}