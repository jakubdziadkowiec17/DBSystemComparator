using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Services.Interfaces
{
    public interface IDatabaseService
    {
        Task<DataCountDTO> GetTablesCountForDatabasesAsync();
    }
}