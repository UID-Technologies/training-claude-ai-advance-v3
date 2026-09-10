namespace LegacyBanking.Domain.Models
{
    public class TransferRequest
    {
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string RequestedBy { get; set; }
    }
}
