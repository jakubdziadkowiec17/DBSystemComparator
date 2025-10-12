using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface IDataCountRepository
    {
        Task<TablesCountDTO> GetTablesCountForPostgreSQLAsync();
        Task<TablesCountDTO> GetTablesCountForSQLServerAsync();
        Task<TablesCountDTO> GetTablesCountForMongoDBAsync();
        Task<TablesCountDTO> GetTablesCountForCassandraAsync();
    }
}