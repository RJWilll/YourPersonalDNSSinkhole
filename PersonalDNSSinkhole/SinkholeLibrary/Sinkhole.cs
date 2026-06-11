using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using System.Net.NetworkInformation;

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
            SetDnsToProxy();
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
            int wasBlocked = 0;
            var stopwatch = Stopwatch.StartNew();

            byte[] response = { };

            if (DatabaseHandler.IsDomain(domain))
            {
                //die
                response = BuildBlockedResponse(query);
                wasBlocked++;
            }
            else
            {
                //pass
                response = BuildPassResponse(query, upstream);
            }

            DatabaseHandler.AddLogAsync(domain, GetQueryType(query), wasBlocked, (int)stopwatch.ElapsedMilliseconds);
            await listener.SendAsync(response, response.Length, sender);
        }

        public void Stop()
        {
            powerSwitch = false;
            RestoreDns();
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

        private string GetQueryType(byte[] query)
        {
            int i = 12;
            while (i < query.Length && query[i] != 0) i++;
            i++;

            if (i + 2 > query.Length) return "UNKNOWN";

            int qtype = (query[i] << 8) | query[i + 1];

            return qtype switch
            {
                1 => "A",
                2 => "NS",
                5 => "CNAME",
                15 => "MX",
                16 => "TXT",
                28 => "AAAA",
                33 => "SRV",
                255 => "ANY",
                _ => $"TYPE{qtype}"
            };
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

        public static void SetDnsToProxy()
        {
            string? networkAdapter = GetActiveAdapter();
            RunNetsh($"interface ip set dns \"{networkAdapter}\" static 127.0.0.1");
        }

        public static void RestoreDns()
        {
            string? networkAdapter = GetActiveAdapter();
            RunNetsh($"interface ip set dns \"{networkAdapter}\" dhcp");
        }

        private static void RunNetsh(string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = args,
                Verb = "runas",          // elevate if needed
                UseShellExecute = true,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
        }

        private static string? GetActiveAdapter()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                ?.Name;
        }
    
    }
}
