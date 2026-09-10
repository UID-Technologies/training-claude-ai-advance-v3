using System.Data.Entity;
using LegacyBanking.Domain.Entities;

namespace LegacyBanking.Data
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext() : base("name=BankingDb")
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
