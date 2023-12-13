using SqlScriptToolClient.ConstructorStorage;
using System.Windows;

namespace SqlScriptToolClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        StaticConstructorStorage.InitializeStorage();
        StaticConstructorStorage.LoadStorage();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        StaticConstructorStorage.SaveStorage();
    }
}