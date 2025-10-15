using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Constants
{
    public static class SCENARIOS
    {
        // CREATE
        public static readonly ScenarioDTO CREATE_1 = new ScenarioDTO
        {
            Id = 1,
            Description = "Adding a new active client named Jan Kowalski to the Clients table.",
            OperationId = (int)OPERATION.CREATE
        };
        public static readonly ScenarioDTO CREATE_2 = new ScenarioDTO
        {
            Id = 2,
            Description = "Adding a new active room with number 100 and price 400 to the Rooms table.",
            OperationId = (int)OPERATION.CREATE
        };
        public static readonly ScenarioDTO CREATE_3 = new ScenarioDTO
        {
            Id = 3,
            Description = "Adding a new active cleaning service to the Services table.",
            OperationId = (int)OPERATION.CREATE
        };
        public static readonly ScenarioDTO CREATE_4 = new ScenarioDTO
        {
            Id = 4,
            Description = "Adding a new inactive client named Kamil Nowak to the Clients table.",
            OperationId = (int)OPERATION.CREATE
        };
        public static readonly ScenarioDTO CREATE_5 = new ScenarioDTO
        {
            Id = 5,
            Description = "Adding a new inactive room with number 200 and price 800 to the Rooms table.",
            OperationId = (int)OPERATION.CREATE
        };

        // READ
        public static readonly ScenarioDTO READ_1 = new ScenarioDTO
        {
            Id = 6,
            Description = "Retrieving a list of active clients and their associated active rooms.",
            OperationId = (int)OPERATION.READ
        };
        public static readonly ScenarioDTO READ_2 = new ScenarioDTO
        {
            Id = 7,
            Description = "Retrieving rooms along with the number of reservations made for each.",
            OperationId = (int)OPERATION.READ
        };
        public static readonly ScenarioDTO READ_3 = new ScenarioDTO
        {
            Id = 8,
            Description = "Retrieving services with their prices and number of uses, sorted in descending order.",
            OperationId = (int)OPERATION.READ
        };
        public static readonly ScenarioDTO READ_4 = new ScenarioDTO
        {
            Id = 9,
            Description = "Retrieving payments with an amount greater than 400 along with client details and room numbers.",
            OperationId = (int)OPERATION.READ
        };
        public static readonly ScenarioDTO READ_5 = new ScenarioDTO
        {
            Id = 10,
            Description = "Retrieving reservations with active clients and active services.",
            OperationId = (int)OPERATION.READ
        };

        // UPDATE
        public static readonly ScenarioDTO UPDATE_1 = new ScenarioDTO
        {
            Id = 11,
            Description = "Updating the address and phone number for 200 active clients.",
            OperationId = (int)OPERATION.UPDATE
        };
        public static readonly ScenarioDTO UPDATE_2 = new ScenarioDTO
        {
            Id = 12,
            Description = "Updating the price of rooms with a capacity of at least 2 that have reservations.",
            OperationId = (int)OPERATION.UPDATE
        };
        public static readonly ScenarioDTO UPDATE_3 = new ScenarioDTO
        {
            Id = 13,
            Description = "Updating the price of active services by increasing it by 25.",
            OperationId = (int)OPERATION.UPDATE
        };
        public static readonly ScenarioDTO UPDATE_4 = new ScenarioDTO
        {
            Id = 14,
            Description = "Updating the price of inactive rooms by decreasing it by 20%.",
            OperationId = (int)OPERATION.UPDATE
        };
        public static readonly ScenarioDTO UPDATE_5 = new ScenarioDTO
        {
            Id = 15,
            Description = "Updating the price of rooms with upcoming reservations by reducing it by 15.",
            OperationId = (int)OPERATION.UPDATE
        };

        // DELETE
        public static readonly ScenarioDTO DELETE_1 = new ScenarioDTO
        {
            Id = 16,
            Description = "Deleting reservations for rooms with a capacity less than 2 and with future check-in dates.",
            OperationId = (int)OPERATION.DELETE
        };
        public static readonly ScenarioDTO DELETE_2 = new ScenarioDTO
        {
            Id = 17,
            Description = "Deleting service associations for the 200 closest upcoming reservations.",
            OperationId = (int)OPERATION.DELETE
        };
        public static readonly ScenarioDTO DELETE_3 = new ScenarioDTO
        {
            Id = 18,
            Description = "Deleting reservations for which no payments exist.",
            OperationId = (int)OPERATION.DELETE
        };
        public static readonly ScenarioDTO DELETE_4 = new ScenarioDTO
        {
            Id = 19,
            Description = "Deleting inactive clients without any assigned reservations.",
            OperationId = (int)OPERATION.DELETE
        };
        public static readonly ScenarioDTO DELETE_5 = new ScenarioDTO
        {
            Id = 20,
            Description = "Deleting inactive rooms without any assigned reservations.",
            OperationId = (int)OPERATION.DELETE
        };

        // ALL
        public static readonly List<ScenarioDTO> ALL = new()
        {
            CREATE_1, CREATE_2, CREATE_3, CREATE_4, CREATE_5,
            READ_1, READ_2, READ_3, READ_4, READ_5,
            UPDATE_1, UPDATE_2, UPDATE_3, UPDATE_4, UPDATE_5,
            DELETE_1, DELETE_2, DELETE_3, DELETE_4, DELETE_5
        };
    }
}