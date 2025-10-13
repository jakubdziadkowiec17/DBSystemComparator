namespace DBSystemComparator_API.Models.DTOs
{
    public class ChartDTO
    {
        public int Database { get; set; }
        public double Result { get; set; }

        public ChartDTO(int database, double result)
        {
            Database = database;
            Result = result;
        }
    }
}