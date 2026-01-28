using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia.Enums;
using ServerPickerX.Helpers;
using ServerPickerX.Models;
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
        public ObservableCollection<ServerModel>? ServerModels { get; set; }

        public ServerModel? SelectedDataGridItem { get; set; }

        public List<Ping> Pings = [];

        // Mvvm tool kit will auto generate source to make this property observable
        // When updating this property, reference it by its auto property name (PascalCase)
        [ObservableProperty]
        public bool showProgressBar;

        public bool PendingOperation = false;

        public async Task<MainWindowViewModel> LoadServersAsync()
        {
            ServerModels = await ServerHelper.LoadServers();

            return this;
        }

        [RelayCommand]
        public async Task PingServers()
        {
            if (ServerModels == null)
            {
                return;
            }

            if (Pings.Count > 0)
            {
                await PingHelper.CancelAllPings(Pings);
            }

            Ping ping = new Ping();

            Pings.Add(ping);

            foreach (ServerModel serverModel in ServerModels) {
                await PingHelper.PingServer(serverModel);
            } 

            ping.Dispose();
        }

        public async Task PingSelectedServer()
        {
            if (SelectedDataGridItem == null)
            {
                return;
            }

            await PingHelper.PingServer(SelectedDataGridItem);
        }

        [RelayCommand]
        public async Task BlockAll()
        {
            if (ServerModels == null || ServerModels.Count == 0)
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
                await MessageBoxHelper.ShowMessageBox("Info", "Please select any server to block");
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
                await MessageBoxHelper.ShowMessageBox("Info", "Please select any server to unblock");
                return;
            }

            var serverModels = new ObservableCollection<ServerModel>(selectedServers.Cast<ServerModel>());

            await performOperation(false, serverModels);

        }

        public async Task performOperation(bool shouldBlock, ObservableCollection<ServerModel> serverModels)
        {
            if (PendingOperation)
            {
                await MessageBoxHelper.ShowMessageBox("Info", "Pending operation. Please wait...");
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
            }
            catch (Exception ex)
            {
                await MessageBoxHelper.ShowMessageBox(
                        "Error",
                        "An error has occured! Please upload generated error file to github.",
                        ButtonEnum.Ok
                    );

                await LogHelper.LogErrorToFile(ex.Message, "An error has occured while blocking or unblocking servers.");
            }

            PendingOperation = false;

            ShowProgressBar = false;
        }

    }
}
