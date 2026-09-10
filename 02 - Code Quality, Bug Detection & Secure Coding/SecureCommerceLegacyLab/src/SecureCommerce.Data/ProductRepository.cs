using SecureCommerce.Domain;
namespace SecureCommerce.Data;
public class ProductRepository {
    private readonly CommerceDbContext _db;
    public ProductRepository(CommerceDbContext db) { _db = db; }

    public List<Product> Search(string text) {
        return _db.Products.Where(p => p.IsActive).ToList()
            .Where(p => p.Name.ToLower().Contains(text.ToLower())).ToList();
    }

    public Product Get(int id) => _db.Products.FirstOrDefault(x => x.Id == id);
    public void Update(Product p) { _db.Products.Update(p); _db.SaveChanges(); }
}
