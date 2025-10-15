using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using DBSystemComparator_API.Services.Interfaces;

namespace DBSystemComparator_API.Services.Implementations
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IPostgreSQLRepository _postgreSQLRepository;
        private readonly ISQLServerRepository _sqlServerRepository;
        private readonly IMongoDBRepository _mongoDBRepository;
        private readonly ICassandraRepository _cassandraRepository;
        public DatabaseService(IPostgreSQLRepository postgreSQLRepository, ISQLServerRepository sqlServerRepository, IMongoDBRepository mongoDBRepository, ICassandraRepository cassandraRepository)
        {
            _postgreSQLRepository = postgreSQLRepository;
            _sqlServerRepository = sqlServerRepository;
            _mongoDBRepository = mongoDBRepository;
            _cassandraRepository = cassandraRepository;
        }

        public async Task<DataCountDTO> GetTablesCountForDatabasesAsync()
        {
            var tablesCountForPostgreSQL = await _postgreSQLRepository.GetTablesCountAsync();
            var tablesCountForSQLServer = await _sqlServerRepository.GetTablesCountAsync();
            var tablesCountForMongoDB = await _mongoDBRepository.GetTablesCountAsync();
            var tablesCountForCassandra = await _cassandraRepository.GetTablesCountAsync();

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