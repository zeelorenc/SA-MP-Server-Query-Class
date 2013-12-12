/*
 * Well I didn't like the 'simplicity' in the original SA-MP query for C#.
 * This is intended for those seeking a nice and simple way to query a server.
 * 
 * Anyways, I was bored. Have fun coding! :D
 * 
 * Coded by Lorenc (zeelorenc)
*/

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace SampQueryApi
{
    class SampQuery
    {
        IPAddress serverIp;
        IPEndPoint serverEndPoint;
        Socket svrConnect;

        string szIP;
        ushort iPort;
        bool bDebug = false;

        DateTime TransmitMS = new DateTime();
        DateTime ReceiveMS = new DateTime();

        Dictionary<string, string> dData = new Dictionary<string, string>();

        public SampQuery(string ip, ushort port, char packet_type, bool console_debug = false)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            try
            {
                serverIp = new IPAddress(IPAddress.Parse(ip).GetAddressBytes());
                serverEndPoint = new IPEndPoint(serverIp, port);

                svrConnect = new Socket(serverEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

                svrConnect.SendTimeout = 5000;
                svrConnect.ReceiveTimeout = 5000;
                szIP = ip;
                iPort = port;
                bDebug = console_debug;

                if(bDebug) Console.Write("Connecting to " + ip + ":" + port + Environment.NewLine);

                try
                {
                    using (stream)
                    {
                        using (writer)
                        {
                            string[] szSplitIP = szIP.ToString().Split('.');

                            writer.Write("SAMP".ToCharArray());

                            writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIP[0])));
                            writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIP[1])));
                            writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIP[2])));
                            writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIP[3])));

                            writer.Write(iPort);
                            writer.Write(packet_type);

                            if (bDebug) Console.Write("Transmitting Packet '" + packet_type + "'" + Environment.NewLine);

                            TransmitMS = DateTime.Now; // To get ping (ms to reach back & forth to the svr)
                        }
                    }
                    svrConnect.SendTo(stream.ToArray(), serverEndPoint);
                }
                catch (Exception e)
                {
                    if (bDebug) Console.Write("Failed to receive packet:", e);
                }
            }
            catch (Exception e)
            {
                if (bDebug) Console.Write("Failed to connect to IP:", e);
            }
        }

        public Dictionary<string, string> read(bool flushdata = true)
        {
            try
            {
                serverIp = new IPAddress(IPAddress.Parse(szIP).GetAddressBytes());
                serverEndPoint = new IPEndPoint(serverIp, iPort);

                EndPoint rawPoint = (EndPoint)serverEndPoint;

                byte[] szReceive = new byte[2048];
                svrConnect.ReceiveFrom(szReceive, ref rawPoint);

                svrConnect.Close();

                ReceiveMS = DateTime.Now;

                if (flushdata)
                    dData.Clear();

                string ping = ReceiveMS.Subtract(TransmitMS).Milliseconds.ToString();

                MemoryStream stream = new MemoryStream(szReceive);
                BinaryReader read = new BinaryReader(stream);

                using (stream)
                {
                    using (read)
                    {
                        read.ReadBytes(10);

                        switch (read.ReadChar())
                        {
                            case 'i':
                                dData.Add("password",       Convert.ToString(read.ReadByte()));
                                dData.Add("players",        Convert.ToString(read.ReadInt16()));
                                dData.Add("maxplayers",     Convert.ToString(read.ReadInt16()));
                                dData.Add("hostname",       new string(read.ReadChars(read.ReadInt32())));
                                dData.Add("gamemode",       new string(read.ReadChars(read.ReadInt32())));
                                dData.Add("mapname",        new string(read.ReadChars(read.ReadInt32())));
                                break;

                            case 'r':
                                for (int i = 0, iRules = read.ReadInt16(); i < iRules; i++)
                                    dData.Add(new string(read.ReadChars(read.ReadByte())), new string(read.ReadChars(read.ReadByte())));
                                break;

                            case 'c':
                                for (int i = 0, iPlayers = read.ReadInt16(); i < iPlayers; i++)
                                    dData.Add(new string(read.ReadChars(read.ReadByte())), Convert.ToString(read.ReadInt32()));
                                break;

                            case 'd':
                                for (int i = 0, iTotalPlayers = read.ReadInt16(); i < iTotalPlayers; i++)
                                {
                                    string id = Convert.ToString(read.ReadByte());
                                    dData.Add(id + ".name",  new string(read.ReadChars(read.ReadByte())));
                                    dData.Add(id + ".score", Convert.ToString(read.ReadInt32()));
                                    dData.Add(id + ".ping", Convert.ToString(read.ReadInt32()));
                                }
                                break;

                            case 'p':
                                dData.Add("Ping", ping.ToString());
                                break;

                        }
                    }
                }

            }
            catch (Exception e)
            {
                if (bDebug) Console.Write("There's been a problem reading the data", e);
            }
            return dData;
        }
    }
}
