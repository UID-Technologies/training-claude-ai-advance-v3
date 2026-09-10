using System;

namespace LegacyBanking.Domain.Entities
{
    public class Transaction
    {
        public long Id { get; set; }
        public string Reference { get; set; }
        public int AccountId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }

        public virtual Account Account { get; set; }
    }
}
