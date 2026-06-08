using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;

namespace SinkholeLibrary
{
    public class Sinkhole
    {
        public event EventHandler<string> NewDomain;
        public bool powerSwitch;


        public Sinkhole()
        {
            NewDomain = delegate { };
        }


        public async void Start()
        {
            var listener = new UdpClient(53);
            var upstream = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);

            Debug.WriteLine("DNS proxy running...");
            powerSwitch = true;

            while (powerSwitch)
            {
                var result = await listener.ReceiveAsync();
                byte[] query = result.Buffer;
                var sender = result.RemoteEndPoint;

                HandleDomainAsync(query, upstream, listener, sender);
            }
        }

        private async void HandleDomainAsync(byte[] query, IPEndPoint upstream, UdpClient listener, IPEndPoint sender)
        {
            string domain = ParseDomainFromQuery(query);
            NewDomain(this, domain);

            byte[] response = { };

            if (DatabaseHandler.IsDomain(domain))
            {
                //die
                response = BuildBlockedResponse(query);
            }
            else
            {
                //pass
                response = BuildPassResponse(query, upstream);
            }

            await listener.SendAsync(response, response.Length, sender);
        }

        public void Stop()
        {
            powerSwitch = false;
        }

        private string ParseDomainFromQuery(byte[] query)
        {
            int pos = 12; 
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

        private byte[] BuildPassResponse(byte[] query, IPEndPoint upstream)
        {
            using var forwarder = new UdpClient();
            forwarder.Send(query, query.Length, upstream);

            var upstreamSender = new IPEndPoint(IPAddress.Any, 0);
            byte[] response = forwarder.Receive(ref upstreamSender);

            return response;
        }
    }
}
