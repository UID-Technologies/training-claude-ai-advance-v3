using SecureCommerce.Domain;
namespace SecureCommerce.Data;
public class UserRepository {
    private readonly CommerceDbContext _db;
    public UserRepository(CommerceDbContext db) { _db = db; }
    public User GetByEmail(string email) => _db.Users.FirstOrDefault(x => x.Email == email);
    public void Add(User user) { _db.Users.Add(user); _db.SaveChanges(); }
}
