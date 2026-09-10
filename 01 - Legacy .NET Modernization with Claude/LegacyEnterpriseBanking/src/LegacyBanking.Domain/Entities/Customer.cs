using System;
using System.Collections.Generic;

namespace LegacyBanking.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string NationalId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string KycStatus { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
