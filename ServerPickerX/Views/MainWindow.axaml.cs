using Avalonia.Controls;
using ServerPickerX.Comparer;
using ServerPickerX.ConfigSections;
using ServerPickerX.Helpers;
using ServerPickerX.ViewModels;
using System.ComponentModel;

namespace ServerPickerX.Views
{
    public partial class MainWindow : Window
    {

        private ListSortDirection pingSortDirection = ListSortDirection.Ascending;

        // initialize a static singleton object for handling json settings
        public static JsonSetting jsonSettings = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await jsonSettings.LoadSettings();

            MainWindowViewModel viewModel = new MainWindowViewModel();

            DataContext = await viewModel.LoadServersAsync();

            // unblock all server to sync new data if steam sdr api has been updated
            if (jsonSettings.server_revision != ServerHelper.current_server_revision)
            {
                await viewModel.UnblockAll();
            } else
            {
                await viewModel.PingServers();
            }
        }

        private void TitleBar_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            // prevent other mouse event listeners from being triggered
            e.Handled = true;

            BeginMoveDrag(e);
        }

        private void MinimizeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private async void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            // a cell is double clicked, ping the selected server
            if (e.Source is Border || e.Source is TextBlock || e.Source is Image)
            {
                await ((MainWindowViewModel)DataContext).PingSelectedServer();
            }
        }

        private void DataGridTextColumn_HeaderPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            pingSortDirection = pingSortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            serverList.Columns[3].CustomSortComparer = new PingComparer(pingSortDirection);
        }
    }
}