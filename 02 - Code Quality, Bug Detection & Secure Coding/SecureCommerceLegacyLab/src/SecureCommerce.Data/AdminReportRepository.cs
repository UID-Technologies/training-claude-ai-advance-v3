using Microsoft.EntityFrameworkCore;
using SecureCommerce.Domain;
namespace SecureCommerce.Data;
public class AdminReportRepository {
    private readonly CommerceDbContext _db;
    public AdminReportRepository(CommerceDbContext db) { _db = db; }

    public Task<List<Order>> SearchOrdersRaw(string status, string fromDate) {
        // Intentional SQL injection vulnerability
        var sql = "SELECT * FROM Orders WHERE Status='" + status +
                  "' AND CreatedOn >= '" + fromDate + "'";
        return _db.Orders.FromSqlRaw(sql).ToListAsync();
    }
}
