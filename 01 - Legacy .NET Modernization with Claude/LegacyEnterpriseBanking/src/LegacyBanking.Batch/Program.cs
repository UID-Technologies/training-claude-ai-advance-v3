using System;
using System.Linq;
using LegacyBanking.Data;

namespace LegacyBanking.Batch
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(
                "Nightly interest batch started at " +
                DateTime.Now);

            using (var db = new BankingDbContext())
            {
                // Legacy issue:
                // materializes all active accounts.
                var accounts =
                    db.Accounts
                        .Where(a => a.Status == "ACTIVE")
                        .ToList();

                foreach (var account in accounts)
                {
                    if (account.AccountType == "SAVINGS")
                    {
                        account.Balance +=
                            account.Balance * 0.0001m;

                        // Legacy issue:
                        // DB round trip per record.
                        db.SaveChanges();
                    }
                }
            }

            Console.WriteLine(
                "Nightly interest batch completed at " +
                DateTime.Now);
        }
    }
}
