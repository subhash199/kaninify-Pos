using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EposRetail.Services
{
    public class LocalServer
    {
        private readonly HttpListener _listener = new();

        public LocalServer(string url = "https://+:5000/") // * means all IPs
        {
            _listener.Prefixes.Add(url);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                var ctx = await _listener.GetContextAsync();
                var res = ctx.Response;

                string html = $@"
                <html>
                  <body>
                    <h1>Hello from MAUI Server on {Dns.GetHostName()}</h1>
                    <p>You connected from: {ctx.Request.RemoteEndPoint}</p>
                    <a href='/about'>About</a>
                  </body>
                </html>";

                byte[] buffer = Encoding.UTF8.GetBytes(html);
                res.ContentLength64 = buffer.Length;
                res.ContentType = "text/html";
                await res.OutputStream.WriteAsync(buffer);
                res.Close();
            }
        }
    }
}
