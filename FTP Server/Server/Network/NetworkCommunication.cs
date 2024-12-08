using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Message_Board.Network
{
    public static class NetworkCommunication
    {
        public static int PacketLength { get; private set; } = 1024;
        public static int PacketErrorTolerance { get; private set; } = 100;
        public static int PacketErrorTimeout { get; private set; } = 500; // milliseconds 


        /// <summary>
        /// Encodes and sends a string over the network.
        /// </summary>
        /// <param name="handler">the socket connection to send the string over</param>
        /// <param name="txt">the string to send</param>
        /// <returns>Returns NetworkFlags.ConnectionSuccess if the file was sent successfully,
        /// if not, returns NetworkFlags.ClientDisconnectedFlag</returns>
        public static string SendOverNetwork(Socket handler, string txt)
        {
            txt += NetworkFlags.EndOfFileFlag;
            byte[] decoded = Encoding.ASCII.GetBytes(txt);

            return SendBytesOverNetwork(handler, decoded);
        }

        /// <summary>
        /// Awaits and receives a string from the given socket connection
        /// </summary>
        /// <param name="handler">the socket to receive the string from</param>
        /// <returns>returns the received string if the connection was successful.
        ///          returns NetworkFlags.ClientDisconnectedFlag if not</returns>
        public static string ReceiveFromSocket(Socket handler)
        {
            string msg = "";

            while (true)
            {
                var buffer = new byte[PacketLength];
                int count = ReceiveBytesFromNetwork(handler, out buffer);

                msg += Encoding.ASCII.GetString(buffer, 0, count);

                if (msg.Contains(NetworkFlags.EndOfFileFlag))
                {
                    msg = msg.Substring(0, msg.Length - 5);
                    break;
                }
            }

            return msg;
        }

        public static string SendFileOverNetwork(Socket handler, string path)
        {
            NetworkPacket[] packets = SerializeFileIntoPackets(path);

            return SendPacketsOverNetwork(handler, packets);
        }
        public static string ReceiveFileFromNetwork(Socket handler, string path)
        {
            string result = ReceivePacketsFromNetwork(handler, out NetworkPacket[] packets);

            DeserializePacketsIntoFile(packets, path);

            return NetworkFlags.ConnectionSuccess;
        }
        
        private static NetworkPacket[] SerializeFileIntoPackets(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open);
            int PacketCount = (int)Math.Ceiling(fileStream.Length / (double)PacketLength);

            NetworkPacket[] packets = new NetworkPacket[PacketCount];

            for (int i = 0; i < PacketCount; i++)
            {
                byte[] content = new byte[PacketLength];

                int count = fileStream.Length - fileStream.Position > PacketLength
                    ? PacketLength
                    : (int)(fileStream.Length - fileStream.Position);

                fileStream.Read(content, i * PacketLength, count);

                NetworkPacket packet = new NetworkPacket(i + 1, PacketCount, content);

                packets[i] = packet;
            }

            return packets;
        }
        private static void DeserializePacketsIntoFile(NetworkPacket[] packets, string filePath)
        {
            StreamWriter writer = new StreamWriter(filePath);

            writer.Flush();
            
            for (int i = 0; i < packets.Length; i++)
            {
                string content = Encoding.ASCII.GetString(packets[i].Content);

                writer.Write(content);
            }
            
            writer.Dispose();
            writer.Close();
        }


        private static string SendPacketsOverNetwork(Socket handler, NetworkPacket[] packets)
        {
            int lastErrorIndex = -1,
                errorCount = 0;

            for (int i = 0; i < packets.Length; i++)
            {
                string result = SendPacketOverNetwork(handler, packets[i]);

                if (result != NetworkFlags.ConnectionSuccess)
                {
                    if (errorCount >= PacketErrorTolerance)
                    {
                        return NetworkFlags.ConnectionFailed;
                    }

                    if (lastErrorIndex == i)
                    {
                        errorCount++;
                        Thread.Sleep(PacketErrorTimeout);
                    }

                    lastErrorIndex = i;
                    i--;
                    continue;
                }
            }

            return NetworkFlags.ConnectionSuccess;
        }
        private static string ReceivePacketsFromNetwork(Socket handler, out NetworkPacket[] packets)
        {
            NetworkPacket initial;

            // get initial packet
            int errorCount = 0, lastErrorIndex = -1;
            while (true)
            {
                initial = ReceivePacketFromNetwork(handler);

                if (initial.index == -1)
                {
                    errorCount++;
                    Thread.Sleep(PacketErrorTimeout);
                    continue;
                }

                if (errorCount == PacketErrorTolerance)
                {
                    packets = null;
                    return NetworkFlags.ConnectionFailed;
                }
                
                break;
            }

            errorCount = 0;
            packets = new NetworkPacket[initial.Max];

            for (int i = 1; i < packets.Length; i++)
            {
                NetworkPacket packet = ReceivePacketFromNetwork(handler);

                if (lastErrorIndex == i)
                {
                    errorCount++;
                }

                if (errorCount >= PacketErrorTolerance)
                {
                    return NetworkFlags.ConnectionFailed;
                }
                
                if (packet.index == -1)
                {
                    lastErrorIndex = i;
                    i--;
                    continue;
                }

                packets[i] = packet;
            }

            return NetworkFlags.ConnectionSuccess;
        }
        
        private static string SendPacketOverNetwork(Socket handler, NetworkPacket packet)
        {
            string json = JsonConvert.SerializeObject(packet, Formatting.None);

            byte[] data = Encoding.ASCII.GetBytes(json);

            return SendBytesOverNetwork(handler, data);
        }
        private static NetworkPacket ReceivePacketFromNetwork(Socket handler)
        {
            byte[] buffer = new byte[PacketLength];
            int count = ReceiveBytesFromNetwork(handler, out buffer);

            if (count == -1) return new NetworkPacket(-1, -1, Array.Empty<byte>());

            string raw = Encoding.ASCII.GetString(buffer);

            NetworkPacket packet = JsonConvert.DeserializeObject<NetworkPacket>(raw);

            return packet;
        }

        private static string SendBytesOverNetwork(Socket handler, byte[] data)
        {
            try
            {
                handler.Send(data);
            }
            catch (Exception)
            {
                return NetworkFlags.ClientDisconnectedFlag;
            }

            return NetworkFlags.ConnectionSuccess;
        }
        private static int ReceiveBytesFromNetwork(Socket handler, out byte[] data)
        {
            var buffer = new byte[PacketLength];
            int count;

            try
            {
                count = handler.Receive(buffer);
            }
            catch (Exception)
            {
                data = new byte[PacketLength];
                return -1;
            }

            data = buffer;

            return count;
        }
    }
}