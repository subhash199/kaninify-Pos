using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EposRetail.Services
{
    public class PaymentService
    {
        public async Task SendCardPaymentAsync(decimal amount, string localIp, string port, string currencyCode)
        {
            using (var client = new HttpClient())
            {
                var terminalIP =localIp; // Change this to your terminal's IP
                var url = $"https://{terminalIP}:{port}"; // SPOS usually runs on 8080
                var responses = await new HttpClient().GetAsync(url);
                Console.WriteLine($"Port {port} responded: {responses.StatusCode}");
                var payload = new
                {
                    msgType = "0200",
                    msgID = Guid.NewGuid().ToString("N"),
                    txnType = "SALE",
                    amount = ((int)(amount * 100)).ToString(),
                    currency = currencyCode, // GBP
                    terminalID = "1850399793"
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Transaction sent successfully.");
                }
                else
                {
                    Console.WriteLine("Transaction failed. Status: " + response.StatusCode);
                }
            }
        }
    }
}
