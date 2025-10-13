using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface IMongoDBRepository
    {
        Task<TablesCountDTO> GetTablesCountAsync();
        Task<List<int>> GetAllRoomIdsAsync();
    }
}