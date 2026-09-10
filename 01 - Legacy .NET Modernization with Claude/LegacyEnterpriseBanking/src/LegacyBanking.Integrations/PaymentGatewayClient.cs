using System.Configuration;
using System.Net.Http;
using System.Text;

namespace LegacyBanking.Integrations
{
    public class PaymentGatewayClient
    {
        public bool Pay(string transactionReference, decimal amount)
        {
            // Legacy issue: HttpClient created per request.
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add(
                "Authorization",
                "Bearer " + ConfigurationManager.AppSettings["PaymentGatewayToken"]);

            // Legacy issue: JSON built manually.
            var json =
                "{\"reference\":\"" + transactionReference +
                "\",\"amount\":" + amount + "}";

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            // Legacy issue: blocks on async call.
            var response = client.PostAsync(
                ConfigurationManager.AppSettings["PaymentGatewayUrl"],
                content).Result;

            return response.IsSuccessStatusCode;
        }
    }
}
