using System.Collections.Generic;
using System.Linq;
using LegacyBanking.Domain.Entities;

namespace LegacyBanking.Data.Repositories
{
    public class CustomerRepository
    {
        public Customer GetById(int id)
        {
            // Legacy issue: context not disposed.
            var db = new BankingDbContext();
            return db.Customers.FirstOrDefault(c => c.Id == id);
        }

        public List<Customer> SearchByName(string searchText)
        {
            using (var db = new BankingDbContext())
            {
                // Legacy issue: entire table is materialized first.
                var customers = db.Customers.ToList();

                return customers
                    .Where(c => c.FullName != null &&
                                c.FullName.ToLower().Contains(searchText.ToLower()))
                    .ToList();
            }
        }

        public void Save(Customer customer)
        {
            using (var db = new BankingDbContext())
            {
                if (customer.Id == 0)
                    db.Customers.Add(customer);
                else
                    db.Entry(customer).State = EntityState.Modified;

                db.SaveChanges();
            }
        }
    }
}
