using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Chapter9_UsingAsynchronousIO
{
    internal class AsyncHttpServer
    {
        private readonly HttpListener _listener;
         

        public AsyncHttpServer(int portNumber)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{portNumber}/");
        }

        const string RESPONSE_TEMPLATE  =  "<html><head><title>Test</title></head><body><h2>Testpage</h2><h4>Today is: {0}</h4></body></html>";

        public async Task Start()
        {
            _listener.Start();

            while (true)
            {
                var ctx = await _listener.GetContextAsync();
                System.Console.WriteLine("Client connected....");
                var response = string.Format(RESPONSE_TEMPLATE, DateTime.Now);

                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    await sw.WriteAsync(response);
                    await sw.FlushAsync();
                }

            }
        }

        public async Task Stop()
        {
            _listener.Abort();
        }
    }
}