using System.Collections.Generic;
using System.Linq;
using LegacyBanking.Domain.Entities;

namespace LegacyBanking.Data.Repositories
{
    public class AccountRepository
    {
        public Account GetByAccountNumber(string accountNumber)
        {
            using (var db = new BankingDbContext())
            {
                return db.Accounts
                    .Include("Customer")
                    .FirstOrDefault(a => a.AccountNumber == accountNumber);
            }
        }

        public List<Account> GetByCustomer(int customerId)
        {
            using (var db = new BankingDbContext())
            {
                return db.Accounts
                    .Where(a => a.CustomerId == customerId)
                    .ToList();
            }
        }

        public void Update(Account account)
        {
            using (var db = new BankingDbContext())
            {
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}
