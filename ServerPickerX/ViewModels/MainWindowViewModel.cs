using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia.Enums;
using ServerPickerX.ConfigSections;
using ServerPickerX.Extensions;
using ServerPickerX.Helpers;
using ServerPickerX.Models;
using ServerPickerX.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ServerPickerX.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollectionExtended<ServerModel> ServerModels { get; set; } = [];

        public ServerModel? SelectedDataGridItem { get; set; }

        // Mvvm tool kit will auto generate source code to make this property observable
        // When updating this property, reference it by its auto property name (PascalCase)
        [ObservableProperty]
        public bool showProgressBar = false;

        public bool PendingOperation = false;

        public bool ServersInitialized = false;

        public async Task LoadServers()
        {
            await ServerHelper.LoadServers();

            await ClusterUnclusterServers();

            ServersInitialized = true;
        }

        [RelayCommand]
        public async Task ClusterUnclusterServers()
        {
            JsonSetting jsonSetting = MainWindow.jsonSettings;

            // do not update json settings and unblock servers on first app load
            if (ServersInitialized)
            {
                jsonSetting.is_clustered = !jsonSetting.is_clustered;

                await jsonSetting.SaveSettings();

                await UnblockAll();
            }

            List<ServerModel> serverModels = jsonSetting.is_clustered ?
                ServerHelper.CLUSTERED_SERVERS : ServerHelper.UNCLUSTERED_SERVERS;

            ServerModels.Clear();

            ServerModels.AddRange(serverModels);

            PingServers(serverModels);
        }

        [RelayCommand]
        public void PingServers(ICollection<ServerModel> serverModels)
        {
            if (serverModels.Count == 0)
            {
                return;
            }

            try
            {
                foreach (ServerModel serverModel in serverModels)
                {
                    serverModel.PingServer();
                }
            } catch (InvalidOperationException ex) {
                // when user suddenly tries to cluster or uncluster the servers while ServerModels is being iterated
            }
        }

        public void PingSelectedServer()
        {
            if (SelectedDataGridItem == null)
            {
                return;
            }

            SelectedDataGridItem.PingServer();
        }

        [RelayCommand]
        public async Task BlockAll()
        {
            if (ServerModels.Count == 0)
            {
                return;
            }

            await performOperation(true, ServerModels);
        }

        [RelayCommand]
        public async Task BlockSelected(IList selectedServers)
        {
            if (selectedServers.Count == 0)
            {
                await MessageBoxHelper.ShowMessageBox("Info", "Hey! Please select at least one server to block");
                return;
            }

            var serverModels = new ObservableCollection<ServerModel>(selectedServers.Cast<ServerModel>());

            await performOperation(true, serverModels);
        }

        [RelayCommand]
        public async Task UnblockAll()
        {
            if (ServerModels == null || ServerModels.Count == 0)
            {
                return;
            }

            await performOperation(false, ServerModels);
        }


        [RelayCommand]
        public async Task UnblockSelected(IList selectedServers)
        {
            if (selectedServers.Count == 0)
            {
                await MessageBoxHelper.ShowMessageBox("Info", "Hey! Please select at least one server to unblock");
                return;
            }

            var serverModels = new ObservableCollection<ServerModel>(selectedServers.Cast<ServerModel>());

            await performOperation(false, serverModels);
        }

        public async Task performOperation(bool shouldBlock, ObservableCollection<ServerModel> serverModels)
        {
            if (PendingOperation)
            {
                await MessageBoxHelper.ShowMessageBox("Info", "Whoa! There's already a pending operation. Please wait...", Icon.Setting);
                return;
            }

            PendingOperation = true;

            ShowProgressBar = true;

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    // offload to background thread, process.waitForExit blocks the UI thread
                    await Task.Run(() => ServerHelper.BlockUnblockServersWindows(shouldBlock, serverModels));
                }
                else if (OperatingSystem.IsLinux())
                {
                    await Task.Run(() => ServerHelper.BlockUnblockServersLinux(shouldBlock, serverModels));
                }

                // Ping servers in the background (parallel operation)
                PingServers(serverModels);
            }
            catch (Exception ex)
            {
                await MessageBoxHelper.ShowMessageBox(
                        "Error",
                        "Oops! Something went wrong. Please upload the error log file to GitHub."
                    );

                await FileHelper.LogErrorToFile(ex.Message, "An error has occurred while blocking or unblocking servers.");
            }

            PendingOperation = false;

            ShowProgressBar = false;
        }

    }
}
