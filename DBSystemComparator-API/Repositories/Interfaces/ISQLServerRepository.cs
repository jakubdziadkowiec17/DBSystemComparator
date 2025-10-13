using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface ISQLServerRepository
    {
        Task<TablesCountDTO> GetTablesCountAsync();
        Task<List<int>> GetAllRoomIdsAsync();
    }
}