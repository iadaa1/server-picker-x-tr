using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ServerPickerX.Settings;
using ServerPickerX.Services;
using ServerPickerX.ViewModels;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ServerPickerX;

public partial class SettingsWindow : Window
{
    private readonly JsonSetting _jsonSetting;

    // Parameterless constructor, allows design previewer to instantiate this class since it doesn't support DI
    public SettingsWindow()
    {
        InitializeComponent();

        _jsonSetting = App.ServiceProvider.GetRequiredService<JsonSetting>();
    }

    // DI constructor, allows inversion of control and unit tests mocking
    public SettingsWindow(
        JsonSetting jsonSetting
        )
    {
        InitializeComponent();

        _jsonSetting = jsonSetting;
    }

    private async void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Set data context and version text block value
        await _jsonSetting.LoadSettingsAsync();

        DataContext = App.ServiceProvider.GetRequiredService<SettingsWindowViewModel>();

        VersionTextBlock.Text = "Version: " + Assembly.GetEntryAssembly()!.GetName().Version!.ToString(3);
    }

    private void TitleBar_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        // Prevent other mouse event listeners from being triggered
        e.Handled = true;

        var parentWindow = TopLevel.GetTopLevel(this) as Window;

        parentWindow?.BeginMoveDrag(e);
    }
}