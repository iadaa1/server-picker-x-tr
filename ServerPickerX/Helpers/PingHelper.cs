using ServerPickerX.Models;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ServerPickerX.Helpers
{
    public class PingHelper
    {
        public static async Task PingServer(ServerModel server)
        {
            if (server == null)
            {
                return;
            }

            using Ping ping = new();

            foreach (RelayModel relay in server.RelayModels)
            {
                server.Ping = "Pinging server";

                try
                {
                    var res = await ping.SendPingAsync(relay.IPv4, timeout: 650);


                    if (res.RoundtripTime > 0)
                    {
                        server.Ping = res.RoundtripTime + "ms";

                        break;
                    }
                }
                catch
                {
                    continue;
                }
            }

            // if pinging status remains after pinging all server relay addresses then its blocked or unreachable
            if (server.Ping == "Pinging server")
            {
                server.Status = "❌";
                server.Ping = "";
            }
            else
            {
                server.Status = "✅";
            }
        }

        public static async Task CancelAllPings(List<Ping> pings)
        {
            foreach (Ping ping in pings)
            {
                ping.SendAsyncCancel();
                ping.Dispose();
            }
        }
    }
}
