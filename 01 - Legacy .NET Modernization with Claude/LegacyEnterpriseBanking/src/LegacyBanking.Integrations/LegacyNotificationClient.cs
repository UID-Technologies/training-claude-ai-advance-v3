using System;

namespace LegacyBanking.Integrations
{
    // Simulates an old SOAP/enterprise notification client.
    public class LegacyNotificationClient
    {
        public void SendEmail(string email, string subject, string body)
        {
            Console.WriteLine(
                "Legacy SOAP EMAIL => " + email +
                " | " + subject +
                " | " + body);
        }

        public void SendSms(string mobile, string body)
        {
            Console.WriteLine(
                "Legacy SOAP SMS => " + mobile +
                " | " + body);
        }
    }
}
