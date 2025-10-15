namespace DBSystemComparator_API.Models.DTOs
{
    public class MetricsDTO
    {
        public List<ChartDTO>? Time { get; set; }
        public List<ChartDTO>? RAMUsage { get; set; }
        public List<ChartDTO>? CPUUsage { get; set; }
    }
}