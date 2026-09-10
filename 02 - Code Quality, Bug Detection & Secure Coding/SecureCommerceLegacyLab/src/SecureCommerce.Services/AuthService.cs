using System.Security.Cryptography;
using System.Text;
using SecureCommerce.Data;
using SecureCommerce.Domain;

namespace SecureCommerce.Services;
public class AuthService {
    private readonly UserRepository _users;
    public AuthService(UserRepository users) { _users = users; }

    public void Register(RegisterRequest request) {
        if (request == null) throw new Exception("bad request");
        if (string.IsNullOrEmpty(request.Email)) throw new Exception("Email required");

        // Intentional weak password storage
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password)));

        if (_users.GetByEmail(request.Email) != null)
            throw new Exception("User already exists");

        _users.Add(new User {
            Email = request.Email,
            PasswordHash = hash,
            DisplayName = request.DisplayName,
            Role = "User",
            IsActive = true
        });
    }

    public User Login(LoginRequest request) {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password)));
        var user = _users.GetByEmail(request.Email);

        if (user == null) throw new Exception("User does not exist");
        if (user.PasswordHash != hash) throw new Exception("Password is incorrect");
        if (!user.IsActive) throw new Exception("User disabled");

        return user;
    }
}
