using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace FTP_Server.Server.Client_Session
{
    public static class ClientManager
    {
        private static Semaphore AccessPool { get; set; } = new Semaphore(1, 1);
        public static List<Client> Clients { get; private set; } = new List<Client>();


        public static void AddClient(Socket connection)
        {
            AccessPool.WaitOne();
            
            Clients.Add(new Client(connection));

            AccessPool.Release();
        }

        public static void RemoveClient(Client client)
        {
            AccessPool.WaitOne();
            
            Clients.Remove(client);

            AccessPool.Release();
        }
    }
}