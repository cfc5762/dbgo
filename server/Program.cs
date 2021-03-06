﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{

    class Program
    {
        public static IPAddress GetLocalIPAddress()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip;
                    }
                }
                return new IPAddress(new byte[] { 127, 0, 0, 1 });
            }
            return new IPAddress(new byte[] { 127, 0, 0, 1 });
        }
        static void Main(string[] args)
        {
            //create sender to talk to the players
            Socket Sender = new Socket(SocketType.Dgram, ProtocolType.Udp);
            //create reciever to recieve from the players
            Socket Listener = new Socket(SocketType.Dgram, ProtocolType.Udp);
            //resolve future host address
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            //temp endpoint
            IPEndPoint localEndPoint = new IPEndPoint(GetLocalIPAddress(), 57735);
            Listener.Bind(localEndPoint);
            Console.WriteLine("hosting on "+localEndPoint.Address+" "+localEndPoint.Port);
            Dictionary<EndPoint, Tuple<Discus.matchMakingPacket, DateTime>> gamerQueue = new Dictionary<EndPoint, Tuple<Discus.matchMakingPacket, DateTime>>(); 
            while (true)
            {
               //our buffer
                byte[] bufferin = new byte[1024];
                //the gamer that comes in the door
                EndPoint wanderingGamer = new IPEndPoint(IPAddress.Any, 0);
                //wait for a packet
                //Console.WriteLine("waiting on " + localEndPoint.Address + " " + localEndPoint.Port);
                Listener.ReceiveFrom(bufferin,ref wanderingGamer);
                
                BinaryFormatter b = new BinaryFormatter();
                try
                {
                    

                    Discus.matchMakingPacket message = (Discus.matchMakingPacket)b.Deserialize(new MemoryStream(bufferin));

                    message.ip = new byte[] { ((IPEndPoint)wanderingGamer).Address.GetAddressBytes()[12], ((IPEndPoint)wanderingGamer).Address.GetAddressBytes()[13], ((IPEndPoint)wanderingGamer).Address.GetAddressBytes()[14], ((IPEndPoint)wanderingGamer).Address.GetAddressBytes()[15] };
                    Console.WriteLine(message.ip[0] + "."+message.ip[1] + "."+message.ip[2] + "."+message.ip[3] + ":"+message.port);
                    if (message.acknowledged)
                    {
                        continue;
                    }
                    if (gamerQueue.Keys.Contains<EndPoint>(wanderingGamer))//is he in there
                    {
                        Console.WriteLine("updating entry");
                        gamerQueue[wanderingGamer]=new Tuple<Discus.matchMakingPacket, DateTime>(gamerQueue[wanderingGamer].Item1, DateTime.Now);
                    }
                    else
                    {
                        Console.WriteLine("got a packet...");
                        Console.WriteLine("deserialized");
                        Console.WriteLine("adding entry");
                        gamerQueue.Add(wanderingGamer, new Tuple<Discus.matchMakingPacket, DateTime>(message,DateTime.Now));
                    }
                    foreach (var item in gamerQueue.Keys)
                    {
                        if (DateTime.Now.Subtract(gamerQueue[item].Item2).TotalSeconds > 3)
                        {
                            //we havnt heard from them in too long
                            Console.WriteLine("removing "+item);
                            gamerQueue.Remove(item);
                        }
                    }
                    if (gamerQueue.Keys.Count >= 2)
                    {
                        Console.WriteLine("sending outward packets");
                        Discus.matchMakingPacket sendToGamer0 = gamerQueue[gamerQueue.Keys.ToArray()[1]].Item1;
                       sendToGamer0.ip = gamerQueue[gamerQueue.Keys.ToArray()[1]].Item1.ip;
                       sendToGamer0.port = gamerQueue[gamerQueue.Keys.ToArray()[1]].Item1.port;
                       sendToGamer0.color = (int)Discus.Team.Red;
                       sendToGamer0.acknowledged = true;
                       sendToGamer0.clientID = "Player 1";
                       sendToGamer0.enemyID = "Player 2";
                        Console.WriteLine("made pack 1");
                        Console.WriteLine("to player 1 - "+sendToGamer0.ip[0] + "." + sendToGamer0.ip[1] + "." + sendToGamer0.ip[2] + "." + sendToGamer0.ip[3] + ":" + sendToGamer0.port);
                        Discus.matchMakingPacket sendToGamer1 = gamerQueue[gamerQueue.Keys.ToArray()[0]].Item1;
                       sendToGamer1.ip = gamerQueue[gamerQueue.Keys.ToArray()[0]].Item1.ip;
                       sendToGamer1.port = gamerQueue[gamerQueue.Keys.ToArray()[0]].Item1.port;
                       sendToGamer1.color = (int)Discus.Team.Blue;
                       sendToGamer1.acknowledged = true;
                       sendToGamer1.clientID = "Player 2";
                       sendToGamer1.enemyID = "Player 1";
                        Console.WriteLine("made pack 2");
                        Console.WriteLine("to player 2 - "+sendToGamer1.ip[0] + "." + sendToGamer1.ip[1] + "." + sendToGamer1.ip[2] + "." + sendToGamer1.ip[3] + ":" + sendToGamer1.port);

                        gamerQueue = new Dictionary<EndPoint, Tuple<Discus.matchMakingPacket, DateTime>>();
                        MemoryStream tempBufferOut = new MemoryStream();
                        b.Serialize(tempBufferOut, sendToGamer1);
                        Sender.SendTo(tempBufferOut.ToArray(), new IPEndPoint(new IPAddress(sendToGamer0.ip), sendToGamer0.port));
                        Console.WriteLine("sent pack 1 to - "+ new IPAddress(sendToGamer0.ip).Address+" "+ sendToGamer0.port);
                        MemoryStream tempBufferOut2 = new MemoryStream();
                        b.Serialize(tempBufferOut2, sendToGamer0);
                        Sender.SendTo(tempBufferOut2.ToArray(), new IPEndPoint(new IPAddress(sendToGamer1.ip), sendToGamer1.port));
                    }    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " \n" + ex.StackTrace + " \n" + ex.HelpLink);
                }
                
            }
        }
    }
}
