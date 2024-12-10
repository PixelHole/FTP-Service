using System;
using System.Net.Sockets;
using Message_Board.Network;

namespace FTP_Server.Server.Client_Session
{
    public class ClientConnection
    {
        private Client Owner { get; set; }


        public ClientConnection(Client owner)
        {
            Owner = owner;
        }

        public string GetCommandFromClient()
        {
            string cmd = NetworkCommunication.ReceiveFromSocket(Owner.ControlSocket);
            if (cmd == NetworkFlags.ClientDisconnectedFlag) ClientDisconnectHandler();
            return cmd;
        }
        public string SendMessageToClient(string msg)
        {
            string cmd = NetworkCommunication.SendOverNetwork(Owner.ControlSocket, msg);
            if (cmd == NetworkFlags.ClientDisconnectedFlag) ClientDisconnectHandler();
            return cmd;
        }

        public Socket EstablishDataConnection()
        {
            Socket dataSocket;

            try
            {
                dataSocket = LocalServer.DataListener.Accept();
            }
            catch (Exception e)
            {
                dataSocket = null;
            }

            return dataSocket;
        }
        
        private void ClientDisconnectHandler()
        {
            // fancy reconnect logic can be here
            Owner.EndService();
        }
    }
}