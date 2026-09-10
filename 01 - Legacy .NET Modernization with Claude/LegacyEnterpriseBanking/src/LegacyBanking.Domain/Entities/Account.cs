using System;
using System.Collections.Generic;

namespace LegacyBanking.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public int CustomerId { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTime OpenedOn { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
