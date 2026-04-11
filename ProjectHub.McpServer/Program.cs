using System;
using System.Threading.Tasks;

namespace ProjectHub.McpServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Project Hub MCP Server starting...");
            var server = new McpServer();
            await server.StartAsync();
        }
    }
}
