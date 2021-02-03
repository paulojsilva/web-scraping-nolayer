namespace Domain.Shared.Configuration
{
    public class ParallelismSettings
    {
        public int MaxDegreeOfParallelism { get; set; }
        public int MaxHttpRequestInParallel { get; set; }
        public int IncreaseDelayGoSlowly { get; set; }
    }
}
