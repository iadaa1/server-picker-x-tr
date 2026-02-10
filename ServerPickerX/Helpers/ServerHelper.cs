using ServerPickerX.ConfigSections;
using ServerPickerX.Models;
using ServerPickerX.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ServerPickerX.Helpers
{
    public class ServerHelper
    {
        public static string CURRENT_SERVER_REVISION = string.Empty;

        // singleton objects for accessing servers on runtime, cache for fast access
        public static List<ServerModel> UNCLUSTERED_SERVERS = [];

        public static List<ServerModel> CLUSTERED_SERVERS = [];

        // keywords for clustering servers
        private static string[] CLUSTERED_SERVER_KEYWORDS = ["China","Sweden","Japan","India","Hong Kong,Netherlands"];

        public async static Task LoadServers()
        {
            using HttpClient httpClient = new();

            try
            {
                Stream res = await httpClient.GetStreamAsync("https://api.steampowered.com/ISteamApps/GetSDRConfig/v1/?appid=730");

                if (res == null)
                {
                    throw new Exception(
                        "Failed to load servers!" + Environment.NewLine + Environment.NewLine +
                        "- Verify your internet connection or firewall are working and enabled" + Environment.NewLine +
                        "- Make sure to run the app as admin or with sudo level execution"
                    );
                }

                JsonObject? mainJson = await JsonObject.ParseAsync(res) as JsonObject;

                if (mainJson?["revision"] == null || mainJson?["pops"] == null)
                {
                    throw new Exception("Server relay data unavailable. Please try again later.");
                }

                CURRENT_SERVER_REVISION = mainJson["revision"].ToString();

                // set server revision value in json setting if app is initialized for the first time
                JsonSetting jsonSettings = MainWindow.jsonSettings;

                if (jsonSettings.server_revision == "-1")
                {
                    jsonSettings.server_revision = CURRENT_SERVER_REVISION;

                    await jsonSettings.SaveSettings();
                }

                foreach (KeyValuePair<string, JsonNode> server in mainJson["pops"] as JsonObject)
                {
                    if (server.Value?["relays"] == null)
                    {
                        continue;
                    }

                    string serverDescription = server.Value["desc"].ToString();
                    string clusterName = CLUSTERED_SERVER_KEYWORDS.FirstOrDefault(keyword => serverDescription.Contains(keyword), "");

                    var serverModel = new ServerModel
                    {
                        Flag = "/Assets/flags/"
                            + serverDescription + $" ({server.Key}).png",
                        Name = server.Key,
                        Description = serverDescription,
                    };

                    foreach (JsonObject relay in server.Value["relays"] as JsonArray)
                    {
                        serverModel.RelayModels.Add(new RelayModel
                        {
                            IPv4 = relay["ipv4"]?.ToString()
                        });
                    }

                    UNCLUSTERED_SERVERS.Add(serverModel);

                    // create a clustered server if server model belongs to a cluster
                    if (!String.IsNullOrEmpty(clusterName))
                    {
                        ServerModel clusteredServer = CLUSTERED_SERVERS.FirstOrDefault(server => server.Description == clusterName, new ServerModel());

                        // merge server relay list to clustered server relay list
                        clusteredServer.RelayModels.AddRange(serverModel.RelayModels);

                        // initialize clustered server data if not an element inside clustered collection
                        if (String.IsNullOrEmpty(clusteredServer.Description)) {
                            clusteredServer.Flag = serverModel.Flag;
                            clusteredServer.Name = "cluster";
                            clusteredServer.Description = clusterName;

                            CLUSTERED_SERVERS.Add(clusteredServer);
                        }
                    } else
                    {
                        CLUSTERED_SERVERS.Add(serverModel);
                    }
                }
            }
            catch (Exception ex) {
                await MessageBoxHelper.ShowMessageBox("Error", ex.Message);
            }
        }

        public static async Task BlockUnblockServersWindows(bool shouldBlock, ObservableCollection<ServerModel> serverModels)
        {
            using Process process = ProcessHelper.CreateProcess("cmd.exe");

            foreach (ServerModel serverModel in serverModels)
            {
                string ipAddresses = String.Join(",", serverModel.RelayModels.Select(s => s.IPv4).ToList());
       
                process.StartInfo.Arguments = $"/c {Path.Combine(Environment.SystemDirectory, "netsh.exe")} " +
                        "advfirewall firewall " +
                        (shouldBlock ? "add" : "delete") + " rule " +
                        "name=server_picker_x_" + serverModel.Description.Replace(" ", "") +
                        (shouldBlock ? " dir=out action=block protocol=ANY " + "remoteip=" + ipAddresses : "");

                process.Start();
                await process.WaitForExitAsync();

                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();

                // skip throwing exception if firewall rule doesn't exist when unblocking servers
                if ((process.ExitCode == 1 || process.ExitCode < 0) &&
                    !$"{stdOut} {stdErr}".Contains("No rules match"))
                {
                    throw new Exception("StdOut: " + stdOut + Environment.NewLine + "StdErr: " + stdErr);
                }
            }
        }

        public static async Task BlockUnblockServersLinux(bool shouldBlock, ObservableCollection<ServerModel> serverModels)
        {
            using Process process = ProcessHelper.CreateProcess("sudo");

            foreach (ServerModel serverModel in serverModels)
            {
                string ipAddresses = String.Join(",", serverModel.RelayModels.Select(s => s.IPv4).ToList());

                // append or delete rules in the iptables input chain
                process.StartInfo.Arguments = "iptables " +
                        (shouldBlock ? "-A" : "-D") + " INPUT -s " + ipAddresses + " -j DROP";

                process.Start();
                await process.WaitForExitAsync();

                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();

                // skip throwing exception if iptables input chain rule doesn't exist when unblocking servers
                if ((process.ExitCode == 1 || process.ExitCode < 0) && 
                    !$"{stdOut} {stdErr}".Contains("Bad rule (does a matching"))
                {
                    throw new Exception("StdOut: " + stdOut + Environment.NewLine + "StdErr: " + stdErr);
                }
            }
        }
    }
}
