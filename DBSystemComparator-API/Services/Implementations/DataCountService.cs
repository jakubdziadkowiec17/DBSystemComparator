using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using DBSystemComparator_API.Services.Interfaces;

namespace DBSystemComparator_API.Services.Implementations
{
    public class DataCountService : IDataCountService
    {
        private readonly IDataCountRepository _dataCountRepository;
        public DataCountService(IDataCountRepository dataCountRepository)
        {
            _dataCountRepository = dataCountRepository;
        }

        public async Task<DataCountDTO> GetDataCountAsync()
        {
            var tablesCountForSQLServer = await _dataCountRepository.GetTablesCountForSQLServerAsync();
            var tablesCountForPostgreSQL = await _dataCountRepository.GetTablesCountForPostgreSQLAsync();
            var tablesCountForMongoDB = await _dataCountRepository.GetTablesCountForMongoDBAsync();
            var tablesCountForCassandra = await _dataCountRepository.GetTablesCountForCassandraAsync();

            return new DataCountDTO()
            {
                PostgreSQL = tablesCountForPostgreSQL,
                SQLServer = tablesCountForSQLServer,
                MongoDB = tablesCountForMongoDB,
                Cassandra = tablesCountForCassandra
            };
        }
    }
}