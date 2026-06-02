using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO.Ports;
using System.Diagnostics;

namespace SinkholeLibrary
{
    public class Sinkhole
    {
        public event EventHandler<string> NewDomain;

        public Sinkhole()
        {
            NewDomain = delegate { };
        }


        public void Start()
        {
            // 1. Initialize the serial port configuration
            SerialPort mySerialPort = new SerialPort("53")
            {
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8
            };
            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            mySerialPort.Open();

        }

        public static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadExisting();
            Debug.WriteLine(data);

            //var upstream = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);

            //while (true)
            //{
            //    var sender = new IPEndPoint(IPAddress.Any, 0);
            //    byte[] query = listener.Receive(ref sender);

            //    string domain = ParseDomainFromQuery(query);
            //    NewDomain?.Invoke(sender, domain);

            //    if (DatabaseHandler.IsDomain(domain))
            //    {
            //        byte[] blocked = BuildBlockedResponse(query);
            //        listener.Send(blocked, blocked.Length, sender);
            //    }
            //    else
            //    {
            //        // Forward to real DNS and relay response
            //        using var forwarder = new UdpClient();
            //        forwarder.Send(query, query.Length, upstream);
            //        var upstreamSender = new IPEndPoint(IPAddress.Any, 0);
            //        byte[] response = forwarder.Receive(ref upstreamSender);
            //        listener.Send(response, response.Length, sender);
            //    }
            //}
        }

        private string ParseDomainFromQuery(byte[] query)
        {
            // Simple DNS query parser to extract the domain name
            int pos = 12; // Skip DNS header
            StringBuilder domain = new StringBuilder();
            while (query[pos] != 0)
            {
                int len = query[pos];
                pos++;
                domain.Append(Encoding.ASCII.GetString(query, pos, len) + ".");
                pos += len;
            }
            return domain.ToString().TrimEnd('.');
        }

        private byte[] BuildBlockedResponse(byte[] query)
        {
            byte[] response = new byte[query.Length + 16];
            Array.Copy(query, response, query.Length); 
            response[2] = 0x81; 
            response[3] = 0x80;
            response[6] = 0x00;
            response[7] = 0x01;
            
            return response;
        }
    }
}
