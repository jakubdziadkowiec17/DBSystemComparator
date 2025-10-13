using DBSystemComparator_API.Constants;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using DBSystemComparator_API.Services.Interfaces;
using System.Diagnostics;

namespace DBSystemComparator_API.Services.Implementations
{
    public class ScenarioService : IScenarioService
    {
        private readonly IPostgreSQLRepository _postgreSQLRepository;
        private readonly ISQLServerRepository _sqlServerRepository;
        private readonly IMongoDBRepository _mongoDBRepository;
        private readonly ICassandraRepository _cassandraRepository;
        public ScenarioService(IPostgreSQLRepository postgreSQLRepository, ISQLServerRepository sqlServerRepository, IMongoDBRepository mongoDBRepository, ICassandraRepository cassandraRepository)
        {
            _postgreSQLRepository = postgreSQLRepository;
            _sqlServerRepository = sqlServerRepository;
            _mongoDBRepository = mongoDBRepository;
            _cassandraRepository = cassandraRepository;
        }

        public async Task<MetricsDTO> CheckScenarioAsync(SelectedScenarioDTO selectedScenarioDTO)
        {
            var scenarios = Scenarios.ALL;
            var selectedScenario = scenarios.FirstOrDefault(a => a.Id == selectedScenarioDTO.Id && a.Sizes.Contains(selectedScenarioDTO.Size));
            if (selectedScenario is null) throw new Exception(ERROR.SELECTED_INCORRECT_SCENARIO);

            var time_postgreSQL = new ChartDTO((int)Db.POSTGRESQL, 0);
            var time_sqlServer = new ChartDTO((int)Db.SQLSERVER, 0);
            var time_mongoDB = new ChartDTO((int)Db.MONGODB, 0);
            var time_cassandra = new ChartDTO((int)Db.CASSANDRA, 0);

            var averageTime_postgreSQL = new ChartDTO((int)Db.POSTGRESQL, 0);
            var averageTime_sqlServer = new ChartDTO((int)Db.SQLSERVER, 0);
            var averageTime_mongoDB = new ChartDTO((int)Db.MONGODB, 0);
            var averageTime_cassandra = new ChartDTO((int)Db.CASSANDRA, 0);

            var capacity_postgreSQL = new ChartDTO((int)Db.POSTGRESQL, 0);
            var capacity_sqlServer = new ChartDTO((int)Db.SQLSERVER, 0);
            var capacity_mongoDB = new ChartDTO((int)Db.MONGODB, 0);
            var capacity_cassandra = new ChartDTO((int)Db.CASSANDRA, 0);

            var ramUsage_postgreSQL = new ChartDTO((int)Db.POSTGRESQL, 0);
            var ramUsage_sqlServer = new ChartDTO((int)Db.SQLSERVER, 0);
            var ramUsage_mongoDB = new ChartDTO((int)Db.MONGODB, 0);
            var ramUsage_cassandra = new ChartDTO((int)Db.CASSANDRA, 0);

            var cpuUsage_postgreSQL = new ChartDTO((int)Db.POSTGRESQL, 0);
            var cpuUsage_sqlServer = new ChartDTO((int)Db.SQLSERVER, 0);
            var cpuUsage_mongoDB = new ChartDTO((int)Db.MONGODB, 0);
            var cpuUsage_cassandra = new ChartDTO((int)Db.CASSANDRA, 0);

            async Task GetMetricsAsync(Func<Task> action, ChartDTO timeChartDTO, ChartDTO averageTimeChartDTO, ChartDTO capacityChartDTO, ChartDTO ramChartDTO, ChartDTO cpuChartDTO, int size)
            {
                double totalRam = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

                long ramBefore = GC.GetTotalMemory(true);
                var cpuBefore = Process.GetCurrentProcess().TotalProcessorTime;
                var stopwatch = Stopwatch.StartNew();

                for (int i = 0; i < size; i++)
                {
                    await action();
                }

                stopwatch.Stop();
                long ramAfter = GC.GetTotalMemory(false);
                var cpuAfter = Process.GetCurrentProcess().TotalProcessorTime;

                double time = stopwatch.Elapsed.TotalSeconds;
                double averageTime = size > 0 ? time / size : 0;
                double capacity = time > 0 ? size / time : 0;

                double ramDelta = ramAfter - ramBefore;
                double ramUsage = (ramAfter / totalRam) * 100.0;

                var cpuDelta = (cpuAfter - cpuBefore).TotalSeconds;
                double cpuUsage = time > 0 ? (cpuDelta / (time * Environment.ProcessorCount)) * 100.0 : 0;

                timeChartDTO.Result = Math.Round(time, 2);
                averageTimeChartDTO.Result = Math.Round(averageTime, 6);
                capacityChartDTO.Result = Math.Round(capacity, 2);
                ramChartDTO.Result = Math.Round(ramUsage, 2);
                cpuChartDTO.Result = Math.Round(cpuUsage, 2);
            }

            if (selectedScenario.OperationId == (int)Operation.CREATE)
            {

            }
            else if (selectedScenario.OperationId == (int)Operation.READ)
            {
                if (selectedScenario.Id == 6)
                {
                    await GetMetricsAsync(
                        async () => await _postgreSQLRepository.GetAllRoomIdsAsync(),
                        time_postgreSQL, averageTime_postgreSQL, capacity_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL,
                        selectedScenarioDTO.Size);

                    await GetMetricsAsync(
                        async () => await _sqlServerRepository.GetAllRoomIdsAsync(),
                        time_sqlServer, averageTime_sqlServer, capacity_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer,
                        selectedScenarioDTO.Size);

                    await GetMetricsAsync(
                        async () => await _mongoDBRepository.GetAllRoomIdsAsync(),
                        time_mongoDB, averageTime_mongoDB, capacity_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB,
                        selectedScenarioDTO.Size);

                    await GetMetricsAsync(
                        async () => await _cassandraRepository.GetAllRoomIdsAsync(),
                        time_cassandra, averageTime_cassandra, capacity_cassandra, ramUsage_cassandra, cpuUsage_cassandra,
                        selectedScenarioDTO.Size);
                }
            }
            else if (selectedScenario.OperationId == (int)Operation.UPDATE)
            {

            }
            else if (selectedScenario.OperationId == (int)Operation.DELETE)
            {

            }

            var time = new List<ChartDTO>() { time_postgreSQL, time_sqlServer, time_mongoDB, time_cassandra };
            var averageTime = new List<ChartDTO>() { averageTime_postgreSQL, averageTime_sqlServer, averageTime_mongoDB, averageTime_cassandra };
            var capacity = new List<ChartDTO>() { capacity_postgreSQL, capacity_sqlServer, capacity_mongoDB, capacity_cassandra };
            var ramUsage = new List<ChartDTO>() { ramUsage_postgreSQL, ramUsage_sqlServer, ramUsage_mongoDB, ramUsage_cassandra };
            var cpuUsage = new List<ChartDTO>() { cpuUsage_postgreSQL, cpuUsage_sqlServer, cpuUsage_mongoDB, cpuUsage_cassandra };

            return new MetricsDTO()
            {
                Time = time,
                AverageTime = averageTime,
                Capacity = capacity,
                RAMUsage = ramUsage,
                CPUUsage = cpuUsage
            };
        }

        public List<ScenarioDTO> GetSceanarios()
        {
            return Scenarios.ALL;
        }
    }
}