using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System;
using System.Net;
using Serg.IO;
using System.Threading;

namespace Serg.IO.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length <= 0) throw new Exception("No params Received");
            if(args[0] == "server"){
                SergServer server = new SergServer();
                server.settings.debug = true;
                server.settings.maxClients = 5;
                server.settings.port = 4567;
                server.On("server-start", data => {
                    if(server.IsServerRunning)
                        Console.WriteLine("Server started in " + data);
                });
                server.On("pong", (data) => {
                    Console.WriteLine(data);
                    Task.Run(() => {
                        Thread.Sleep(200);
                        server.Emit("ping", data);
                    });
                });
                server.StartServer();
            }else {
                SergClient client = new SergClient();
                client.settings.debug = true;
                client.settings.ipConnect = "127.0.1.1";
                client.settings.port = 4567;
                client.On("server-connect", data => {
                    if(client.Connected){
                        Console.WriteLine("Client Connected");
                        client.Emit("pong", data);
                    }
                });
                client.On("ping", (data) => {
                    Console.WriteLine(data);
                    Task.Run(() => {
                        Thread.Sleep(200);
                        client.Emit("pong", data);
                    });
                });
                client.ConnectToServer();
            }
            Console.ReadKey();
        }
    }
}
