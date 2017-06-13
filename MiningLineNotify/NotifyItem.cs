namespace MiningLineNotify.Models
{
    public class NotifyItem
    {
        public string NotiFyType { get; set; }
        public string NotiFyMessate { get; set; }
        public double NotiFyPercentage { get; set; }
        public bool IsDanger { get; set; }
    }
}