using LegacyBanking.Domain.Entities;

namespace LegacyBanking.Data.Repositories
{
    public class AuditRepository
    {
        public void Add(AuditLog log)
        {
            using (var db = new BankingDbContext())
            {
                db.AuditLogs.Add(log);
                db.SaveChanges();
            }
        }
    }
}
