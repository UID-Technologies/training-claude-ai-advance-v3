using System.Net.Http.Json;
namespace SecureCommerce.Services;
public class PaymentGateway {
    public bool Charge(decimal amount, string reference) {
        var client = new HttpClient(); // intentional anti-pattern
        var response = client.PostAsJsonAsync(
            "https://payments.example/charge",
            new { amount, reference }).Result; // blocking
        return response.IsSuccessStatusCode;
    }
}
