using Avalonia.Controls;
using Avalonia.Interactivity;
using SqlScriptToolAvalonia.AdditionalWindows;
using SqlScriptToolAvalonia.GeneratorWindows;
using SqlScriptToolAvalonia.ConstructorWindows;

namespace SqlScriptToolAvalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BtnConstructor_OnClick(object? sender, RoutedEventArgs e)
    {
        // Открытие окна конструктора (списки объектов и кнопки создания новых объектов)
        var constructorWindow = new ConstructorWindow();
        constructorWindow.Show();
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