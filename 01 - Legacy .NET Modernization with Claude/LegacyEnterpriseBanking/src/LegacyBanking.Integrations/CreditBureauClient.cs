using System.Configuration;
using System.Net;

namespace LegacyBanking.Integrations
{
    public class CreditBureauClient
    {
        public int GetCreditScore(string nationalId)
        {
            var baseUrl = ConfigurationManager.AppSettings["CreditBureauUrl"];
            var apiKey = ConfigurationManager.AppSettings["CreditBureauApiKey"];

            // Legacy issues:
            // - sync WebClient
            // - national ID in query string
            // - API key in query string
            var url = baseUrl +
                      "/score?nationalId=" + nationalId +
                      "&apiKey=" + apiKey;

            using (var client = new WebClient())
            {
                var value = client.DownloadString(url);
                return int.Parse(value);
            }
        }
    }
}
