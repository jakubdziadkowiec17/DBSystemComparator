namespace DBSystemComparator_API.Models.Domain
{
    public class UrlPrediction
    {
        public bool PredictedLabel { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}