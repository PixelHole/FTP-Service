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
        public static int ActualPacketLength { get; private set; } = 1024;
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

            for (int i = 0; i <= decoded.Length / PacketLength; i++)
            {
                byte[] trimmed = new byte[PacketLength];

                if (decoded.Length > PacketLength)
                {
                    Array.Copy(decoded, i * PacketLength, trimmed, 0, Math.Min(PacketLength, decoded.Length - i * PacketLength));
                }
                else
                {
                    trimmed = decoded;
                }

                if (SendBytesOverNetwork(handler, trimmed) != NetworkFlags.ConnectionSuccess)
                    return NetworkFlags.ConnectionFailed;
            }

            return NetworkFlags.ConnectionSuccess;
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

                if (count == -1) return NetworkFlags.ClientDisconnectedFlag;
                
                msg += Encoding.ASCII.GetString(buffer, 0, count);

                if (msg.Contains(NetworkFlags.EndOfFileFlag))
                {
                    msg = msg.Substring(0, msg.IndexOf(NetworkFlags.EndOfFileFlag, StringComparison.Ordinal));
                    break;
                }
            }

            return msg;
        }

        public static string SendFileOverNetwork(Socket handler, string path)
        {
            NetworkPacket[] packets = SerializeFileIntoPackets(path);

            SendOverNetwork(handler, NetworkFlags.ReadyFlag);

            string handshake = ReceiveFromSocket(handler);

            if (handshake != NetworkFlags.ReadyFlag) return NetworkFlags.FailedHandshakeFlag;

            return SendPacketsOverNetwork(handler, packets);
        }
        public static string ReceiveFileFromNetwork(Socket handler, string path)
        {
            string ready = ReceiveFromSocket(handler);

            if (ready != NetworkFlags.ReadyFlag) return NetworkFlags.FailedHandshakeFlag;
            
            SendOverNetwork(handler, NetworkFlags.ReadyFlag);
            
            string result = ReceivePacketsFromNetwork(handler, out NetworkPacket[] packets);

            DeserializePacketsIntoFile(packets, path);

            return result;
        }
        
        private static NetworkPacket[] SerializeFileIntoPackets(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open);
            int PacketCount = (int)Math.Ceiling(fileStream.Length / (double)PacketLength);

            NetworkPacket[] packets = new NetworkPacket[PacketCount];

            for (int i = 0; i < PacketCount; i++)
            {
                byte[] content = new byte[PacketLength];

                fileStream.Read(content,0 , PacketLength);

                NetworkPacket packet = new NetworkPacket(i + 1, PacketCount, content);

                packets[i] = packet;
            }

            fileStream.Dispose();
            fileStream.Close();

            return packets;
        }
        private static void DeserializePacketsIntoFile(NetworkPacket[] packets, string filePath)
        {
            StreamWriter writer = new StreamWriter(filePath);

            writer.Flush();
            
            for (int i = 0; i < packets.Length; i++)
            {
                Byte[] data;
                
                if (i == packets.Length - 1)
                {
                    int firstZero = packets[i].Content.Length;
                    
                    for (int j = 0; j < packets[i].Content.Length; j++)
                    {
                        if (packets[i].Content[j] == 0)
                        {
                            firstZero = j;
                            break;
                        }
                    }

                    data = new byte[firstZero];
                    
                    Array.Copy(packets[i].Content, 0, data, 0, firstZero);
                }
                else
                {
                    data = packets[i].Content;
                }
                
                string content = Encoding.ASCII.GetString(data);
                
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

            return NetworkFlags.TransferSuccessFlag;
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

            packets[0] = initial;

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

            return NetworkFlags.TransferSuccessFlag;
        }
        
        private static string SendPacketOverNetwork(Socket handler, NetworkPacket packet)
        {
            string json = JsonConvert.SerializeObject(packet, Formatting.None);

            return SendOverNetwork(handler, json);
        }
        private static NetworkPacket ReceivePacketFromNetwork(Socket handler)
        {
            string raw = ReceiveFromSocket(handler);

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


        private static void Print(string msg)
        {
            Console.WriteLine($"[Network] : {msg}");
        }
    }
}