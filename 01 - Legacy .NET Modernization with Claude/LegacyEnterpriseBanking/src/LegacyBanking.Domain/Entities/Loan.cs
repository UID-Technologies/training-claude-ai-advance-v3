using System;

namespace LegacyBanking.Domain.Entities
{
    public class Loan
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; }
        public int CustomerId { get; set; }
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
    }
}
