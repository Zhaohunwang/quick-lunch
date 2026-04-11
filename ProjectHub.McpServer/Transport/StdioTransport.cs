using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.McpServer
{
    public class StdioTransport
    {
        private Func<string, Task> _requestHandler;

        public async Task StartAsync(Func<string, Task> requestHandler)
        {
            _requestHandler = requestHandler;
            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var line = Console.ReadLine();
                        if (line != null)
                        {
                            _requestHandler(line).Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }
                }
            });
        }

        public void SendResponse(string response)
        {
            Console.WriteLine(response);
            Console.Out.Flush();
        }
    }
}
