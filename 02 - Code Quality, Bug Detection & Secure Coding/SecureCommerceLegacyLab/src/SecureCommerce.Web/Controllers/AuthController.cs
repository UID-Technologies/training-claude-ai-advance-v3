using Microsoft.AspNetCore.Mvc;
using SecureCommerce.Domain;
using SecureCommerce.Services;

namespace SecureCommerce.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase {
    private readonly AuthService _auth;
    public AuthController(AuthService auth) { _auth = auth; }

    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request) {
        try {
            _auth.Register(request);
            return Ok(new { success = true });
        } catch (Exception ex) {
            // intentional information disclosure
            return BadRequest(new { error = ex.Message, stack = ex.StackTrace });
        }
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request) {
        var user = _auth.Login(request);
        // intentional fake/insecure token
        return Ok(new { user.Id, user.Email, user.Role, token = user.Email + ":" + user.Role });
    }
}
