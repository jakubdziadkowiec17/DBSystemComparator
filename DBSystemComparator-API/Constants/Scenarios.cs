using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Constants
{
    public static class Scenarios
    {
        private static readonly List<int> sizes = new List<int>() { 10000, 100000, 1000000 };

        public static readonly ScenarioDTO CREATE_1 = new ScenarioDTO { Id = 1, Description = "a", OperationId = (int)Operation.CREATE, Sizes = sizes };
        public static readonly ScenarioDTO CREATE_2 = new ScenarioDTO { Id = 2, Description = "b", OperationId = (int)Operation.CREATE, Sizes = sizes };
        public static readonly ScenarioDTO CREATE_3 = new ScenarioDTO { Id = 3, Description = "c", OperationId = (int)Operation.CREATE, Sizes = sizes };
        public static readonly ScenarioDTO CREATE_4 = new ScenarioDTO { Id = 4, Description = "d", OperationId = (int)Operation.CREATE, Sizes = sizes };
        public static readonly ScenarioDTO CREATE_5 = new ScenarioDTO { Id = 5, Description = "e", OperationId = (int)Operation.CREATE, Sizes = sizes };

        public static readonly ScenarioDTO READ_1 = new ScenarioDTO { Id = 6, Description = "", OperationId = (int)Operation.READ, Sizes = sizes };
        public static readonly ScenarioDTO READ_2 = new ScenarioDTO { Id = 7, Description = "", OperationId = (int)Operation.READ, Sizes = sizes };
        public static readonly ScenarioDTO READ_3 = new ScenarioDTO { Id = 8, Description = "", OperationId = (int)Operation.READ, Sizes = sizes };
        public static readonly ScenarioDTO READ_4 = new ScenarioDTO { Id = 9, Description = "", OperationId = (int)Operation.READ, Sizes = sizes };
        public static readonly ScenarioDTO READ_5 = new ScenarioDTO { Id = 10, Description = "", OperationId = (int)Operation.READ, Sizes = sizes };

        public static readonly ScenarioDTO UPDATE_1 = new ScenarioDTO { Id = 11, Description = "", OperationId = (int)Operation.UPDATE, Sizes = sizes };
        public static readonly ScenarioDTO UPDATE_2 = new ScenarioDTO { Id = 12, Description = "", OperationId = (int)Operation.UPDATE, Sizes = sizes };
        public static readonly ScenarioDTO UPDATE_3 = new ScenarioDTO { Id = 13, Description = "", OperationId = (int)Operation.UPDATE, Sizes = sizes };
        public static readonly ScenarioDTO UPDATE_4 = new ScenarioDTO { Id = 14, Description = "", OperationId = (int)Operation.UPDATE, Sizes = sizes };
        public static readonly ScenarioDTO UPDATE_5 = new ScenarioDTO { Id = 15, Description = "", OperationId = (int)Operation.UPDATE, Sizes = sizes };

        public static readonly ScenarioDTO DELETE_1 = new ScenarioDTO { Id = 16, Description = "", OperationId = (int)Operation.DELETE, Sizes = sizes };
        public static readonly ScenarioDTO DELETE_2 = new ScenarioDTO { Id = 17, Description = "", OperationId = (int)Operation.DELETE, Sizes = sizes };
        public static readonly ScenarioDTO DELETE_3 = new ScenarioDTO { Id = 18, Description = "", OperationId = (int)Operation.DELETE, Sizes = sizes };
        public static readonly ScenarioDTO DELETE_4 = new ScenarioDTO { Id = 19, Description = "", OperationId = (int)Operation.DELETE, Sizes = sizes };
        public static readonly ScenarioDTO DELETE_5 = new ScenarioDTO { Id = 20, Description = "", OperationId = (int)Operation.DELETE, Sizes = sizes };

        public static readonly List<ScenarioDTO> ALL = new()
        {
            CREATE_1, CREATE_2, CREATE_3, CREATE_4, CREATE_5,
            READ_1, READ_2, READ_3, READ_4, READ_5,
            UPDATE_1, UPDATE_2, UPDATE_3, UPDATE_4, UPDATE_5,
            DELETE_1, DELETE_2, DELETE_3, DELETE_4, DELETE_5
        };
    }
}