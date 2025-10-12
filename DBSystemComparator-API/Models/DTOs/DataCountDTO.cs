namespace DBSystemComparator_API.Models.DTOs
{
    public class DataCountDTO
    {
        public TablesCountDTO PostgreSQL { get; set; }
        public TablesCountDTO SQLServer { get; set; }
        public TablesCountDTO MongoDB { get; set; }
        public TablesCountDTO Cassandra { get; set; }
    }
}