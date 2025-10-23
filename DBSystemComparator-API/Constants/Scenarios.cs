using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Constants
{
    public static class SCENARIOS
    {
        // CREATE
        public static readonly ScenarioDTO CREATE_1 = new ScenarioDTO
        {
            Id = 1,
            Description = "Adding a new client.",
            OperationId = (int)OPERATION.CREATE
        };
        public static readonly ScenarioDTO CREATE_2 = new ScenarioDTO
        {
            Id = 2,
            Description = "Adding a new room.",
            OperationId = (int)OPERATION.CREATE
        };
        public static readonly ScenarioDTO CREATE_3 = new ScenarioDTO
        {
            Id = 3,
            Description = "Adding a new service.",
            OperationId = (int)OPERATION.CREATE
        };
        public static readonly ScenarioDTO CREATE_4 = new ScenarioDTO
        {
            Id = 4,
            Description = "Adding 500 new clients asynchronously.",
            OperationId = (int)OPERATION.CREATE
        };
        public static readonly ScenarioDTO CREATE_5 = new ScenarioDTO
        {
            Id = 5,
            Description = "Adding 500 new rooms asynchronously.",
            OperationId = (int)OPERATION.CREATE
        };

        // READ
        public static readonly ScenarioDTO READ_1 = new ScenarioDTO
        {
            Id = 6,
            Description = "Selecting reservations with check-in dates after the second half of 2025.",
            OperationId = (int)OPERATION.READ
        };
        public static readonly ScenarioDTO READ_2 = new ScenarioDTO
        {
            Id = 7,
            Description = "Selecting reservations with payments above a specified amount above 4500.",
            OperationId = (int)OPERATION.READ
        };
        public static readonly ScenarioDTO READ_3 = new ScenarioDTO
        {
            Id = 8,
            Description = "Retrieving clients who currently have active reservations.",
            OperationId = (int)OPERATION.READ
        };
        public static readonly ScenarioDTO READ_4 = new ScenarioDTO
        {
            Id = 9,
            Description = "Selecting active services with price above 150 used in reservations .",
            OperationId = (int)OPERATION.READ
        };
        public static readonly ScenarioDTO READ_5 = new ScenarioDTO
        {
            Id = 10,
            Description = "Selecting reservations for rooms above 8 capacity.",
            OperationId = (int)OPERATION.READ
        };

        // UPDATE
        public static readonly ScenarioDTO UPDATE_1 = new ScenarioDTO
        {
            Id = 11,
            Description = "Updating address and phone number for active clients born after 2004.",
            OperationId = (int)OPERATION.UPDATE
        };
        public static readonly ScenarioDTO UPDATE_2 = new ScenarioDTO
        {
            Id = 12,
            Description = "Increasing price (150) for rooms with minimum 9 capacity that have reservations.",
            OperationId = (int)OPERATION.UPDATE
        };
        public static readonly ScenarioDTO UPDATE_3 = new ScenarioDTO
        {
            Id = 13,
            Description = "Increasing the price by 25 for active services for price above 180.",
            OperationId = (int)OPERATION.UPDATE
        };
        public static readonly ScenarioDTO UPDATE_4 = new ScenarioDTO
        {
            Id = 14,
            Description = "Applying a discount 20% to inactive rooms with price above 4400.",
            OperationId = (int)OPERATION.UPDATE
        };
        public static readonly ScenarioDTO UPDATE_5 = new ScenarioDTO
        {
            Id = 15,
            Description = "Reducing room rates by 15 for reservations to april 2024.",
            OperationId = (int)OPERATION.UPDATE
        };

        // DELETE
        public static readonly ScenarioDTO DELETE_1 = new ScenarioDTO
        {
            Id = 16,
            Description = "Deleting payments for reservations before march 2023.",
            OperationId = (int)OPERATION.DELETE
        };
        public static readonly ScenarioDTO DELETE_2 = new ScenarioDTO
        {
            Id = 17,
            Description = "Deleting payments with sum below 600.",
            OperationId = (int)OPERATION.DELETE
        };
        public static readonly ScenarioDTO DELETE_3 = new ScenarioDTO
        {
            Id = 18,
            Description = "Deleting reservation-service links for reservations before march 2023.",
            OperationId = (int)OPERATION.DELETE
        };
        public static readonly ScenarioDTO DELETE_4 = new ScenarioDTO
        {
            Id = 19,
            Description = "Deleting reservation-service rows for services below the price of 20.",
            OperationId = (int)OPERATION.DELETE
        };
        public static readonly ScenarioDTO DELETE_5 = new ScenarioDTO
        {
            Id = 20,
            Description = "Deleting services below 30 and not linked to any reservation.",
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