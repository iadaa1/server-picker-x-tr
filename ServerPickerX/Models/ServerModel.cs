using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ServerPickerX.Models
{
    // ObservableObject base class requires a partial class type to  
    // generate boiler plate code for common MVVM implementations
    public partial class ServerModel: ObservableObject
    {
        public string Flag { get; set; } = "";

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        [ObservableProperty]
        public string? ping;

        [ObservableProperty]
        public string? status;

        public List<RelayModel> RelayModels { get; set; } = [];

        public async void PingServer()
        {
            if (RelayModels.Count == 0)
            {
                return;
            }

            using Ping ping = new();

            foreach (RelayModel relay in RelayModels)
            {
                Ping = "Pinging server";

                try
                {
                    var res = await ping.SendPingAsync(relay.IPv4, timeout: 1000);

                    if (res.RoundtripTime > 0)
                    {
                        Ping = res.RoundtripTime + "ms";

                        break;
                    }
                }
                catch
                {
                    // server timed out, proceed to next relay
                    continue;
                }
            }

            // if pinging status remains after pinging all server relay addresses then its blocked or unreachable
            if (Ping == "Pinging server")
            {
                Status = "❌";
                Ping = "";
            }
            else
            {
                Status = "✅";
            }
        }
    }
}
