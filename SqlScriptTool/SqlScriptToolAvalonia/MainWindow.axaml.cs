using Avalonia.Controls;
using Avalonia.Interactivity;
using SqlScriptToolAvalonia.AdditionalWindows;
using SqlScriptToolAvalonia.GeneratorWindows;

namespace SqlScriptToolAvalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BtnGenScript_OnClick(object? sender, RoutedEventArgs e)
    {
        var genWindow = new ModelGenerationListWindow();
        genWindow.Show();
    }

    private void MiSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Show();
    }
}