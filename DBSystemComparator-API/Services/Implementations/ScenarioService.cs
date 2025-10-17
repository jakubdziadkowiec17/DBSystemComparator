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

        public async Task<MetricsDTO> TestScenarioAsync(SelectedScenarioDTO selectedScenarioDTO)
        {
            var scenarios = SCENARIOS.ALL;
            var selectedScenario = scenarios.FirstOrDefault(a => a.Id == selectedScenarioDTO.Id);
            if (selectedScenario is null) throw new Exception(ERROR.SELECTED_INCORRECT_SCENARIO);

            var time_postgreSQL = new ChartDTO((int)DB.POSTGRESQL, 0);
            var ramUsage_postgreSQL = new ChartDTO((int)DB.POSTGRESQL, 0);
            var cpuUsage_postgreSQL = new ChartDTO((int)DB.POSTGRESQL, 0);

            var time_sqlServer = new ChartDTO((int)DB.SQLSERVER, 0);
            var ramUsage_sqlServer = new ChartDTO((int)DB.SQLSERVER, 0);
            var cpuUsage_sqlServer = new ChartDTO((int)DB.SQLSERVER, 0);

            var time_mongoDB = new ChartDTO((int)DB.MONGODB, 0);
            var ramUsage_mongoDB = new ChartDTO((int)DB.MONGODB, 0);
            var cpuUsage_mongoDB = new ChartDTO((int)DB.MONGODB, 0);

            var time_cassandra = new ChartDTO((int)DB.CASSANDRA, 0);
            var ramUsage_cassandra = new ChartDTO((int)DB.CASSANDRA, 0);
            var cpuUsage_cassandra = new ChartDTO((int)DB.CASSANDRA, 0);

            async Task GetMetricsAsync(Func<Task> action, ChartDTO timeChartDTO, ChartDTO ramChartDTO, ChartDTO cpuChartDTO)
            {
                long ramBefore = GC.GetTotalMemory(true);
                var cpuBefore = Process.GetCurrentProcess().TotalProcessorTime;
                var stopwatch = Stopwatch.StartNew();

                await action();

                stopwatch.Stop();
                long ramAfter = GC.GetTotalMemory(false);
                var cpuAfter = Process.GetCurrentProcess().TotalProcessorTime;

                double time = stopwatch.Elapsed.TotalSeconds;

                double ramDelta = ramAfter - ramBefore;
                double ramUsage = (ramAfter - ramBefore) / (1024.0 * 1024.0);

                var cpuDelta = (cpuAfter - cpuBefore).TotalSeconds;
                double cpuUsage = time > 0 ? (cpuDelta / (time * Environment.ProcessorCount)) * 100.0 : 0;

                timeChartDTO.Result = Math.Round(time, 2);
                ramChartDTO.Result = Math.Round(ramUsage, 2);
                cpuChartDTO.Result = Math.Round(cpuUsage, 2);
            }

            // CREATE
            if (selectedScenario.Id == SCENARIOS.CREATE_1.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.CreateClientAsync("Jan", "Kamil", "Kowalski", "abcdeabcde@abcdeabcde.com", new DateTime(2000, 1, 1), "Cracow, abc 8", "123456789", true),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.CreateClientAsync("Jan", "Kamil", "Kowalski", "abcdeabcde@abcdeabcde.com", new DateTime(2000, 1, 1), "Cracow, abc 8", "123456789", true),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.CreateClientAsync("Jan", "Kamil", "Kowalski", "abcdeabcde@abcdeabcde.com", new DateTime(2000, 1, 1), "Cracow, abc 8", "123456789", true),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.CreateClientAsync("Jan", "Kamil", "Kowalski", "abcdeabcde@abcdeabcde.com", new DateTime(2000, 1, 1), "Cracow, abc 8", "123456789", true),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.CREATE_2.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.CreateRoomAsync(100, 2, 400, true),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.CreateRoomAsync(100, 2, 400, true),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.CreateRoomAsync(100, 2, 400, true),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.CreateRoomAsync(100, 2, 400, true),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.CREATE_3.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.CreateServiceAsync("Room cleaning", 70, true),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.CreateServiceAsync("Room cleaning", 70, true),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.CreateServiceAsync("Room cleaning", 70, true),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.CreateServiceAsync("Room cleaning", 70, true),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.CREATE_4.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.CreateClientsAsync("Kamil", "Jan", "Nowak", "abcdeabcde2@abcdeabcde.com", new DateTime(1998, 1, 1), "Cracow, abc 10", "123456789", false, 500),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.CreateClientsAsync("Kamil", "Jan", "Nowak", "abcdeabcde2@abcdeabcde.com", new DateTime(1998, 1, 1), "Cracow, abc 10", "123456789", false, 500),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.CreateClientAsync("Kamil", "Jan", "Nowak", "abcdeabcde2@abcdeabcde.com", new DateTime(1998, 1, 1), "Cracow, abc 10", "123456789", false),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.CreateClientAsync("Kamil", "Jan", "Nowak", "abcdeabcde2@abcdeabcde.com", new DateTime(1998, 1, 1), "Cracow, abc 10", "123456789", false),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.CREATE_5.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.CreateRoomsAsync(200, 4, 800, false, 500),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.CreateRoomsAsync(200, 4, 800, false, 500),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.CreateRoomAsync(200, 4, 800, false),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.CreateRoomAsync(200, 4, 800, false),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            // READ
            else if (selectedScenario.Id == SCENARIOS.READ_1.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.ReadReservationsAfter2024Async(),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.ReadReservationsAfter2024Async(),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.ReadClientsWithRoomsAsync(true),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.ReadClientsWithRoomsAsync(true),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.READ_2.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.ReadReservationsWithPaymentsAboveAsync(4400),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.ReadReservationsWithPaymentsAboveAsync(4400),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.ReadRoomsWithReservationCountAsync(),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.ReadRoomsWithReservationCountAsync(),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.READ_3.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.ReadClientsWithActiveReservationsAsync(),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.ReadClientsWithActiveReservationsAsync(),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.ReadServicesUsageAsync(),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.ReadServicesUsageAsync(),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.READ_4.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.ReadActiveServicesUsedInReservationsAsync(),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.ReadActiveServicesUsedInReservationsAsync(),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.ReadPaymentsAboveAsync(400),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.ReadPaymentsAboveAsync(400),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.READ_5.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.ReadCapacityReservationsAsync(7),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.ReadCapacityReservationsAsync(7),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.ReadReservationsWithServicesAsync(true, true),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.ReadReservationsWithServicesAsync(true, true),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            // UPDATE
            else if (selectedScenario.Id == SCENARIOS.UPDATE_1.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.UpdateClientsAddressAndPhoneAsync(true),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.UpdateClientsAddressAndPhoneAsync(true),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.UpdateClientsAddressPhoneAsync(true),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.UpdateClientsAddressPhoneAsync(true),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.UPDATE_2.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.UpdateRoomsPriceForReservationsAsync(2, 150),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.UpdateRoomsPriceForReservationsAsync(2, 150),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.UpdateRoomsPriceJoinReservationsAsync(2),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.UpdateRoomsPriceJoinReservationsAsync(2),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.UPDATE_3.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.UpdateServicesPriceAsync(25, true),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.UpdateServicesPriceAsync(25, true),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.UpdateServicesPriceAsync(true),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.UpdateServicesPriceAsync(true),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.UPDATE_4.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.UpdatePriceForInactiveRoomsAsync(0.8),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.UpdatePriceForInactiveRoomsAsync(0.8),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.UpdateRoomsPriceInactiveAsync(),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.UpdateRoomsPriceInactiveAsync(),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.UPDATE_5.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.UpdateRoomsPriceForReservationsTo2024Async(15),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.UpdateRoomsPriceForReservationsTo2024Async(15),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.UpdateRoomsPriceFutureReservationsAsync(),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.UpdateRoomsPriceFutureReservationsAsync(),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            // DELETE
            else if (selectedScenario.Id == SCENARIOS.DELETE_1.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.DeletePaymentsOlderThan2024Async(),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.DeletePaymentsOlderThan2024Async(),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.DeleteReservationsSmallRoomsAsync(2),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.DeleteReservationsSmallRoomsAsync(2),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.DELETE_2.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.DeletePaymentsWithoutReservationAsync(),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.DeletePaymentsWithoutReservationAsync(),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.DeleteReservationsServicesFutureAsync(200),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.DeleteReservationsServicesFutureAsync(200),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.DELETE_3.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.DeleteReservationsServicesOlderThan2024Async(),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.DeleteReservationsServicesOlderThan2024Async(),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.DeleteReservationsWithoutPaymentsAsync(),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.DeleteReservationsWithoutPaymentsAsync(),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.DELETE_4.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.DeleteReservationsServicesWithServicePriceBelowAsync(100),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.DeleteReservationsServicesWithServicePriceBelowAsync(100),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.DeleteInactiveClientsWithoutReservationsAsync(),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.DeleteInactiveClientsWithoutReservationsAsync(),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }
            else if (selectedScenario.Id == SCENARIOS.DELETE_5.Id)
            {
                await GetMetricsAsync(
                    async () => await _postgreSQLRepository.DeleteUnusedServicesAsync(),
                    time_postgreSQL, ramUsage_postgreSQL, cpuUsage_postgreSQL);

                await GetMetricsAsync(
                    async () => await _sqlServerRepository.DeleteUnusedServicesAsync(),
                    time_sqlServer, ramUsage_sqlServer, cpuUsage_sqlServer);

                //await GetMetricsAsync(
                //    async () => await _mongoDBRepository.DeleteRoomsWithoutReservationsAsync(),
                //    time_mongoDB, ramUsage_mongoDB, cpuUsage_mongoDB);

                //await GetMetricsAsync(
                //    async () => await _cassandraRepository.DeleteRoomsWithoutReservationsAsync(),
                //    time_cassandra, ramUsage_cassandra, cpuUsage_cassandra);
            }

            var time = new List<ChartDTO>() { time_postgreSQL, time_sqlServer, time_mongoDB, time_cassandra };
            var ramUsage = new List<ChartDTO>() { ramUsage_postgreSQL, ramUsage_sqlServer, ramUsage_mongoDB, ramUsage_cassandra };
            var cpuUsage = new List<ChartDTO>() { cpuUsage_postgreSQL, cpuUsage_sqlServer, cpuUsage_mongoDB, cpuUsage_cassandra };

            return new MetricsDTO()
            {
                Time = time,
                RAMUsage = ramUsage,
                CPUUsage = cpuUsage
            };
        }

        public List<ScenarioDTO> GetSceanarios()
        {
            return SCENARIOS.ALL;
        }
    }
}